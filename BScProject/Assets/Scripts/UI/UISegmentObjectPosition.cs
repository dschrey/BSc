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
    public GameObject DragableObject;
    public UnityEvent<int> SelectedSegmentChanged;
    private List<SegmentObjectPositionOption> _segmentObjects = new();
    private SegmentObjectPositionOption _selectedToggle;
    public ToggleGroup ToggleGroup;

    // TODO Connect the dragable object to the segment

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        ToggleGroup = GetComponent<ToggleGroup>();
        SelectedSegmentChanged.AddListener(OnSelectedSegmentChanged);
        _sliderhorizontalPosition.onValueChanged.AddListener(OnHorizontalPositionChanged);
        _sliderverticalPosition.onValueChanged.AddListener(OnVerticalPositionChanged);
        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);
        SetSliderSettings(_sliderhorizontalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.x, 0f);
        SetSliderSettings(_sliderverticalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.y, 0f);

        for(int i = 0; i < AssessmentManager.Instance.PathAssessmentData.PathSegmentAssessments.Count; i++)
        {
            SegmentObjectPositionOption segmentObjectPosition = Instantiate(_segmentObjectPositionOptionPrefab, _segmentSelectionParent).GetComponent<SegmentObjectPositionOption>();
            segmentObjectPosition.InitializeSegment(i,
                AssessmentManager.Instance.PathAssessmentData.PathSegmentAssessments[i].SelectedObjectiveObjectSprite, this);
            _segmentObjects.Add(segmentObjectPosition);
        }
    }

    private void OnDisable() 
    {
        SelectedSegmentChanged.RemoveListener(OnSelectedSegmentChanged);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnSelectedSegmentChanged(int toggleID)
    {
        if (_selectedToggle != null)
        {
            _selectedToggle.SegmentCheckmark.gameObject.SetActive(_selectedToggle.ObjectPositioned);
        }
        _selectedToggle = _segmentObjects[toggleID];
        _sliderhorizontalPosition.value = _selectedToggle.HorizontalDistance;
        _sliderverticalPosition.value = _selectedToggle.VerticalDistance;


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

    private void OnVerticalPositionChanged(float value)
    {
        if (_selectedToggle == null)
            return;
        _selectedToggle.VerticalDistance = value;
    }

    private void OnHorizontalPositionChanged(float value)
    {
        if (_selectedToggle == null)
            return;
        _selectedToggle.HorizontalDistance = value;
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
            slider.value = GetSliderStepValue(currentValue, ExperimentManager.Instance.ExperimentSettings.MovementSpeedStepSize);
    }

    private float GetSliderStepValue(float value, float stepSize)
    {
        return Mathf.Round(value / stepSize) * stepSize;
    }


}
