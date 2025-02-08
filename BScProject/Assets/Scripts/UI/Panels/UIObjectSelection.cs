using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegmentSocketSelection
{
    public int SegmentID;
    public int SelectedSocket;
    public List<ObjectSelectionSocket> Sockets = new();

    public SegmentSocketSelection(int ID, List<ObjectSelectionSocket> sockets)
    {
        SegmentID = ID;
        SelectedSocket  = -1;
        Sockets.AddRange(sockets);
    }
}

public class UIObjectSelection : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private PathLayoutCreator _pathPreviewCreator;

    [Header("Socket Interactor")]
    [SerializeField] private GameObject _socketInteractorPrefab;
    [SerializeField] private Transform _socketSelectionHolder;
    public List<SegmentSocketSelection> _hoverObjectSocketData = new();
    public List<SegmentSocketSelection> _langmarkObjectSocketData = new();

    [Header("Selection Mode")]
    [SerializeField] private Transform _segmentIndicatorParent;
    [SerializeField] private GameObject _segmentIndicatorPrefab;
    [SerializeField] private Button _buttonPrevious;
    [SerializeField] private Button _buttonNext;
    [SerializeField] private TMP_Text _selectionModeText;

    [Header("Object Selection")]
    [SerializeField] private Transform _hoverSelectionGrid;
    [SerializeField] private Transform _landmarkSelectionGrid;
    [SerializeField] private GameObject _objectSelectionPrefab;

    [Header("Misc")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private RawImage _selectedPathLayout;

    private readonly List<UIGridObjectSpawner> _selectionHoverObjects = new();
    private readonly List<UIGridObjectSpawner> _selectionLandmarkObjects = new();
    private readonly List<UISegmentIndicator> _taskIndicators = new();
    private enum SelectionMode {Hover = 0, Landmark = 1};
    private SelectionMode _selectionMode;
    private bool _selectionChange = false;
    private bool _buttonCooldown = false;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    void OnEnable() 
    {
        _buttonPrevious.onClick.AddListener(OnPreviousSelectionModeClick);
        _buttonNext.onClick.AddListener(OnNextSelectionModeClick);
        _continueButton.onClick.AddListener(OnContinueButtonClicked);

        UISegmentIndicator hoverObjectsIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
        _taskIndicators.Add(hoverObjectsIndicator);
        UISegmentIndicator landmarkObjectIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
        _taskIndicators.Add(landmarkObjectIndicator);

        _pathPreviewCreator = PathLayoutManager.Instance.GetPathLayout(AssessmentManager.Instance.CurrentPathAssessment.SelectedPathLayoutID);
        _pathPreviewCreator.ResetCameraView();
        _selectedPathLayout.texture = _pathPreviewCreator.RenderTexture;

        _selectionMode = SelectionMode.Hover;
        CreateSocketInteractors();
        StartCoroutine(SpawnInitGrid());
    }

    void OnDisable() 
    {
        foreach (UIGridObjectSpawner selection in _selectionHoverObjects)
        {
            selection.ObjectGrabbed -= OnObjectGrabbed;
            Destroy(selection.gameObject);
        }
        _selectionHoverObjects.Clear();

        foreach (UIGridObjectSpawner selection in _selectionLandmarkObjects)
        {
            selection.ObjectGrabbed -= OnObjectGrabbed;
            Destroy(selection.gameObject);
        }
        _selectionLandmarkObjects.Clear();

        _taskIndicators.ForEach(i => Destroy(i.gameObject));
        _taskIndicators.Clear();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------


    private void OnObjectGrabbed(int selectionID)
    {
        foreach (UIGridObjectSpawner selection in _selectionHoverObjects)
        {
            if (selection.SelectionObjectID == selectionID)
                continue;
            
            if (selection.SpawnedObject != null)
            {
                selection.DisableSelectionObject(2000);
            }
        }
    }

    private void OnContinueButtonClicked()
    {
        foreach (SegmentSocketSelection selection in _langmarkObjectSocketData)
        {
            selection.Sockets.ForEach(socket =>
            {
                socket.OnSocketObjectChanged -= OnSocketObjectChanged;
                socket.DeleteSocketObject();
                Destroy(socket.gameObject);
            });
        }
        _langmarkObjectSocketData.Clear();

        foreach (SegmentSocketSelection selection in _hoverObjectSocketData)
        {
            selection.Sockets.ForEach(socket =>
            {
                socket.OnSocketObjectChanged -= OnSocketObjectChanged;
                socket.DeleteSocketObject();
                Destroy(socket.gameObject);
            });
        }
        _hoverObjectSocketData.Clear(); 

        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private async void OnNextSelectionModeClick()
    {
        if (_buttonCooldown) return;
        UpdateSelectionMode();
        await StartCooldown(500);
    }

    private async void OnPreviousSelectionModeClick()
    {
        if (_buttonCooldown) return;
        UpdateSelectionMode();
        await StartCooldown(500);
    }

    private void OnSocketObjectChanged(int belongsToSegment, int socketID, GameObject socketObject)
    {
        if (_selectionChange) return;

        SegmentSocketSelection segment = null;

        switch (_selectionMode)
        {
            case SelectionMode.Hover:
                segment = _hoverObjectSocketData.Find(s => s.SegmentID == belongsToSegment);
                break;
            case SelectionMode.Landmark:
                segment = _langmarkObjectSocketData.Find(s => s.SegmentID == belongsToSegment);
                break;
        }

        if (segment.SelectedSocket != -1)
        {
            ObjectSelectionSocket oldSocket = segment.Sockets.Find(s => s.SocketID == segment.SelectedSocket);
            oldSocket.isOccupied = false;
            if (oldSocket.SocketObject != null)
                Destroy(oldSocket.SocketObject);
        }

        if (socketObject != null)
        {
            segment.SelectedSocket = socketID;
            SegmentObjectData segmentObjectData = _pathPreviewCreator.SpawnedSegments.Find(s => s.SegmentID == belongsToSegment);

            if (_selectionMode == SelectionMode.Hover)
            {
                segmentObjectData.AssignedHoverObjectSocketID = socketID;
                segmentObjectData.AssignedHoverObjectID = socketObject.GetComponent<RenderObject>().ID;
                AssessmentManager.Instance.AssignSegmentHoverObject(belongsToSegment, socketObject.GetComponent<RenderObject>().ID);
            }
            else 
            {
                segmentObjectData.AssignedLandmarkObjectSocketID = socketID;
                ObjectSelectionSocket socket = segment.Sockets.Find(socket => socket.SocketID == socketID);
                Vector3 canvasPos = socket.GetComponent<RectTransform>().anchoredPosition;
                segmentObjectData.CanvasSocketPosition = canvasPos;
                segmentObjectData.AssignedLandmarkObjectID = socketObject.GetComponent<RenderObject>().ID;
                segmentObjectData.SocketOffsetAngle = socket.angle;
                AssessmentManager.Instance.AssignSegmentLandmarkObject(belongsToSegment, socketObject.GetComponent<RenderObject>().ID);
            }
        }
        else
        {
            segment.SelectedSocket = -1;
            _continueButton.interactable = false;
            SegmentObjectData segmentObjectData = _pathPreviewCreator.SpawnedSegments.Find(s => s.SegmentID == belongsToSegment);
            if (_selectionMode == SelectionMode.Hover)
            {
                segmentObjectData.AssignedHoverObjectSocketID = -1;
                segmentObjectData.AssignedHoverObjectID = -1;
            }
            else
            {
                segmentObjectData.AssignedLandmarkObjectSocketID = -1;
                segmentObjectData.AssignedLandmarkObjectID = -1;
            }
        }

        CheckSelectionState();
        _continueButton.interactable = VerifySelectionValues();
    }

    private void CheckSelectionState()
    {
        switch (_selectionMode)
        {
            case SelectionMode.Hover:
                foreach (SegmentSocketSelection segment in _hoverObjectSocketData)
                {
                    if (segment.SelectedSocket == -1)
                    {
                        _taskIndicators[0].SetState(false);
                        return;
                    }
                }
                _taskIndicators[0].SetState(true);
                break;
            case SelectionMode.Landmark:
                foreach (SegmentSocketSelection segment in _langmarkObjectSocketData)
                {
                    if (segment.SelectedSocket == -1)
                    {
                        _taskIndicators[1].SetState(false);
                        return;
                    }
                }
                _taskIndicators[1].SetState(true);
                break;
        }
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void UpdateSelectionMode()
    {
        _selectionChange = true;
        switch (_selectionMode)
        {
            case SelectionMode.Hover:

                // Cleanup
                foreach (UIGridObjectSpawner selection in _selectionHoverObjects)
                {
                    selection.ObjectGrabbed -= OnObjectGrabbed;
                    Destroy(selection.gameObject);
                }
                _selectionHoverObjects.Clear();

                foreach (SegmentSocketSelection selection in _hoverObjectSocketData)
                {
                    if (selection.SelectedSocket != -1)
                    {
                        ObjectSelectionSocket socket = selection.Sockets.Find(s => s.SocketID == selection.SelectedSocket);
                        socket.DeleteSocketObject();
                    }
                    selection.Sockets.ForEach(s => s.gameObject.SetActive(false));
                }

                // Create Sockets
                _taskIndicators[(int)_selectionMode].Toggle(false);
                _selectionMode = SelectionMode.Landmark;
                _taskIndicators[(int)_selectionMode].Toggle(true);
                if (_langmarkObjectSocketData.Count == 0)
                {
                    CreateSocketInteractors();
                }
                else
                {
                    foreach (SegmentSocketSelection selection in _langmarkObjectSocketData)
                    {
                        selection.Sockets.ForEach(s => s.gameObject.SetActive(true));
                        if (selection.SelectedSocket != -1)
                        {
                            // Put the selected object back in socket
                            ObjectSelectionSocket socket = selection.Sockets.Find(s => s.SocketID == selection.SelectedSocket);
                            SegmentObjectData segmentObjectData = _pathPreviewCreator.SpawnedSegments.Find(s => s.SegmentID == selection.SegmentID);
                            if (segmentObjectData.AssignedLandmarkObjectSocketID != socket.SocketID)
                            {
                                Debug.LogError($"Socket IDs do not align.");
                                return;
                            }
                            GameObject socketObject = ResourceManager.Instance.GetLandmarkObject(segmentObjectData.AssignedLandmarkObjectID);
                            socket.InitializeSocketObject(socketObject);
                        }
                    }
                }
                
                // Create selection grid
                CreateObjectSelectionGrid();
                _selectionModeText.text = "Landmark Objects";
                break;
            case SelectionMode.Landmark:
                // Cleanup
                foreach (UIGridObjectSpawner selection in _selectionLandmarkObjects)
                {
                    selection.ObjectGrabbed -= OnObjectGrabbed;
                    Destroy(selection.gameObject);
                }
                _selectionLandmarkObjects.Clear();
                
                foreach (SegmentSocketSelection selection in _langmarkObjectSocketData)
                {
                    if (selection.SelectedSocket != -1)
                    {
                        ObjectSelectionSocket socket = selection.Sockets.Find(s => s.SocketID == selection.SelectedSocket);
                        socket.DeleteSocketObject();
                    }
                    selection.Sockets.ForEach(s => s.gameObject.SetActive(false));
                }

                // Create Sockets
                _taskIndicators[(int)_selectionMode].Toggle(false);
                _selectionMode = SelectionMode.Hover;
                _taskIndicators[(int)_selectionMode].Toggle(true);
                if (_hoverObjectSocketData.Count == 0)
                {
                    CreateSocketInteractors();
                }
                else
                {
                    foreach (SegmentSocketSelection selection in _hoverObjectSocketData)
                    {
                        selection.Sockets.ForEach(s => s.gameObject.SetActive(true));
                        if (selection.SelectedSocket != -1)
                        {
                            // Put the selected object back in socket
                            ObjectSelectionSocket socket = selection.Sockets.Find(s => s.SocketID == selection.SelectedSocket);
                            SegmentObjectData segmentObjectData = _pathPreviewCreator.SpawnedSegments.Find(s => s.SegmentID == selection.SegmentID);
                            if (segmentObjectData.AssignedHoverObjectSocketID != socket.SocketID)
                            {
                                Debug.LogError($"Socket IDs do not align.");
                                return;
                            }
                            GameObject socketObject = ResourceManager.Instance.GetHoverObject(segmentObjectData.AssignedHoverObjectID);
                            socket.InitializeSocketObject(socketObject);
                        }
                    }
                }

                // Create selection grid
                CreateObjectSelectionGrid();
                _selectionModeText.text = "Hover Objects";
                break;
        }
        _selectionChange = false;
    }

    private bool VerifySelectionValues()
    {
        foreach (SegmentSocketSelection segment in _hoverObjectSocketData)
        {
            if (segment.SelectedSocket == -1)
                return false;
        }
        if (_langmarkObjectSocketData.Count == 0)
            return false;
        foreach (SegmentSocketSelection segment in _langmarkObjectSocketData)
        {
            if (segment.SelectedSocket == -1)
                return false;
        }
        return true;
    }

    public void ResetPanelData()
    {
    }

    private void CreateSocketInteractors()
    {
        List<Vector3> offsets = new();
        float offsetDistance = 0.5f;

        if (_selectionMode == SelectionMode.Landmark)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                float radians = Mathf.Deg2Rad * angle;
                // Compute the circular offset.
                float offsetX = Mathf.Sin(radians) * offsetDistance;
                float offsetZ = Mathf.Cos(radians) * offsetDistance;
                Vector3 offset = new(offsetX, 0, offsetZ);
                
                // Map the circular offset to a square:
                float maxComponent = Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.z));
                if (maxComponent > 0)
                {
                    offset = offset / maxComponent * offsetDistance;
                }
                
                offsets.Add(offset);
            }
        }
        else if (_selectionMode == SelectionMode.Hover)
        {
            offsets.Add(new Vector3(0, 0, 0));
        }


        foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
        {
            if (segment.SegmentID == -1)
                continue;

            Vector3 referenceLocalPosition = segment.transform.localPosition;
            Vector3 referencePosition = segment.transform.position;

            List<ObjectSelectionSocket> sockets = new();
            foreach (Vector3 offset in offsets)
            {
                Vector3 localWorldPosition = referenceLocalPosition + offset;
                if (! ValidatePosition(localWorldPosition))
                {
                    continue;
                }

                Vector3 socketWorldPosition = referencePosition + offset;
                Vector2 canvasPosition = _pathPreviewCreator.RenderCamera.WorldCoordinatesToScreenSpace(socketWorldPosition);
                
                float angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360;
                if (angle >= 360) angle -= 360;

                ObjectSelectionSocket socketInteractor = Instantiate(_socketInteractorPrefab, _socketSelectionHolder.transform).GetComponent<ObjectSelectionSocket>();
                Color segmentColor = PathManager.Instance.CurrentPath.Segments.Find(s => s.PathSegmentData.SegmentID == segment.SegmentID).PathSegmentData.SegmentColor;
                socketInteractor.Initialize(segment.SegmentID, sockets.Count(), segmentColor);
                socketInteractor.angle = angle;
                socketInteractor.OnSocketObjectChanged += OnSocketObjectChanged;
                RectTransform socketRect = socketInteractor.GetComponent<RectTransform>();
                socketRect.anchoredPosition = canvasPosition;

                sockets.Add(socketInteractor);
            }

            if (_selectionMode == SelectionMode.Landmark)
            {
                _langmarkObjectSocketData.Add(new SegmentSocketSelection(segment.SegmentID, sockets));
            }
            else if (_selectionMode == SelectionMode.Hover)
            {
                _hoverObjectSocketData.Add(new SegmentSocketSelection(segment.SegmentID, sockets));
            }
        }
    }


    private bool ValidatePosition(Vector3 position)
    {
        float halfWidth = DataManager.Instance.Settings.MovementArea.x / 2;
        float halfHeight = DataManager.Instance.Settings.MovementArea.y / 2;

        if (position.z < - halfWidth || position.z > halfWidth)
        {
            return false;
        }
        else if(position.x < - halfHeight || position.x > halfHeight )
        {
            return false;
        }

        return true;
    }

    private void CreateHoverObjectSelectionGrid()
    {
        foreach (RenderObject obj in ResourceManager.Instance.HoverObjects)
        {
            UIGridObjectSpawner objectSelection = Instantiate(_objectSelectionPrefab, _hoverSelectionGrid).GetComponent<UIGridObjectSpawner>();
            objectSelection.Initialize(obj.ID, obj.RenderTexture);
            objectSelection.ObjectGrabbed += OnObjectGrabbed;
            _selectionHoverObjects.Add(objectSelection);
            Canvas.ForceUpdateCanvases();
            objectSelection.SpawnObject(obj.gameObject);
        }
    }

    private void CreateLandmarkObjectSelectionGrid()
    {
        foreach (RenderObject obj in ResourceManager.Instance.LandmarkObjects)
        {
            UIGridObjectSpawner objectSelection = Instantiate(_objectSelectionPrefab, _landmarkSelectionGrid).GetComponent<UIGridObjectSpawner>();
            objectSelection.Initialize(obj.ID, obj.RenderTexture);
            objectSelection.ObjectGrabbed += OnObjectGrabbed;
            _selectionLandmarkObjects.Add(objectSelection);
            Canvas.ForceUpdateCanvases();
            objectSelection.SpawnObject(obj.gameObject);
        }
    }

    private void CreateObjectSelectionGrid()
    {

        if (_selectionMode == SelectionMode.Hover)
        {
            CreateHoverObjectSelectionGrid();
        }
        else
        {
            CreateLandmarkObjectSelectionGrid();
        }
    }

    private IEnumerator SpawnInitGrid()
    {
        yield return new WaitForSeconds(0.05f);
        CreateObjectSelectionGrid();
        _taskIndicators[(int)_selectionMode].Toggle(true);
        _selectionModeText.text = _selectionMode == SelectionMode.Hover ? "Hover Objects" : "Landmark Objects";
    }

    private async Task StartCooldown(int cd)
    {
        _buttonCooldown = true;
        await Task.Delay(cd); 
        _buttonCooldown = false;
    }
}
