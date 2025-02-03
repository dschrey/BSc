using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISegmentDistances : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private PathLayoutCreator _pathPreviewCreator;

    [Header("Adjustments")]
    [SerializeField] private Slider _sliderDistance;
    [SerializeField] private TMP_Text _textSliderDistance;

    [Header("Selection")]
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private TMP_Text _textSegmentID;
    [SerializeField] private TMP_Text _textDistanceValue;
    [SerializeField] private GameObject _selectionObjectPrefab;
    [SerializeField] private RectTransform _movementArea;
    [SerializeField] private Transform _selectionObjectsHolder;

    [Header("Misc")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private RawImage _selectedPathLayout;

    private readonly List<SegmentArrowSelection> _segmentDistanceData = new();
    private SegmentArrowSelection _selectedSegment;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _sliderDistance.onValueChanged.AddListener(OnDistanceValueChanged);
        _continueButton.onClick.AddListener(OnContinueButtonPressed);

        _pathPreviewCreator = PathLayoutManager.Instance.GetPathLayout(AssessmentManager.Instance.CurrentPathAssessment.SelectedPathLayoutID);
        _pathPreviewCreator.ResetCameraView();
        _pathPreviewCreator.CanvasMovementArea = _movementArea;
        _selectedPathLayout.texture = _pathPreviewCreator.RenderTexture;

        _pathPreviewCreator.SegmentBoundaryStatusUpdate += OnSegmentBoundaryStatusUpdate;

        CreateSegmentSelectionObjects();

        SetSliderSettings(_sliderDistance, 0.5f, 10f, 0.5f);
        OnSelectedSegmentChanged(0);

        foreach (var segment in _segmentDistanceData)
        {
            AssessmentManager.Instance.SetSegmentObjectiveDistance(_selectedSegment.SegmentID, DataManager.Instance.ExperimentData.DefaultSegmentLength);
        }
    }


    void OnDisable() 
    {
        _pathPreviewCreator.SegmentBoundaryStatusUpdate -= OnSegmentBoundaryStatusUpdate;
        _continueButton.onClick.RemoveListener(OnContinueButtonPressed);
        _sliderDistance.onValueChanged.RemoveListener(OnDistanceValueChanged);

        _segmentDistanceData.ForEach(segmentData => {
            segmentData.SelectedSegmentChanged -= OnSelectedSegmentChanged;
            Destroy(segmentData.gameObject);
        });
        _segmentDistanceData.Clear();
    }
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSelectedSegmentChanged(int segmentID)
    {
        _selectedSegment = _segmentDistanceData.Find(data => data.SegmentID == segmentID);

        _textSegmentID.text = (_selectedSegment.SegmentID +1).ToString();
        _textDistanceValue.text = _selectedSegment.Length.ToString("F2", CultureInfo.InvariantCulture) + " m";

        SetSliderSettings(_sliderDistance, 0.5f, 10f, _selectedSegment.Length);
        _textSliderDistance.text = _selectedSegment.Length.ToString("F2", CultureInfo.InvariantCulture) + " m";
    }

    private async void OnDistanceValueChanged(float value)
    {
        if (_selectedSegment == null) return;

        float roundedValue = Mathf.Round(value * 10f) / 10f;

        if (await _pathPreviewCreator.AdjustArrowLength(_selectedSegment.SegmentID, roundedValue, roundedValue >= _selectedSegment.Length, _segmentDistanceData))
        {
            _sliderDistance.value = roundedValue;
            _selectedSegment.Length = roundedValue;

            _textSliderDistance.text = _selectedSegment.Length.ToString("F2", CultureInfo.InvariantCulture) + " m";
            _textDistanceValue.text = _selectedSegment.Length.ToString("F2", CultureInfo.InvariantCulture) + " m";
            AssessmentManager.Instance.SetSegmentObjectiveDistance(_selectedSegment.SegmentID, _selectedSegment.Length);
        }
        else
        {
            _sliderDistance.value = _selectedSegment.Length;
        }
    }

    private void OnSegmentBoundaryStatusUpdate(int segmentID, bool status)
    {
        SegmentArrowSelection segment = _segmentDistanceData.Find(data => data.SegmentID == segmentID);
        segment.hitBoundary = status;
    }

    private void OnContinueButtonPressed()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }
    
    private void OnBackButtonClicked()
    {
        AssessmentManager.Instance.GoToPreviousAssessmentStep();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------


    private void CreateSegmentSelectionObjects()
    {
        foreach (SegmentObjectData segmentData in _pathPreviewCreator.SpawnedSegments)
        {
            if (segmentData.SegmentID == -1 || segmentData.ArrowRenderer == null)
            {
                continue;
            }

            SegmentArrowSelection segmentSelection = Instantiate(_selectionObjectPrefab, _selectionObjectsHolder).GetComponent<SegmentArrowSelection>();
            segmentSelection.Initialize(segmentData.SegmentID, DataManager.Instance.ExperimentData.DefaultSegmentLength, _toggleGroup);
            (Vector3 position, Vector2 size) =  _pathPreviewCreator.CalculateSelectorProperties(segmentData.ArrowRenderer);
            segmentSelection.RectTransform.anchoredPosition = position;
            segmentSelection.RectTransform.sizeDelta = size;

            segmentSelection.SelectedSegmentChanged += OnSelectedSegmentChanged; 
            _segmentDistanceData.Add(segmentSelection);
        }
    }


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

}
