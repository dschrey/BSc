using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UISegmentObjectPosition : MonoBehaviour
{
    [SerializeField] private GameObject _segmentObjectPositionOptionPrefab;
    [SerializeField] private Transform _segmentSelectionParent;
    [SerializeField] private Slider _sliderhorizontalPosition;
    [SerializeField] private Slider _sliderverticalPosition;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private GameObject _canvasEnvironment;
    [SerializeField] private CanvasCameraHandler _canvasCamera;
    [SerializeField] private LineController _lineRender;
    public UnityEvent<int> SelectedSegmentChanged;
    private List<SegmentObjectPositionOption> _segmentObjects = new();
    private SegmentObjectPositionOption _selectedSegment;
    public ToggleGroup ToggleGroup;
    public Path _currentPath;
    [SerializeField] private UIDragHandler _draggableCanvasObject;

    // TODO test if this works


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        // _currentPath = null; // TODO remove comment after testing is done
        _selectedSegment = null;
        _segmentObjects.Clear();
        _lineRender.ResetLinePoints();

        ToggleGroup = GetComponent<ToggleGroup>();

        SelectedSegmentChanged.AddListener(OnSelectedSegmentChanged);
        _sliderhorizontalPosition.onValueChanged.AddListener(OnHorizontalPositionChanged);
        _sliderverticalPosition.onValueChanged.AddListener(OnVerticalPositionChanged);
        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);
        _draggableCanvasObject.WorldPositionChanged.AddListener(OnSegmentObjectPositionChanged);

        SetSliderSettings(_sliderhorizontalPosition, -ExperimentManager.Instance.ExperimentSettings.MovementArea.x * 50, ExperimentManager.Instance.ExperimentSettings.MovementArea.x *50, 0f);
        SetSliderSettings(_sliderverticalPosition, -ExperimentManager.Instance.ExperimentSettings.MovementArea.y*50, ExperimentManager.Instance.ExperimentSettings.MovementArea.y*50, 0f);

        for (int i = 0; i < AssessmentManager.Instance.PathAssessment.PathSegmentAssessments.Count; i++)
        {
            SegmentObjectPositionOption segmentObjectPosition = Instantiate(_segmentObjectPositionOptionPrefab, _segmentSelectionParent).GetComponent<SegmentObjectPositionOption>();
            segmentObjectPosition.InitializeSegment(i, _currentPath.Segments[i].PathSegmentData.ObjectPrefab, 
                AssessmentManager.Instance.PathAssessment.PathSegmentAssessments[i].SelectedObjectiveObjectRenderTexture,
                _currentPath.Segments[i].Objective.gameObject,  this);

            _segmentObjects.Add(segmentObjectPosition);
        }

        _canvasEnvironment.SetActive(true);

        PrepareSegments();
        SwapSegment(0, -1); // Start with first segment
    }


    private void OnDisable() 
    {
        SelectedSegmentChanged.RemoveListener(OnSelectedSegmentChanged);
        _sliderhorizontalPosition.onValueChanged.RemoveListener(OnHorizontalPositionChanged);
        _sliderverticalPosition.onValueChanged.RemoveListener(OnVerticalPositionChanged);
        _confirmButton.onClick.RemoveListener(OnConfirmButtonPressed);
        _canvasEnvironment.SetActive(false);

    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnSelectedSegmentChanged(int segmentToggleID)
    {
        _selectedSegment.SegmentCheckmark.gameObject.SetActive(_selectedSegment.ObjectPositioned);
        SwapSegment(segmentToggleID, _selectedSegment.SegmentID);
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

    private void OnHorizontalPositionChanged(float value)
    {
        if (_selectedSegment == null)
            return;
        _selectedSegment.HorizontalPosition = value;
    }

    private void OnVerticalPositionChanged(float value)
    {
        if (_selectedSegment == null)
            return;
        _selectedSegment.VerticalPosition = value;
    }

    private void OnSegmentObjectPositionChanged(Vector3 arg0)
    {
        _selectedSegment.CalculateDistance();
        AssessmentManager.Instance.SetPathSegmentObjectDistance(_selectedSegment.SegmentID, _selectedSegment.DistanceValue);
        _selectedSegment.HorizontalPosition = _sliderhorizontalPosition.value;
        _selectedSegment.VerticalPosition = _sliderverticalPosition.value;

        // Check if all segments objects are positioned
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
        _currentPath = null;
        _currentPath = PathManager.Instance.CurrentPath;

        foreach (PathSegment pathSegment in _currentPath.Segments)
        {
            pathSegment.gameObject.SetActive(false);
        }
    }

    private void SwapSegment(int newSegmentID, int oldSegmentID = -1)
    {
        if (oldSegmentID != -1)
        {
            _selectedSegment.DisableSegmentObject();
        }

        _selectedSegment = _segmentObjects[newSegmentID];
        foreach (PathSegment pathSegment in _currentPath.Segments)
        {
            if (oldSegmentID != -1 && oldSegmentID == pathSegment.PathSegmentData.SegmentID)
            {
                pathSegment.SegmentObject.SetActive(true);
                pathSegment.gameObject.SetActive(false);
            }
            if (newSegmentID == pathSegment.PathSegmentData.SegmentID)
            {
                pathSegment.gameObject.SetActive(true);
                pathSegment.SegmentObject.SetActive(false);

            }
        }

        _selectedSegment.SpawnSegmentObject(_canvasEnvironment.transform);
        _draggableCanvasObject.SetWorldObject(_selectedSegment.SegmentObject);
        List<Transform> lineTransforms = new()
        {
            _selectedSegment.SegmentObjective.transform,
            _selectedSegment.SegmentObject.transform
        };
        _lineRender.SetLinePoints(lineTransforms);
        _sliderhorizontalPosition.value = _selectedSegment.HorizontalPosition;
        _sliderverticalPosition.value = _selectedSegment.VerticalPosition;


        
        _draggableCanvasObject.SetWorldObject(_selectedSegment.SegmentObject);
    }


}
