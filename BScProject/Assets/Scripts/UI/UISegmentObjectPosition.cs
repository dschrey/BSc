using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISegmentObjectPosition : MonoBehaviour
{
    [SerializeField] private GameObject _segmentObjectPositionOptionPrefab;
    [SerializeField] private Transform _segmentSelectionParent;
    [SerializeField] private TMP_Text _textHorizontalPosition;
    [SerializeField] private Slider _sliderhorizontalPosition;
    [SerializeField] private TMP_Text _textVerticalPosition;
    [SerializeField] private Slider _sliderverticalPosition;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private GameObject _canvasEnvironment;
    [SerializeField] private CanvasCameraHandler _canvasCamera;
    [SerializeField] private LineController _lineRender;
    [SerializeField] private UIDragHandler _draggableCanvasObject;
    [SerializeField] private ToggleGroup _toggleGroup;
    public UnityEvent<int> SelectedSegmentChanged;
    private List<SegmentObjectPositionOption> _segmentObjects = new();
    private SegmentObjectPositionOption _selectedSegment;
    private Path _currentPath;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _canvasEnvironment.SetActive(true);

        SelectedSegmentChanged.AddListener(OnSelectedSegmentChanged);
        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);
        _draggableCanvasObject.WorldPositionChanged.AddListener(OnDraggableSegmentObjectPositionChanged);
        _draggableCanvasObject.SegmentObjectPositioned.AddListener(OnDraggableSegmentObjectPositioned);

        SetSliderSettings(_sliderhorizontalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.x * 100, 0f);
        SetSliderSettings(_sliderverticalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.y * 100, 0f);

        PrepareSegments();

        if (_currentPath == null)
        {
            Debug.LogError($"UISegmentObjectPosition :: OnEnable() : _currentPath is null");
            return;
        }

        foreach (PathSegmentAssessment pathAssessmentData in AssessmentManager.Instance.CurrentPathAssessment.PathSegmentAssessments)
        {
            SegmentObjectPositionOption segmentObjectPosition = Instantiate(_segmentObjectPositionOptionPrefab, _segmentSelectionParent).GetComponent<SegmentObjectPositionOption>();
            segmentObjectPosition.Initialize(pathAssessmentData.GetSegmentID(), pathAssessmentData.GetSegmentData().SegmentColor,
                _toggleGroup, ResourceManager.Instance.GetSegmentRenderTexture(pathAssessmentData.SelectedSegmentObjectID),
                ResourceManager.Instance.GetSegmentObject(pathAssessmentData.GetSegmentData().SegmentObjectID),
                _currentPath.Segments.Find(seg => seg.PathSegmentData.SegmentID == pathAssessmentData.GetSegmentID()).gameObject, this);
            _segmentObjects.Add(segmentObjectPosition);
        }

        _segmentObjects[0].Select();
    }

    private void OnDisable() 
    {
        SelectedSegmentChanged.RemoveListener(OnSelectedSegmentChanged);
        _confirmButton.onClick.RemoveListener(OnConfirmButtonPressed);

        _currentPath = null;
        _draggableCanvasObject.SetupDraggableObjects(null);
        _selectedSegment = null;
        _lineRender.ResetLinePoints();

        _segmentObjects.ForEach(s => s.CleanSegment());
        _segmentObjects.Clear();
        
        _canvasEnvironment.SetActive(false);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSelectedSegmentChanged(int segmentToggleID)
    {
        int oldSegmentID = -1;
        if (_selectedSegment != null)
        {
            oldSegmentID = _selectedSegment.SegmentID;
        }
        SwapSegment(segmentToggleID, oldSegmentID);
    }

    private void OnConfirmButtonPressed()
    {
        foreach (SegmentObjectPositionOption segmentObjectPosition in _segmentObjects)
        {
            if (!segmentObjectPosition.ObjectPositioned)
                return;
        }

        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnDraggableSegmentObjectPositionChanged()
    {
        _selectedSegment.CalculateDistanceToRealObject(_currentPath.Segments.Find(s => s.PathSegmentData.SegmentID == _selectedSegment.SegmentID).SegmentObject);
        _selectedSegment.CalculateDistanceToObjective();
        _textHorizontalPosition.text = _selectedSegment.GetHorizontalValue().ToString("F2") + "m";
        _textVerticalPosition.text =  _selectedSegment.GetVerticalValue().ToString("F2") + "m";
    }

    private void OnDraggableSegmentObjectPositioned()
    {
        _selectedSegment.CanvasObjectPosition = _draggableCanvasObject.GetCurrentPosition();
        AssessmentManager.Instance.SetPathSegmentObjectDistance(_selectedSegment.SegmentID, _selectedSegment.DistanceToObjective, _selectedSegment.DistanceToRealObject);
   
        foreach (SegmentObjectPositionOption segmentObjectPosition in _segmentObjects)
        {
            if (!segmentObjectPosition.ObjectPositioned)
                return;
        }
        _confirmButton.interactable = true;
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void SetSliderSettings(Slider slider, float minValue, float maxValue, float currentValue)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        if (currentValue > maxValue)
            slider.value = maxValue;
        else if (currentValue < minValue)
            slider.value = minValue;
        else
            slider.value = currentValue;
    }

    private void PrepareSegments()
    {
        _currentPath = PathManager.Instance.CurrentPath;

        foreach (PathSegment pathSegment in _currentPath.Segments)
        {
            pathSegment.gameObject.SetActive(false);
        }
    }

    private void SwapSegment(int newSegmentID, int oldSegmentID)
    {
        Debug.Log($"SwapSegment() :: Switching segments: {oldSegmentID} -> {newSegmentID}.");
        for (int i = 0; i < _segmentObjects.Count; i++)
        {
            if (oldSegmentID != -1 && oldSegmentID == _segmentObjects[i].SegmentID)
            {
                _selectedSegment.DisableDraggableSegmentObject();
                _currentPath.Segments[i].gameObject.SetActive(false);
            }
            if (newSegmentID == _segmentObjects[i].SegmentID)
            {
                _currentPath.Segments[i].gameObject.SetActive(true);
            }
        }
    
        _selectedSegment = _segmentObjects[newSegmentID];
        _selectedSegment.EnableDraggableSegmentObject(_canvasEnvironment.transform);
        _draggableCanvasObject.SetupDraggableObjects(_selectedSegment.DraggableSegmentObject);
        _draggableCanvasObject.UpdateSliderValues();
        _textHorizontalPosition.text = _selectedSegment.GetHorizontalValue().ToString("F2") + "m";
        _textVerticalPosition.text =  _selectedSegment.GetVerticalValue().ToString("F2") + "m";

        List<Transform> lineTransforms = new()
        {
            _selectedSegment.SegmentObjective.transform,
            _selectedSegment.DraggableSegmentObject.transform
        };
        _lineRender.SetLinePoints(lineTransforms);
    }
}
