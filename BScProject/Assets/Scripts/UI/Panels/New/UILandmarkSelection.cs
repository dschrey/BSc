using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[Serializable]
public class LandmarkPlacementData
{
    public int SegmentID;
    public int SelectedObjectID;
    public LandmarkObjectSocket Socket;
    public LineController LineRender;
    public GameObject WorldPositionObject;
    public Color SegmentColor = Color.white;
    public float Distance;

    public LandmarkPlacementData(int ID, Color color)
    {
        SegmentID = ID;
        SelectedObjectID = -1;
        Socket = null;
        LineRender = null;
        WorldPositionObject = null;
        SegmentColor = color;
        Distance = 0f;
    }
}

public class UILandmarkSelection : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private PathLayoutCreator _pathPreviewCreator;
    private CanvasCameraHandler _canvasCamera;

    [Header("Inputs")]
    public NearFarInteractor UIInteractor;
    public InputActionReference PlaceAction;

    [Header("Canvas Landmark Socket")]
    [SerializeField] private GameObject _segmentSelectorPrefab;
    [SerializeField] private Transform _landmarkSocketHolder;
    [SerializeField] private GameObject _landmarkObjectSocketPrefab;
    [SerializeField] private GameObject _placementCrosshairPrefab;
    [SerializeField] private GameObject _worldPreviewObjectPrefab;
    [SerializeField] private RectTransform _movementArea;
    [SerializeField] private ToggleGroup _landmarkSegmentsGroup;

    [Header("Segment Selection")]
    [SerializeField] private Transform _segmentIndicatorParent;
    [SerializeField] private GameObject _segmentIndicatorPrefab;
    [SerializeField] private Button _buttonPreviousSegment;
    [SerializeField] private Button _buttonNextSegment;
    [SerializeField] private TMP_Text _textSelectedSegment;

    [Header("Object Selection")]
    [SerializeField] private Transform _objectSelectionHolder;
    [SerializeField] private GameObject _objectSelectionPrefab;
    [SerializeField] private ToggleGroup _objectSelectionGroup;
    [SerializeField] private TMP_Text _textDistanceValue;
    [SerializeField] private TMP_Text _textObjectName;
    [SerializeField] private TMP_Text _textPreviewDistance;

    [Header("Misc")]
    [SerializeField] private Button _finishButton;
    [SerializeField] private RawImage _selectedPathLayout;
    private bool _completed;

    // Landmark Object Variables
    private readonly List<UISegmentIndicator> _landmarkSegmentIndicators = new();
    private readonly List<GridObjectSelection> _landmarkObjectSelection = new();
    private readonly List<LandmarkPlacementData> _canvasLandmarkObjectData = new();
    private readonly List<CanvasSegmentSelector> _landmarkSegmentSelectors = new();
    private int _selectedLandmarkSegmentID = -1;
    private int _selectedLandmarkObjectID = -1;
    private bool _isValidPlacementPosition;
    private LandmarkPlacementData _currentSegmentLandmarkData => _canvasLandmarkObjectData.Find(s => s.SegmentID == _selectedLandmarkSegmentID);
    private GameObject _placementCrosshair = null;
    private GameObject _placementWorldObject = null;
    private Vector2 _lastPointerPosition = Vector2.negativeInfinity;
    private LineController _lineRender;
    private GameObject _currentLandmarkObjective =>
        _pathPreviewCreator.SpawnedSegments.Find(segment => segment.SegmentID == _selectedLandmarkSegmentID).gameObject;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    #region Unity Methods

    void OnEnable()
    {
        _completed = false;
        _isValidPlacementPosition = false;
        _buttonPreviousSegment.onClick.AddListener(OnPreviousSegmentClick);
        _buttonNextSegment.onClick.AddListener(OnNextSegmentModeClick);

        _finishButton.onClick.AddListener(OnFinishButtonClicked);

        int layoutID = AssessmentManager.Instance.GetSelectedPathLayout();
        _pathPreviewCreator = PathLayoutManager.Instance.GetPathLayout(layoutID);
        _canvasCamera = _pathPreviewCreator.RenderCamera;
        _lineRender = _pathPreviewCreator.DistanceLineController;
        _pathPreviewCreator.ResetCameraView();
        _selectedPathLayout.texture = _pathPreviewCreator.RenderTexture;

        foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
        {
            if (segment.SegmentID == -1) continue;
            UISegmentIndicator landmarkSegmentIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
            _landmarkSegmentIndicators.Add(landmarkSegmentIndicator);
        }

        CreateLandmarkSocketData();
        CreateLandmarkObjectSelectionGrid();
        CreateLandmarkSegmentSelectors();

        _selectedLandmarkSegmentID = 0;
        _landmarkSegmentSelectors.Find(selector => selector.BelongsToSegmentID == _selectedLandmarkSegmentID).Select();
        _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(true);
        UpdateSegmentText();

        CreateLandmarkPositionObjects();

        _textObjectName.text = "None";
        _finishButton.interactable = false;

        // XRInteractionToggle interactionToggle = FindObjectOfType<XRInteractionToggle>();
        // if (interactionToggle != null)
        // {
        //     interactionToggle.UpdateUIInteractor();
        // }
    }


    void Update()
    {
        if (!_completed)
        {
            if (GetInteractorPositionOnCanvas(out Vector2 pointerPosition))
            {
                if (_lastPointerPosition == pointerPosition) return;

                Vector2 roundedResult = new(Mathf.Round(pointerPosition.x / 5f) * 5f, Mathf.Round(pointerPosition.y / 5f) * 5f);
                _lineRender.gameObject.SetActive(true);
                _placementCrosshair.SetActive(true);
                _placementWorldObject.SetActive(true);
                _placementCrosshair.GetComponent<RectTransform>().anchoredPosition = roundedResult;
                Vector3 worldPosition = _canvasCamera.ScreenCoordinatesToWorldSpace(roundedResult);
                worldPosition.y = 0;
                _placementWorldObject.transform.position = worldPosition;
                _lastPointerPosition = roundedResult;
                float distance = Vector3.Distance(_currentLandmarkObjective.transform.position, _placementWorldObject.transform.position);
                _textPreviewDistance.text = distance.ToString("F2", CultureInfo.InvariantCulture) + " m";
                _isValidPlacementPosition = true;
            }
            else
            {
                if (!_isValidPlacementPosition) return;
                _isValidPlacementPosition = false;
                _lineRender.gameObject.SetActive(false);
                _placementCrosshair.SetActive(false);
                _placementWorldObject.SetActive(false);
                _lastPointerPosition = Vector2.negativeInfinity;
                _textPreviewDistance.text = "-:-- m";
            }
        }

        if (PlaceAction != null && _isValidPlacementPosition && PlaceAction.action.WasPressedThisFrame())
        {
            HandleLandmarkPlacement(_lastPointerPosition);
        }
    }

    #endregion
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------
    #region Listener Methods

    private void OnNextSegmentModeClick()
    {
        LandmarkPlacementData oldLandmarkData = _currentSegmentLandmarkData;
        if (oldLandmarkData.WorldPositionObject != null)
            oldLandmarkData.WorldPositionObject.SetActive(false);
        if (oldLandmarkData.LineRender != null)
            oldLandmarkData.LineRender.gameObject.SetActive(false);
        if (oldLandmarkData.Socket != null)
        {
            oldLandmarkData.Socket.ToggleSocketVisual(false);
        }
        _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(false);

        _selectedLandmarkSegmentID = (_selectedLandmarkSegmentID + 1) % (_pathPreviewCreator.SpawnedSegments.Count - 1);
        _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(true);

        _landmarkSegmentSelectors.Find(selector => selector.BelongsToSegmentID == _selectedLandmarkSegmentID).Select();
        UpdateSegmentText();
        HandleLandmarkSegmentChange();
    }

    private void OnPreviousSegmentClick()
    {
        LandmarkPlacementData oldLandmarkData = _currentSegmentLandmarkData;
        if (oldLandmarkData.WorldPositionObject != null)
            oldLandmarkData.WorldPositionObject.SetActive(false);
        if (oldLandmarkData.LineRender != null)
            oldLandmarkData.LineRender.gameObject.SetActive(false);
        if (oldLandmarkData.Socket != null)
        {
            oldLandmarkData.Socket.ToggleSocketVisual(false);
        }
        _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(false);

        _selectedLandmarkSegmentID = (_selectedLandmarkSegmentID - 1 + (_pathPreviewCreator.SpawnedSegments.Count - 1)) % (_pathPreviewCreator.SpawnedSegments.Count - 1);
        _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(true);
        UpdateSegmentText();
        HandleLandmarkSegmentChange();
    }

    private void OnObjectSelectionChanged(int selectedObjectID)
    {
        _selectedLandmarkObjectID = selectedObjectID;
        GameObject landmarkObject = null;
        if (selectedObjectID != -1)
        {
            landmarkObject = ResourceManager.Instance.GetLandmarkObject(_selectedLandmarkObjectID);
            _textObjectName.text = landmarkObject.name.Replace("(Clone)", "").Trim();
        }
        else
        {
            _textObjectName.text = "None";
        }
        if (_selectedLandmarkSegmentID == -1) return;

        LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;
        landmarkData.SelectedObjectID = selectedObjectID;

        LandmarkObjectSocket landmarkSocket = landmarkData.Socket;
        if (landmarkSocket == null) return;
        if (selectedObjectID != -1)
        {
            if (landmarkSocket.IsOccupied)
                landmarkSocket.RemoveSocketObject();
            landmarkSocket.PlaceSocketObject(landmarkObject);
        }
        else
        {
            landmarkSocket.RemoveSocketObject();
            _landmarkSegmentIndicators[_selectedLandmarkSegmentID].SetState(false);
        }

        UpdateObjectAssignment();
    }

    private void OnSelectionPreviewChanged(int previewObjectID)
    {
        HandleSelectionPreview(previewObjectID);
    }

    private void OnSelectionPreviewRemoved()
    {
        HandleSelectionPreview(-1);
    }

    private void HandleSelectionPreview(int previewObjectID)
    {
        if (_selectedLandmarkSegmentID == -1) return;

        LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;
        if (landmarkData == null) return;
        if (landmarkData.Socket == null) return;
        if (landmarkData.SelectedObjectID != -1) return;

        LandmarkObjectSocket landmarkSocket = landmarkData.Socket;
        if (previewObjectID == -1)
        {
            landmarkSocket.RemoveSocketObject();
            _textObjectName.text = "None";
        }
        else
        {
            GameObject landmarkObject = ResourceManager.Instance.GetLandmarkObject(previewObjectID);
            landmarkSocket.PlaceSocketObject(landmarkObject);
            _textObjectName.text = landmarkSocket.SocketObject.name.Replace("(Clone)", "").Trim();
        }
    }

    private void OnFinishButtonClicked()
    {
        _completed = true;
        if (ClearPanel())
        {
            AssessmentManager.Instance.ProceedToNextAssessmentStep();
        }
    }


    #endregion
    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------
    #region Class Methods

    private void HandleLandmarkSegmentChange()
    {
        _lineRender.SetLinePoints(new()
        {
            _placementWorldObject.transform,
            _currentLandmarkObjective.transform
        });

        LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;
        if (landmarkData.Socket != null)
            _currentSegmentLandmarkData.Socket.ToggleSocketVisual(true);
        if (landmarkData.WorldPositionObject != null)
            landmarkData.WorldPositionObject.SetActive(true);
        if (landmarkData.LineRender != null)
            landmarkData.LineRender.gameObject.SetActive(true);

        _textDistanceValue.text = landmarkData.Distance.ToString("F2", CultureInfo.InvariantCulture) + " m";

        if (landmarkData.SelectedObjectID != -1)
        {
            GridObjectSelection selection = _landmarkObjectSelection.Find(sel => sel.ObjectTextureID == landmarkData.SelectedObjectID);
            selection.Select();
            _selectedLandmarkObjectID = landmarkData.SelectedObjectID;
        }
        else
        {
            if (_selectedLandmarkObjectID != -1)
            {
                GridObjectSelection selection = _landmarkObjectSelection.Find(sel => sel.ObjectTextureID == _selectedLandmarkObjectID);
                selection.DeSelect();
                _selectedLandmarkObjectID = -1;
            }
        }
    }

    private void UpdateObjectAssignment()
    {

        _finishButton.interactable = VerifyLandmarkObjects();
        if (_selectedLandmarkSegmentID == -1) return;

        LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;

        if (landmarkData.Socket == null) return;
        if (landmarkData.SelectedObjectID == -1) return;

        Debug.Log($"Landmark: Segment {_selectedLandmarkSegmentID} - Object - {landmarkData.SelectedObjectID}");

        // SegmentObjectData segmentLandmarkObjectData = _pathPreviewCreator.SpawnedSegments.Find(s => s.SegmentID == _selectedLandmarkSegmentID);
        // segmentLandmarkObjectData.AssignedHoverObjectID = landmarkData.SelectedObjectID;
        // segmentLandmarkObjectData.AssignedHoverObjectSocketID = 0;
        AssessmentManager.Instance.AssignSegmentLandmarkObject(_selectedLandmarkSegmentID, landmarkData.SelectedObjectID);

        _textObjectName.text = landmarkData.Socket.SocketObject.name.Replace("(Clone)", "").Trim();


        Vector3 realSpawnpoint = _currentLandmarkObjective.transform.position + AssessmentManager.Instance.CurrentPath.GetSegmentData(_selectedLandmarkSegmentID).RelativeLandmarkPositionToObjective;
        Vector3 objectPosition = landmarkData.WorldPositionObject.transform.position;
        objectPosition.y = 0;
        float differenceToRealPosition = Vector3.Distance(realSpawnpoint, objectPosition);
        AssessmentManager.Instance.SetSegmentLandmarkObjectDistance(_selectedLandmarkSegmentID, landmarkData.Distance, differenceToRealPosition);

        _landmarkSegmentIndicators[_selectedLandmarkSegmentID].SetState(true);
    }

    private void HandleLandmarkPlacement(Vector2 canvasPosition)
    {
        if (_selectedLandmarkSegmentID == -1) return;

        Vector3 worldPosition = _canvasCamera.ScreenCoordinatesToWorldSpace(canvasPosition);
        worldPosition.y = 0;
        LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;
        if (landmarkData.Socket == null)
        {
            landmarkData.Socket = Instantiate(_landmarkObjectSocketPrefab, _landmarkSocketHolder).GetComponent<LandmarkObjectSocket>();
            landmarkData.Socket.Position = canvasPosition;
            landmarkData.Socket.Initialize(landmarkData.SegmentColor);
            landmarkData.WorldPositionObject = Instantiate(_worldPreviewObjectPrefab, worldPosition, Quaternion.identity, _pathPreviewCreator.transform);

            if (_selectedLandmarkObjectID != -1)
            {
                landmarkData.SelectedObjectID = _selectedLandmarkObjectID;
                GameObject landmarkObject = ResourceManager.Instance.GetLandmarkObject(_selectedLandmarkObjectID);
                landmarkData.Socket.PlaceSocketObject(landmarkObject);
            }
        }
        else
        {
            landmarkData.Socket.Position = canvasPosition;
            if (landmarkData.Socket.IsOccupied)
                landmarkData.Socket.RespawnSocketObject();
            landmarkData.WorldPositionObject.transform.position = worldPosition;
        }

        if (landmarkData.LineRender == null)
        {
            landmarkData.LineRender = _pathPreviewCreator.CreateLineController();
        }

        landmarkData.LineRender.SetColor(landmarkData.SegmentColor);
        landmarkData.LineRender.SetLinePoints(new()
            {
                _currentLandmarkObjective.transform,
                landmarkData.WorldPositionObject.transform
            }
        );

        float distance = Vector3.Distance(_currentLandmarkObjective.transform.position, landmarkData.WorldPositionObject.transform.position);
        landmarkData.Distance = distance;
        _textDistanceValue.text = distance.ToString("F2", CultureInfo.InvariantCulture) + " m";
        UpdateObjectAssignment();
    }

    private void CreateLandmarkPositionObjects()
    {
        _placementCrosshair = Instantiate(_placementCrosshairPrefab, _landmarkSocketHolder);
        _placementWorldObject = Instantiate(_worldPreviewObjectPrefab, _pathPreviewCreator.transform);

        _lineRender.SetLinePoints(new()
        {
            _placementWorldObject.transform,
            _currentLandmarkObjective.transform
        });

        _placementWorldObject.transform.position = _currentLandmarkObjective.transform.position;
    }

    private bool VerifyLandmarkObjects()
    {
        foreach (LandmarkPlacementData segment in _canvasLandmarkObjectData)
        {
            if (segment.SelectedObjectID == -1)
                return false;
        }
        return true;
    }

    private void CreateLandmarkSocketData()
    {
        foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
        {
            if (segment.SegmentID == -1) continue;
            Color segmentColor = PathManager.Instance.CurrentPath.Segments.Find(s => s.PathSegmentData.SegmentID == segment.SegmentID).PathSegmentData.SegmentColor;
            _canvasLandmarkObjectData.Add(new LandmarkPlacementData(segment.SegmentID, segmentColor));
        }
    }

    private void CreateLandmarkSegmentSelectors()
    {
        foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
        {
            Vector3 referencePosition = segment.transform.position;
            Vector3 socketWorldPosition = referencePosition;
            Vector2 canvasPosition = _pathPreviewCreator.RenderCamera.WorldCoordinatesToScreenSpace(socketWorldPosition);

            CanvasSegmentSelector segmentSelector = Instantiate(_segmentSelectorPrefab, _landmarkSocketHolder).GetComponent<CanvasSegmentSelector>();
            segmentSelector.Initialize(segment.SegmentID, segment.SegmentColor, _landmarkSegmentsGroup);
            segmentSelector.GetComponent<RectTransform>().anchoredPosition = canvasPosition;
            _landmarkSegmentSelectors.Add(segmentSelector);

        }
    }

    private void CreateLandmarkObjectSelectionGrid()
    {
        foreach (RenderObject obj in ResourceManager.Instance.LandmarkObjects)
        {
            GridObjectSelection selection = Instantiate(_objectSelectionPrefab, _objectSelectionHolder).GetComponent<GridObjectSelection>();
            selection.Initialize(obj.ID, obj.RenderTexture, _objectSelectionGroup);
            selection.SelectedObjectChanged += OnObjectSelectionChanged;
            selection.PreviewObjectChanged += OnSelectionPreviewChanged;
            selection.PreviewObjectRemoved += OnSelectionPreviewRemoved;
            _landmarkObjectSelection.Add(selection);
        }
    }

    private void UpdateSegmentText()
    {
        if (_selectedLandmarkSegmentID == -1)
            _textSelectedSegment.text = "Unset";
        else
            _textSelectedSegment.text = _selectedLandmarkSegmentID.ToString();
    }

    private bool GetInteractorPositionOnCanvas(out Vector2 canvasPosition)
    {
        canvasPosition = Vector2.zero;
        if (UIInteractor.TryGetCurrentUIRaycastResult(out RaycastResult result))
        {
            if (result.gameObject != null && result.gameObject == _movementArea.gameObject)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _movementArea,
                    result.screenPosition,
                    result.module.eventCamera,
                    out canvasPosition
                );
                return true;
            }
        }
        return false;
    }

    private bool ClearPanel()
    {
        _buttonPreviousSegment.onClick.RemoveListener(OnPreviousSegmentClick);
        _buttonNextSegment.onClick.RemoveListener(OnNextSegmentModeClick);

        _finishButton.onClick.RemoveListener(OnFinishButtonClicked);

        foreach (LandmarkPlacementData socket in _canvasLandmarkObjectData)
        {
            socket.Socket.RemoveSocketObject();
            socket.LineRender.ResetPointList();
            Destroy(socket.LineRender.gameObject);
            Destroy(socket.WorldPositionObject);
            Destroy(socket.Socket.gameObject);
        }
        _canvasLandmarkObjectData.Clear();

        foreach (GridObjectSelection gridSelector in _landmarkObjectSelection)
        {
            gridSelector.SelectedObjectChanged -= OnObjectSelectionChanged;
            gridSelector.PreviewObjectChanged -= OnSelectionPreviewChanged;
            gridSelector.PreviewObjectRemoved -= OnSelectionPreviewRemoved;
            Destroy(gridSelector.gameObject);
        }
        _landmarkObjectSelection.Clear();

        _landmarkSegmentSelectors.ForEach(s => Destroy(s.gameObject));
        _landmarkSegmentSelectors.Clear();

        _landmarkSegmentIndicators.ForEach(i => Destroy(i.gameObject));
        _landmarkSegmentIndicators.Clear();

        _lineRender.ResetPointList();
        Destroy(_placementCrosshair);
        Destroy(_placementWorldObject);

        return true;
    }
    
    #endregion
}
