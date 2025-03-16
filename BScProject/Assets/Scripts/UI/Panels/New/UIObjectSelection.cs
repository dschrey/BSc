using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// [Serializable]
// public class SegmentSocketSelection
// {
//     public int SegmentID;
//     public int SelectedObjectID;
//     public ObjectPlacementSocket Socket;

//     public SegmentSocketSelection(int ID, ObjectPlacementSocket socket)
//     {
//         SegmentID = ID;
//         SelectedObjectID = -1;
//         Socket = socket;
//     }
// }

// [Serializable]
// public class LandmarkPlacementData
// {
//     public int SegmentID;
//     public int SelectedObjectID;
//     public LandmarkObjectSocket Socket;
//     public LineController LineRender;
//     public GameObject WorldPositionObject;
//     public Color SegmentColor = Color.white;
//     public float Distance;

//     public LandmarkPlacementData(int ID, Color color)
//     {
//         SegmentID = ID;
//         SelectedObjectID = -1;
//         Socket = null;
//         LineRender = null;
//         WorldPositionObject = null;
//         SegmentColor = color;
//         Distance = 0f;
//     }
// }

public class UIObjectSelection : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private PathLayoutCreator _pathPreviewCreator;
    private CanvasCameraHandler _canvasCamera;

    // [Header("Inputs")]
    // [SerializeField] public NearFarInteractor UIInteractor;
    // [SerializeField] public InputActionReference PlaceAction;

    [Header("Canvas Objective Socket")]
    [SerializeField] private Transform _objectiveSocketHolder;
    [SerializeField] private GameObject _objectiveSocketPrefab;
    private readonly List<SegmentSocketSelection> _canvasHoverObjectData = new();
    [SerializeField] private ToggleGroup _hoverSegmentsGroup;

    // [Header("Canvas Landmark Object Placement")]
    // [SerializeField] private GameObject _segmentSelectorPrefab;
    // private readonly List<CanvasSegmentSelector> _landmarkSegmentSelectors = new();
    // [SerializeField] private Transform _landmarkSocketHolder;
    // [SerializeField] private GameObject _landmarkObjectSocketPrefab;
    // [SerializeField] private GameObject _placementCrosshairPrefab;
    // [SerializeField] private GameObject _worldPreviewObjectPrefab;
    // [SerializeField] private RectTransform _movementArea;
    // private readonly List<LandmarkPlacementData> _canvasLandmarkObjectData = new();
    // [SerializeField] private ToggleGroup _landmarkSegmentsGroup;

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
    [SerializeField] private GameObject _distanceInfo;
    [SerializeField] private TMP_Text _textObjectName;

    [Header("Misc")]
    [SerializeField] private Button _nextButton;
    // [SerializeField] private Button _finishButton;
    [SerializeField] private RawImage _selectedPathLayout;
    private bool _selectionChange;

    // Hover Objects Variables
    private readonly List<UISegmentIndicator> _hoverSegmentIndicators = new();
    private readonly List<GridObjectSelection> _hoverObjectSelection = new();
    private int _selectedHoverSegmentID = -1;
    private int _selectedHoverObjectID = -1;
    private SegmentSocketSelection _currentSegmentHoverData => _canvasHoverObjectData.Find(s => s.SegmentID == _selectedHoverSegmentID);

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    #region Unity Methods
    void OnEnable()
    {
        _selectionChange = false;
        _buttonPreviousSegment.onClick.AddListener(OnPreviousSegmentClick);
        _buttonNextSegment.onClick.AddListener(OnNextSegmentModeClick);

        _nextButton.onClick.AddListener(OnNextObjectTypeClicked);
        // _finishButton.onClick.AddListener(OnFinishButtonClicked);

        int layoutID = AssessmentManager.Instance.GetSelectedPathLayout();
        _pathPreviewCreator = PathLayoutManager.Instance.GetPathLayout(layoutID);
        _canvasCamera = _pathPreviewCreator.RenderCamera;
        _pathPreviewCreator.ResetCameraView();
        _selectedPathLayout.texture = _pathPreviewCreator.RenderTexture;

        foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
        {
            if (segment.SegmentID == -1) continue;
            UISegmentIndicator hoverSegmentIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
            _hoverSegmentIndicators.Add(hoverSegmentIndicator);
        }

        _distanceInfo.SetActive(false);
        SetSegmentIndicators();
        CreateObjectiveCanvasSockets();
        // CreateLandmarkSocketData();
        CreateHoverObjectSelectionGrid();
        UpdateSegmentText();

        XRInteractionToggle interactionToggle = FindObjectOfType<XRInteractionToggle>();
        if (interactionToggle != null)
        {
            interactionToggle.UpdateUIInteractor();
        }
    }

    void OnDisable()
    {
        _buttonPreviousSegment.onClick.RemoveListener(OnPreviousSegmentClick);
        _buttonNextSegment.onClick.RemoveListener(OnNextSegmentModeClick);

        _nextButton.onClick.RemoveListener(OnNextObjectTypeClicked);
        // _finishButton.onClick.RemoveListener(OnFinishButtonClicked);

        foreach (SegmentSocketSelection socket in _canvasHoverObjectData)
        {
            socket.Socket.RemoveSocketObject();
            Destroy(socket.Socket.gameObject);
        }
        _canvasHoverObjectData.Clear();

        // foreach (LandmarkPlacementData socket in _canvasLandmarkObjectData)
        // {
        //     socket.Socket.RemoveSocketObject();
        //     socket.LineRender.ResetPointList();
        //     Destroy(socket.LineRender.gameObject);
        //     Destroy(socket.WorldPositionObject);
        //     Destroy(socket.Socket.gameObject);
        // }
        // _canvasLandmarkObjectData.Clear();

        ClearGridSelection(_hoverObjectSelection);

        _hoverSegmentIndicators.ForEach(i => Destroy(i.gameObject));
        _hoverSegmentIndicators.Clear();
    }

    // void Update()
    // {
    //     if (!_completed && _selectionMode == SelectionType.Landmark)
    //     {
    //         if (GetInteractorPositionOnCanvas(out Vector2 pointerPosition))
    //         {
    //             if (_lastPointerPosition == pointerPosition) return;

    //             Vector2 roundedResult = new(Mathf.Round(pointerPosition.x / 5f) * 5f, Mathf.Round(pointerPosition.y / 5f) * 5f);
    //             _lineRender.gameObject.SetActive(true);
    //             _placementCrosshair.SetActive(true);
    //             _placementWorldObject.SetActive(true);
    //             _placementCrosshair.GetComponent<RectTransform>().anchoredPosition = roundedResult;
    //             Vector3 worldPosition = _canvasCamera.ScreenCoordinatesToWorldSpace(roundedResult);
    //             worldPosition.y = 0;
    //             _placementWorldObject.transform.position = worldPosition;
    //             _lastPointerPosition = roundedResult;
    //             float distance = Vector3.Distance(_currentLandmarkObjective.transform.position, _placementWorldObject.transform.position);
    //             _textPreviewDistance.text = distance.ToString("F2", CultureInfo.InvariantCulture) + " m";
    //             _isValidPlacementPosition = true;
    //         }
    //         else
    //         {
    //             if (!_isValidPlacementPosition) return;
    //             _isValidPlacementPosition = false;
    //             _lineRender.gameObject.SetActive(false);
    //             _placementCrosshair.SetActive(false);
    //             _placementWorldObject.SetActive(false);
    //             _lastPointerPosition = Vector2.negativeInfinity;
    //             _textPreviewDistance.text = "-:-- m";
    //         }
    //     }

    //     if (PlaceAction != null && _isValidPlacementPosition && PlaceAction.action.WasPressedThisFrame())
    //     {
    //         HandleLandmarkPlacement(_lastPointerPosition);
    //     }
    // }

    #endregion
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------
    #region Listener Methods    
    private void OnNextSegmentModeClick()
    {
        // if (_selectionMode == SelectionType.Hover)
        // {
            _selectedHoverSegmentID = (_selectedHoverSegmentID + 1) % (_pathPreviewCreator.SpawnedSegments.Count - 1);
            SegmentSocketSelection hoverData = _currentSegmentHoverData;
            hoverData.Socket.Select();
            if (hoverData.SelectedObjectID != -1)
            {
                _selectedHoverObjectID = hoverData.SelectedObjectID;
                GridObjectSelection selection = _hoverObjectSelection.Find(sel => sel.ObjectTextureID == hoverData.SelectedObjectID);
                selection.Select();
            }
        // }
        // else
        // {
        //     LandmarkPlacementData oldLandmarkData = _currentSegmentLandmarkData;
        //     if (oldLandmarkData.WorldPositionObject != null)
        //         oldLandmarkData.WorldPositionObject.SetActive(false);
        //     if (oldLandmarkData.LineRender != null)
        //         oldLandmarkData.LineRender.gameObject.SetActive(false);
        //     if (oldLandmarkData.Socket != null)
        //     {
        //         oldLandmarkData.Socket.ToggleSocketVisual(false);
        //     }
        //     _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(false);

        //     _selectedLandmarkSegmentID = (_selectedLandmarkSegmentID + 1) % (_pathPreviewCreator.SpawnedSegments.Count - 1);
        //     _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(true);

        //     _landmarkSegmentSelectors.Find(selector => selector.BelongsToSegmentID == _selectedLandmarkSegmentID).Select();
        //     UpdateSegmentText();
        //     HandleLandmarkSegmentChange();
        // }
    }

    private void OnPreviousSegmentClick()
    {
        // if (_selectionMode == SelectionType.Hover)
        // {
            _selectedHoverSegmentID = (_selectedHoverSegmentID - 1 + (_pathPreviewCreator.SpawnedSegments.Count - 1)) % (_pathPreviewCreator.SpawnedSegments.Count - 1);
            SegmentSocketSelection hoverData = _currentSegmentHoverData;
            hoverData.Socket.Select();
            if (hoverData.SelectedObjectID != -1)
            {
                GridObjectSelection selection = _hoverObjectSelection.Find(sel => sel.ObjectTextureID == hoverData.SelectedObjectID);
                selection.Select();
            }
        // }
        // else
        // {
        //     LandmarkPlacementData oldLandmarkData = _currentSegmentLandmarkData;
        //     if (oldLandmarkData.WorldPositionObject != null)
        //         oldLandmarkData.WorldPositionObject.SetActive(false);
        //     if (oldLandmarkData.LineRender != null)
        //         oldLandmarkData.LineRender.gameObject.SetActive(false);
        //     if (oldLandmarkData.Socket != null)
        //     {
        //         oldLandmarkData.Socket.ToggleSocketVisual(false);
        //     }
        //     _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(false);

        //     _selectedLandmarkSegmentID = (_selectedLandmarkSegmentID - 1 + (_pathPreviewCreator.SpawnedSegments.Count - 1)) % (_pathPreviewCreator.SpawnedSegments.Count - 1);
        //     _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(true);
        //     UpdateSegmentText();
        //     HandleLandmarkSegmentChange();
        // }
    }

    private void OnObjectSelectionChanged(int selectedObjectID)
    {
        // switch (_selectionMode)
        // {
        //     case SelectionType.Hover:
                _selectedHoverObjectID = selectedObjectID;
                GameObject hoverObject = null;
                if (selectedObjectID != -1)
                {
                    hoverObject = ResourceManager.Instance.GetHoverObject(_selectedHoverObjectID);
                    _textObjectName.text = hoverObject.name.Replace("(Clone)", "").Trim();
                }
                else
                {
                    _textObjectName.text = "None";
                }
                if (_selectedHoverSegmentID == -1) return;

                SegmentSocketSelection hoverData = _currentSegmentHoverData;
                if (hoverData == null) return;

                hoverData.SelectedObjectID = selectedObjectID;

                if (selectedObjectID != -1)
                {
                    ObjectPlacementSocket hoverSocket = hoverData.Socket;
                    if (hoverSocket == null) return;
                    if (hoverSocket.IsOccupied)
                        hoverSocket.RemoveSocketObject();
                    hoverSocket.PlaceSocketObject(hoverObject);
                }
                else
                {
                    _hoverSegmentIndicators[_selectedHoverSegmentID].SetState(false);
                }

                UpdateObjectAssignment();
        //         break;
        //     case SelectionType.Landmark:
        //         _selectedLandmarkObjectID = selectedObjectID;
        //         GameObject landmarkObject = null;
        //         if (selectedObjectID != -1)
        //         {
        //             landmarkObject = ResourceManager.Instance.GetLandmarkObject(_selectedLandmarkObjectID);
        //             _textObjectName.text = landmarkObject.name.Replace("(Clone)", "").Trim();
        //         }
        //         else
        //         {
        //             _textObjectName.text = "None";
        //         }
        //         if (_selectedLandmarkSegmentID == -1) return;

        //         LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;
        //         landmarkData.SelectedObjectID = selectedObjectID;

        //         LandmarkObjectSocket landmarkSocket = landmarkData.Socket;
        //         if (landmarkSocket == null) return;
        //         if (selectedObjectID != -1)
        //         {
        //             if (landmarkSocket.IsOccupied)
        //                 landmarkSocket.RemoveSocketObject();
        //             landmarkSocket.PlaceSocketObject(landmarkObject);
        //         }
        //         else
        //         {
        //             landmarkSocket.RemoveSocketObject();
        //             _landmarkSegmentIndicators[_selectedLandmarkSegmentID].SetState(false);
        //         }

        //         UpdateObjectAssignment();
        //         break;
        // }
    }

    private void OnSelectionPreviewChanged(int previewObjectID)
    {
        HandleSelectionPreview(previewObjectID);
    }

    private void OnSelectionPreviewRemoved()
    {
        HandleSelectionPreview(-1);
    }

    private void OnSocketSelectionChanged(int segmentID, int socketID)
    {
        if (_selectionChange) return;
        _selectedHoverSegmentID = segmentID;
        _hoverSegmentIndicators[_selectedHoverSegmentID].Toggle(true);
        UpdateSegmentText();

        SegmentSocketSelection hoverSegment = _currentSegmentHoverData;
        if (hoverSegment == null) return;

        if (socketID == -1)
        {
            _hoverSegmentIndicators[_selectedHoverSegmentID].Toggle(false);
            _selectedHoverSegmentID = -1;
            UpdateSegmentText();
            if (hoverSegment.SelectedObjectID != -1)
            {
                GridObjectSelection objectSelection = _hoverObjectSelection.Find(s => s.ObjectTextureID == hoverSegment.SelectedObjectID);
                objectSelection.DeSelect();
            }
            return;
        }

        if (hoverSegment.SelectedObjectID != -1)
        {
            GridObjectSelection objectSelection = _hoverObjectSelection.Find(s => s.ObjectTextureID == hoverSegment.SelectedObjectID);
            objectSelection.Select();
        }
        else if (_selectedHoverObjectID != -1)
        {
            hoverSegment.SelectedObjectID = _selectedHoverObjectID;
            UpdateObjectAssignment();
        }
    }

    private void OnSocketPreviewChanged(int segmentID, int socketID)
    {
        if (_selectedHoverSegmentID != -1) return;
        if (_selectedHoverObjectID == -1) return;

        SegmentSocketSelection hoverSegment = _canvasHoverObjectData.Find(s => s.SegmentID == segmentID);
        if (hoverSegment == null) return;
        if (hoverSegment.SelectedObjectID != -1) return;

        ObjectPlacementSocket hoverSocket = hoverSegment.Socket;
        if (hoverSocket == null) return;
        GameObject hoverObject = ResourceManager.Instance.GetHoverObject(_selectedHoverObjectID);
        if (hoverSocket.IsOccupied)
            hoverSocket.RemoveSocketObject();
        hoverSocket.PlaceSocketObject(hoverObject);
    }

    private void OnSocketPreviewRemoved(int segmentID, int socketID)
    {
        if (_selectedHoverSegmentID != -1) return;
        if (_selectedHoverObjectID == -1) return;

        SegmentSocketSelection hoverSegment = _canvasHoverObjectData.Find(s => s.SegmentID == segmentID);
        if (hoverSegment == null) return;
        if (hoverSegment.SelectedObjectID != -1) return;

        ObjectPlacementSocket hoverSocket = hoverSegment.Socket;
        if (hoverSocket == null) return;
        hoverSocket.RemoveSocketObject();
    }

    private void UpdateObjectAssignment()
    {
        // switch (_selectionMode)
        // {
        //     case SelectionType.Hover:
                _nextButton.interactable = VerifyHoverObjects();
                if (_selectedHoverSegmentID == -1) return;

                SegmentSocketSelection hoverSelection = _currentSegmentHoverData;

                if (hoverSelection.SelectedObjectID == -1) return;

                Debug.Log($"Hover: Segment {_selectedHoverSegmentID} -  Object - {hoverSelection.SelectedObjectID}");

                SegmentObjectData segmentHoverObjectData = _pathPreviewCreator.SpawnedSegments.Find(s => s.SegmentID == _selectedHoverSegmentID);
                segmentHoverObjectData.AssignedHoverObjectID = hoverSelection.SelectedObjectID;
                AssessmentManager.Instance.AssignSegmentHoverObject(_selectedHoverSegmentID, hoverSelection.SelectedObjectID);

                ObjectPlacementSocket hoverSocket = hoverSelection.Socket;
                if (!hoverSocket.IsOccupied)
                {
                    GameObject hoverObject = ResourceManager.Instance.GetHoverObject(_selectedHoverObjectID);
                    hoverSocket.PlaceSocketObject(hoverObject);
                }

                _textObjectName.text = hoverSocket.SocketObject.name.Replace("(Clone)", "").Trim();
                _hoverSegmentIndicators[_selectedHoverSegmentID].SetState(true);
                _selectedHoverObjectID = -1;

        //         break;
        //     case SelectionType.Landmark:
        //         _finishButton.interactable = VerifyLandmarkObjects();
        //         if (_selectedLandmarkSegmentID == -1) break;

        //         LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;

        //         if (landmarkData.Socket == null) break;
        //         if (landmarkData.SelectedObjectID == -1) break;

        //         Debug.Log($"Landmark: Segment {_selectedLandmarkSegmentID} - Object - {landmarkData.SelectedObjectID}");

        //         SegmentObjectData segmentLandmarkObjectData = _pathPreviewCreator.SpawnedSegments.Find(s => s.SegmentID == _selectedLandmarkSegmentID);
        //         segmentLandmarkObjectData.AssignedHoverObjectID = landmarkData.SelectedObjectID;
        //         segmentLandmarkObjectData.AssignedHoverObjectSocketID = 0;
        //         AssessmentManager.Instance.AssignSegmentLandmarkObject(_selectedLandmarkSegmentID, landmarkData.SelectedObjectID);

        //         _textObjectName.text = landmarkData.Socket.SocketObject.name.Replace("(Clone)", "").Trim();


        //         Vector3 realSpawnpoint = _currentLandmarkObjective.transform.position + AssessmentManager.Instance.CurrentPath.GetSegmentData(_selectedLandmarkSegmentID).RelativeLandmarkPositionToObjective;
        //         Vector3 objectPosition = landmarkData.WorldPositionObject.transform.position;
        //         objectPosition.y = 0;
        //         float differenceToRealPosition = Vector3.Distance(realSpawnpoint, objectPosition);
        //         AssessmentManager.Instance.SetSegmentLandmarkObjectDistance(_selectedLandmarkSegmentID, landmarkData.Distance, differenceToRealPosition);

        //         _landmarkSegmentIndicators[_selectedLandmarkSegmentID].SetState(true);
        //         break;
        // }
    }

    // private void OnFinishButtonClicked()
    // {
    //     _completed = true;
    //     if (ClearUIData())
    //     {
    //         AssessmentManager.Instance.ProceedToNextAssessmentStep();
    //     }
    // }

    private void OnNextObjectTypeClicked()
    {
        // TODO Switch to next panel
        // UpdateSelectionMode();
    }

    #endregion
    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------
    #region Class Methods

    // private void HandleLandmarkSegmentChange()
    // {
    //     _lineRender.SetLinePoints(new()
    //     {
    //         _placementWorldObject.transform,
    //         _currentLandmarkObjective.transform
    //     });

    //     LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;
    //     if (landmarkData.Socket != null)
    //         _currentSegmentLandmarkData.Socket.ToggleSocketVisual(true);
    //     if (landmarkData.WorldPositionObject != null)
    //         landmarkData.WorldPositionObject.SetActive(true);
    //     if (landmarkData.LineRender != null)
    //         landmarkData.LineRender.gameObject.SetActive(true);

    //     _textDistanceValue.text = landmarkData.Distance.ToString("F2", CultureInfo.InvariantCulture) + " m";

    //     if (landmarkData.SelectedObjectID != -1)
    //     {
    //         GridObjectSelection selection = _landmarkObjectSelection.Find(sel => sel.ObjectTextureID == landmarkData.SelectedObjectID);
    //         selection.Select();
    //         _selectedLandmarkObjectID = landmarkData.SelectedObjectID;
    //     }
    //     else
    //     {
    //         if (_selectedLandmarkObjectID != -1)
    //         {
    //             GridObjectSelection selection = _landmarkObjectSelection.Find(sel => sel.ObjectTextureID == _selectedLandmarkObjectID);
    //             selection.DeSelect();
    //             _selectedLandmarkObjectID = -1;
    //         }
    //     }
    // }

    private void HandleSelectionPreview(int previewObjectID)
    {
        // switch (_selectionMode)
        // {
        //     case SelectionType.Hover:
                if (_selectedHoverSegmentID == -1) return;

                SegmentSocketSelection hoverData = _currentSegmentHoverData;
                if (hoverData == null) return;
                if (hoverData.SelectedObjectID != -1) return;

                ObjectPlacementSocket hoverSocket = hoverData.Socket;
                if (previewObjectID == -1)
                {
                    hoverSocket.RemoveSocketObject();
                    _textObjectName.text = "None";
                }
                else
                {
                    GameObject hoverObject = ResourceManager.Instance.GetHoverObject(previewObjectID);
                    hoverSocket.PlaceSocketObject(hoverObject);
                    _textObjectName.text = hoverData.Socket.SocketObject.name.Replace("(Clone)", "").Trim();
                }
        //         break;
        //     case SelectionType.Landmark:
        //         if (_selectedLandmarkSegmentID == -1) return;

        //         LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;
        //         if (landmarkData == null) return;
        //         if (landmarkData.Socket == null) return;
        //         if (landmarkData.SelectedObjectID != -1) return;

        //         LandmarkObjectSocket landmarkSocket = landmarkData.Socket;
        //         if (previewObjectID == -1)
        //         {
        //             landmarkSocket.RemoveSocketObject();
        //             _textObjectName.text = "None";
        //         }
        //         else
        //         {
        //             GameObject landmarkObject = ResourceManager.Instance.GetLandmarkObject(previewObjectID);
        //             landmarkSocket.PlaceSocketObject(landmarkObject);
        //             _textObjectName.text = landmarkSocket.SocketObject.name.Replace("(Clone)", "").Trim();
        //         }
        //         break;
        // }
    }

    // private void HandleLandmarkPlacement(Vector2 canvasPosition)
    // {
    //     if (_selectedLandmarkSegmentID == -1) return;

    //     Vector3 worldPosition = _canvasCamera.ScreenCoordinatesToWorldSpace(canvasPosition);
    //     worldPosition.y = 0;
    //     LandmarkPlacementData landmarkData = _currentSegmentLandmarkData;
    //     if (landmarkData.Socket == null)
    //     {
    //         landmarkData.Socket = Instantiate(_landmarkObjectSocketPrefab, _landmarkSocketHolder).GetComponent<LandmarkObjectSocket>();
    //         landmarkData.Socket.Position = canvasPosition;
    //         landmarkData.Socket.Initialize(landmarkData.SegmentColor);
    //         landmarkData.WorldPositionObject = Instantiate(_worldPreviewObjectPrefab, worldPosition, Quaternion.identity, _pathPreviewCreator.transform);

    //         if (_selectedLandmarkObjectID != -1)
    //         {
    //             landmarkData.SelectedObjectID = _selectedLandmarkObjectID;
    //             GameObject landmarkObject = ResourceManager.Instance.GetLandmarkObject(_selectedLandmarkObjectID);
    //             landmarkData.Socket.PlaceSocketObject(landmarkObject);
    //         }
    //     }
    //     else
    //     {
    //         landmarkData.Socket.Position = canvasPosition;
    //         if (landmarkData.Socket.IsOccupied)
    //             landmarkData.Socket.RespawnSocketObject();
    //         landmarkData.WorldPositionObject.transform.position = worldPosition;
    //     }

    //     if (landmarkData.LineRender == null)
    //     {
    //         landmarkData.LineRender = _pathPreviewCreator.CreateLineController();
    //     }

    //     landmarkData.LineRender.SetColor(landmarkData.SegmentColor);
    //     landmarkData.LineRender.SetLinePoints(new()
    //         {
    //             _currentLandmarkObjective.transform,
    //             landmarkData.WorldPositionObject.transform
    //         }
    //     );

    //     float distance = Vector3.Distance(_currentLandmarkObjective.transform.position, landmarkData.WorldPositionObject.transform.position);
    //     landmarkData.Distance = distance;
    //     _textDistanceValue.text = distance.ToString("F2", CultureInfo.InvariantCulture) + " m";
    //     UpdateObjectAssignment();
    // }

    // private void UpdateSelectionMode()
    // {
    //     _selectionChange = true;

    //     // Cleanup
    //     foreach (SegmentSocketSelection socket in _canvasHoverObjectData)
    //     {
    //         socket.Socket.RemoveSocketObject();
    //         Destroy(socket.Socket.gameObject);
    //     }
    //     _canvasHoverObjectData.Clear();

    //     ClearGridSelection(_hoverObjectSelection);


    //     // Create Sockets
    //     _selectionMode = SelectionType.Landmark;

    //     SetSegmentIndicators();
    //     _hoverSegmentIndicators.ForEach(i => Destroy(i.gameObject));
    //     _hoverSegmentIndicators.Clear();

    //     CreateLandmarkObjectSelectionGrid();
    //     CreateLandmarkSegmentSelectors();

    //     if (_placementCrosshair == null)
    //     {
    //         _placementCrosshair = Instantiate(_placementCrosshairPrefab, _landmarkSocketHolder);
    //         _placementWorldObject = Instantiate(_worldPreviewObjectPrefab, _pathPreviewCreator.transform);
    //     }
    //     else
    //     {
    //         _placementCrosshair.SetActive(true);
    //         _placementWorldObject.SetActive(true);
    //     }

    //     _selectedLandmarkSegmentID = 0;
    //     _landmarkSegmentSelectors.Find(selector => selector.BelongsToSegmentID == _selectedLandmarkSegmentID).Select();
    //     _landmarkSegmentIndicators[_selectedLandmarkSegmentID].Toggle(true);

    //     _lineRender.SetLinePoints(new()
    //     {
    //         _placementWorldObject.transform,
    //         _currentLandmarkObjective.transform
    //     });

    //     _placementWorldObject.transform.position = _currentLandmarkObjective.transform.position;

    //     _textObjectName.text = "None";
    //     _textPreviewDistance.gameObject.SetActive(true);
    //     _distanceInfo.gameObject.SetActive(true);
    //     _textSelectedMode.text = "Landmark Objects";
    //     UpdateSegmentText();
    //     _nextButton.gameObject.SetActive(false);
    //     _finishButton.gameObject.SetActive(true);
    //     _finishButton.interactable = false;
    //     _selectionChange = false;
    // }

    private bool VerifyHoverObjects()
    {
        foreach (SegmentSocketSelection segment in _canvasHoverObjectData)
        {
            if (segment.SelectedObjectID == -1)
                return false;
        }
        return true;
    }

    // private bool VerifyLandmarkObjects()
    // {
    //     foreach (LandmarkPlacementData segment in _canvasLandmarkObjectData)
    //     {
    //         if (segment.SelectedObjectID == -1)
    //             return false;
    //     }
    //     return true;
    // }

    private void CreateObjectiveCanvasSockets()
    {
        foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
        {
            if (segment.SegmentID == -1) continue;

            Vector3 referencePosition = segment.transform.position;
            Vector3 socketWorldPosition = referencePosition;
            Vector2 canvasPosition = _pathPreviewCreator.RenderCamera.WorldCoordinatesToScreenSpace(socketWorldPosition);

            ObjectPlacementSocket socketInteractor = Instantiate(_objectiveSocketPrefab, _objectiveSocketHolder).GetComponent<ObjectPlacementSocket>();
            socketInteractor.Initialize(segment.SegmentID, 0, segment.SegmentColor, _hoverSegmentsGroup);
            socketInteractor.OnSocketObjectChanged += OnSocketSelectionChanged;
            socketInteractor.PreviewObjectChanged += OnSocketPreviewChanged;
            socketInteractor.PreviewObjectRemoved += OnSocketPreviewRemoved;
            socketInteractor.GetComponent<RectTransform>().anchoredPosition = canvasPosition;
            _canvasHoverObjectData.Add(new SegmentSocketSelection(segment.SegmentID, socketInteractor));
        }
    }

    // private void CreateLandmarkSocketData()
    // {
    //     foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
    //     {
    //         if (segment.SegmentID == -1) continue;
    //         Color segmentColor = PathManager.Instance.CurrentPath.Segments.Find(s => s.PathSegmentData.SegmentID == segment.SegmentID).PathSegmentData.SegmentColor;
    //         _canvasLandmarkObjectData.Add(new LandmarkPlacementData(segment.SegmentID, segmentColor));
    //     }
    // }

    // private void CreateLandmarkSegmentSelectors()
    // {
    //     foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
    //     {
    //         Vector3 referencePosition = segment.transform.position;
    //         Vector3 socketWorldPosition = referencePosition;
    //         Vector2 canvasPosition = _pathPreviewCreator.RenderCamera.WorldCoordinatesToScreenSpace(socketWorldPosition);

    //         CanvasSegmentSelector segmentSelector = Instantiate(_segmentSelectorPrefab, _landmarkSocketHolder).GetComponent<CanvasSegmentSelector>();
    //         segmentSelector.Initialize(segment.SegmentID, segment.SegmentColor, _landmarkSegmentsGroup);
    //         segmentSelector.GetComponent<RectTransform>().anchoredPosition = canvasPosition;
    //         _landmarkSegmentSelectors.Add(segmentSelector);
            
    //     }
    // }

    private void CreateHoverObjectSelectionGrid()
    {
        foreach (RenderObject obj in ResourceManager.Instance.HoverObjects)
        {
            GridObjectSelection selection = Instantiate(_objectSelectionPrefab, _objectSelectionHolder).GetComponent<GridObjectSelection>();
            selection.Initialize(obj.ID, obj.RenderTexture, _objectSelectionGroup);
            selection.SelectedObjectChanged += OnObjectSelectionChanged;
            selection.PreviewObjectChanged += OnSelectionPreviewChanged;
            selection.PreviewObjectRemoved += OnSelectionPreviewRemoved;
            _hoverObjectSelection.Add(selection);
        }
    }

    // private void CreateLandmarkObjectSelectionGrid()
    // {
    //     foreach (RenderObject obj in ResourceManager.Instance.LandmarkObjects)
    //     {
    //         GridObjectSelection selection = Instantiate(_objectSelectionPrefab, _objectSelectionHolder).GetComponent<GridObjectSelection>();
    //         selection.Initialize(obj.ID, obj.RenderTexture, _objectSelectionGroup);
    //         selection.SelectedObjectChanged += OnObjectSelectionChanged;
    //         selection.PreviewObjectChanged += OnSelectionPreviewChanged;
    //         selection.PreviewObjectRemoved += OnSelectionPreviewRemoved;
    //         _landmarkObjectSelection.Add(selection);
    //     }
    // }

    private void SetSegmentIndicators()
    {
        // if (_selectionMode == SelectionType.Hover)
        // {
            _hoverSegmentIndicators.ForEach(i => i.gameObject.SetActive(true));
        // }
        // else
        // {
        //     _hoverSegmentIndicators.ForEach(i => i.gameObject.SetActive(false));
        //     _landmarkSegmentIndicators.ForEach(i => i.gameObject.SetActive(true));
        // }
    }

    private void UpdateSegmentText()
    {
        // if (_selectionMode == SelectionType.Hover)
        // {
            if (_selectedHoverSegmentID == -1)
                _textSelectedSegment.text = "Unset";
            else
                _textSelectedSegment.text = _selectedHoverSegmentID.ToString();
        // }
        // else
        // {
        //     if (_selectedLandmarkSegmentID == -1)
        //         _textSelectedSegment.text = "Unset";
        //     else
        //         _textSelectedSegment.text = _selectedLandmarkSegmentID.ToString();
        // }
    }

    private void ClearGridSelection(List<GridObjectSelection> selections)
    {
        selections.ForEach(selection =>
        {
            selection.SelectedObjectChanged -= OnObjectSelectionChanged;
            selection.PreviewObjectChanged -= OnSelectionPreviewChanged;
            selection.PreviewObjectRemoved -= OnSelectionPreviewRemoved;
            Destroy(selection.gameObject);
        });
        selections.Clear();
    }

    // private bool GetInteractorPositionOnCanvas(out Vector2 canvasPosition)
    // {
    //     canvasPosition = Vector2.zero;
    //     if (UIInteractor.TryGetCurrentUIRaycastResult(out RaycastResult result))
    //     {
    //         if (result.gameObject != null && result.gameObject == _movementArea.gameObject)
    //         {
    //             RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //                 _movementArea,
    //                 result.screenPosition,
    //                 result.module.eventCamera,
    //                 out canvasPosition
    //             );
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    private bool ClearUIData()
    {
        // foreach (LandmarkPlacementData socket in _canvasLandmarkObjectData)
        // {
        //     socket.Socket.RemoveSocketObject();
        //     socket.LineRender.ResetPointList();
        //     Destroy(socket.LineRender.gameObject);
        //     Destroy(socket.WorldPositionObject);
        //     Destroy(socket.Socket.gameObject);
        // }
        // _canvasLandmarkObjectData.Clear();

        // foreach (CanvasSegmentSelector segment in _landmarkSegmentSelectors)
        // {
        //     Destroy(segment.gameObject);
        // }
        // _landmarkSegmentSelectors.Clear();

        // ClearGridSelection(_landmarkObjectSelection);

        // TODO ADD objective related stuff

        return true;
    }
    #endregion
}
