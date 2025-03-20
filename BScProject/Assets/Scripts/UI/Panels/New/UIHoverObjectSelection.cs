using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class SegmentSocketSelection
{
    public int SegmentID;
    public int SelectedObjectID;
    public ObjectPlacementSocket Socket;

    public SegmentSocketSelection(int ID, ObjectPlacementSocket socket)
    {
        SegmentID = ID;
        SelectedObjectID = -1;
        Socket = socket;
    }
}

public class UIHoverObjectSelection : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private PathLayoutCreator _pathPreviewCreator;

    [Header("Canvas Objective Socket")]
    [SerializeField] private Transform _objectiveSocketHolder;
    [SerializeField] private GameObject _objectiveSocketPrefab;
    [SerializeField] private ToggleGroup _hoverSegmentsGroup;

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
    [SerializeField] private TMP_Text _textObjectName;

    [Header("Misc")]
    [SerializeField] private Button _nextButton;
    [SerializeField] private RawImage _selectedPathLayout;
    private bool _selectionChange;

    // Hover Objects Variables
    private readonly List<UISegmentIndicator> _hoverSegmentIndicators = new();
    private readonly List<GridObjectSelection> _hoverObjectSelection = new();
    private readonly List<SegmentSocketSelection> _canvasHoverObjectSelectionData = new();
    private int _selectedHoverSegmentID = -1;
    private int _selectedHoverObjectID = -1;
    private SegmentSocketSelection _currentSegmentHoverData => _canvasHoverObjectSelectionData.Find(s => s.SegmentID == _selectedHoverSegmentID);

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    #region Unity Methods
    void OnEnable()
    {
        _selectionChange = false;
        _buttonPreviousSegment.onClick.AddListener(OnPreviousSegmentClick);
        _buttonNextSegment.onClick.AddListener(OnNextSegmentModeClick);

        _nextButton.onClick.AddListener(OnContinueButtonClicked);

        int layoutID = AssessmentManager.Instance.GetSelectedPathLayout();
        _pathPreviewCreator = PathLayoutManager.Instance.GetPathLayout(layoutID);
        _pathPreviewCreator.ResetCameraView();
        _selectedPathLayout.texture = _pathPreviewCreator.RenderTexture;

        foreach (SegmentObjectData segment in _pathPreviewCreator.SpawnedSegments)
        {
            if (segment.SegmentID == -1) continue;
            UISegmentIndicator hoverSegmentIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
            _hoverSegmentIndicators.Add(hoverSegmentIndicator);
        }

        CreateObjectiveSelectionSockets();
        CreateHoverObjectSelectionGrid();
        UpdateSegmentText();
    }

    void OnDisable()
    {
        _buttonPreviousSegment.onClick.RemoveListener(OnPreviousSegmentClick);
        _buttonNextSegment.onClick.RemoveListener(OnNextSegmentModeClick);

        _nextButton.onClick.RemoveListener(OnContinueButtonClicked);

        _hoverSegmentIndicators.ForEach(i => Destroy(i.gameObject));
        _hoverSegmentIndicators.Clear();

        foreach (SegmentSocketSelection selectionData in _canvasHoverObjectSelectionData)
        {
            selectionData.Socket.OnSocketObjectChanged -= OnSocketSelectionChanged;
            selectionData.Socket.PreviewObjectChanged -= OnSocketPreviewChanged;
            selectionData.Socket.PreviewObjectRemoved -= OnSocketPreviewRemoved;
            selectionData.Socket.RemoveSocketObject();
            Destroy(selectionData.Socket.gameObject);
        }
        _canvasHoverObjectSelectionData.Clear();

        foreach (GridObjectSelection gridSelector in _hoverObjectSelection)
        {
            gridSelector.SelectedObjectChanged -= OnObjectSelectionChanged;
            gridSelector.PreviewObjectChanged -= OnSelectionPreviewChanged;
            gridSelector.PreviewObjectRemoved -= OnSelectionPreviewRemoved;
            Destroy(gridSelector.gameObject);
        }
        _hoverObjectSelection.Clear();
    }

    #endregion
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------
    #region Listener Methods

    private void OnNextSegmentModeClick()
    {
        _selectedHoverSegmentID = (_selectedHoverSegmentID + 1) % (_pathPreviewCreator.SpawnedSegments.Count - 1);
        SegmentSocketSelection hoverData = _currentSegmentHoverData;
        hoverData.Socket.Select();
        if (hoverData.SelectedObjectID != -1)
        {
            _selectedHoverObjectID = hoverData.SelectedObjectID;
            GridObjectSelection selection = _hoverObjectSelection.Find(sel => sel.ObjectTextureID == hoverData.SelectedObjectID);
            selection.Select();
        }
    }

    private void OnPreviousSegmentClick()
    {
        _selectedHoverSegmentID = (_selectedHoverSegmentID - 1 + (_pathPreviewCreator.SpawnedSegments.Count - 1)) % (_pathPreviewCreator.SpawnedSegments.Count - 1);
        SegmentSocketSelection hoverData = _currentSegmentHoverData;
        hoverData.Socket.Select();
        if (hoverData.SelectedObjectID != -1)
        {
            GridObjectSelection selection = _hoverObjectSelection.Find(sel => sel.ObjectTextureID == hoverData.SelectedObjectID);
            selection.Select();
        }
    }

    private void OnContinueButtonClicked()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    // Object Sockets
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

        SegmentSocketSelection hoverSegment = _canvasHoverObjectSelectionData.Find(s => s.SegmentID == segmentID);
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

        SegmentSocketSelection hoverSegment = _canvasHoverObjectSelectionData.Find(s => s.SegmentID == segmentID);
        if (hoverSegment == null) return;
        if (hoverSegment.SelectedObjectID != -1) return;

        ObjectPlacementSocket hoverSocket = hoverSegment.Socket;
        if (hoverSocket == null) return;
        hoverSocket.RemoveSocketObject();
    }

    // Selection Grid
    private void OnObjectSelectionChanged(int selectedObjectID)
    {
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
    }

    #endregion
    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------
    #region Class Methods

    private void CreateObjectiveSelectionSockets()
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
            _canvasHoverObjectSelectionData.Add(new SegmentSocketSelection(segment.SegmentID, socketInteractor));
        }
    }

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

    private void UpdateSegmentText()
    {
        if (_selectedHoverSegmentID == -1)
            _textSelectedSegment.text = "Unset";
        else
            _textSelectedSegment.text = _selectedHoverSegmentID.ToString();
    }

    private void UpdateObjectAssignment()
    {
        _nextButton.interactable = VerifyHoverObjects();
        if (_selectedHoverSegmentID == -1) return;

        SegmentSocketSelection hoverSelection = _currentSegmentHoverData;

        if (hoverSelection.SelectedObjectID == -1) return;

        Debug.Log($"Hover: Segment {_selectedHoverSegmentID} -  Object - {hoverSelection.SelectedObjectID}");
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
    }

    private bool VerifyHoverObjects()
    {
        foreach (SegmentSocketSelection segment in _canvasHoverObjectSelectionData)
        {
            if (segment.SelectedObjectID == -1)
                return false;
        }
        return true;
    }
    
    #endregion
}
