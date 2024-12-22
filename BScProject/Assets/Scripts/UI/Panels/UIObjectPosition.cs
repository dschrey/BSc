using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectPosition : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private PathLayoutCreator _pathPreviewCreator;
    
    [Header("Indicator")]
    [SerializeField] private Transform _segmentIndicatorParent;
    [SerializeField] private GameObject _segmentIndicatorPrefab;
    private readonly List<UISegmentIndicator> _segmentIndicators;

    [Header("Object Positioning")]
    [SerializeField] private TMP_Text _textHorizontalPosition;
    [SerializeField] private Slider _sliderhorizontalPosition;
    [SerializeField] private TMP_Text _textVerticalPosition;
    [SerializeField] private Slider _sliderverticalPosition;
    [SerializeField] private LineController _lineRender;

    [Header("Object Info")]
    [SerializeField] private TMP_Text _textSegmentID;
    [SerializeField] private TMP_Text _textDistanceValue;
    [SerializeField] private TMP_Text _textDistanceValueBig;
    [SerializeField] private RawImage _objectPreview;

    [Header("Object Selection")]
    [SerializeField] private GameObject _selectionObjectPrefab;
    [SerializeField] private Transform _movementArea;
    [SerializeField] private ToggleGroup _toggleGroup;
    private CanvasCameraHandler _canvasCamera;

    [Header("Misc")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private RawImage _selectedPathLayout;

    private readonly List<SegmentObjectSelection> _objectPositionData = new();
    private SegmentObjectSelection _selectedSegmentObject;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _continueButton.onClick.AddListener(OnContinueButtonPressed);

        _pathPreviewCreator = PathLayoutManager.Instance.GetPathLayout(AssessmentManager.Instance.CurrentPathAssessment.SelectedPathLayoutID);
        _canvasCamera = _pathPreviewCreator.RenderCamera;
        _lineRender = _pathPreviewCreator.DistanceLineController;
        _selectedPathLayout.texture = _pathPreviewCreator.RenderTexture;

        CreateSegmentObjectData();

        _sliderhorizontalPosition.onValueChanged.AddListener(OnHorizontalPositionChanged);
        _sliderverticalPosition.onValueChanged.AddListener(OnVerticalPositionChanged);

        SetSliderSettings(_sliderhorizontalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.x * 100, 0f);
        SetSliderSettings(_sliderverticalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.y * 100, 0f);
    }


    void OnDisable() 
    {
        _continueButton.onClick.RemoveListener(OnContinueButtonPressed);
        _sliderhorizontalPosition.onValueChanged.RemoveListener(OnHorizontalPositionChanged);
        _sliderverticalPosition.onValueChanged.RemoveListener(OnVerticalPositionChanged);

        foreach (SegmentObjectSelection objectSelection in _objectPositionData)
        {
            objectSelection.SelectedObjectChanged -= OnSelectedObjectChanged;
            Destroy(objectSelection.WorldObject);
            Destroy(objectSelection.gameObject);

        }
        _objectPositionData.Clear();
    }
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSelectedObjectChanged(int segmentID)
    {
        if (_objectPositionData.Count < 1) return;
        _selectedSegmentObject = _objectPositionData.Find(data => data.SegmentID == segmentID);

        _textSegmentID.text = _selectedSegmentObject.SegmentID.ToString();
        _textDistanceValue.text = _selectedSegmentObject.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + " m";
        _textDistanceValueBig.text = _selectedSegmentObject.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + " m";
        _objectPreview.texture = ResourceManager.Instance.GetLandmarkObjectRenderTexture(_selectedSegmentObject.ObjectID);

        SetSliderSettings(_sliderhorizontalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.x * 100, _selectedSegmentObject.RectTransform.anchoredPosition.x);
        SetSliderSettings(_sliderverticalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.y * 100, _selectedSegmentObject.RectTransform.anchoredPosition.y);
        _textHorizontalPosition.text = GetHorizontalValue().ToString("F2", CultureInfo.InvariantCulture) + " m";
        _textVerticalPosition.text =  GetVerticalValue().ToString("F2", CultureInfo.InvariantCulture) + "m";
    }

    private void OnVerticalPositionChanged(float value)
    {
        if (_selectedSegmentObject == null) return;
        Vector2 newPosition = _selectedSegmentObject.RectTransform.anchoredPosition;
        newPosition.y = value;
        _selectedSegmentObject.RectTransform.anchoredPosition = newPosition;
        _selectedSegmentObject.UpdateWorldObjectPosition(_canvasCamera.ScreenCoordinatesToWorldSpace(newPosition));
        UpdateSegmentData();
    }

    private void OnHorizontalPositionChanged(float value)
    {
        if (_selectedSegmentObject == null) return;
        Vector2 newPosition = _selectedSegmentObject.RectTransform.anchoredPosition;
        newPosition.x = value;
        _selectedSegmentObject.RectTransform.anchoredPosition = newPosition;
        _selectedSegmentObject.UpdateWorldObjectPosition(_canvasCamera.ScreenCoordinatesToWorldSpace(newPosition));
        UpdateSegmentData();
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


    private void CreateSegmentObjectData()
    {
        foreach (SegmentObjectData segmentData in _pathPreviewCreator.SpawnedSegments)
        {
            if (segmentData.AssignedLandmarkObjectID == -1)
            {
                continue;
            }
            SegmentObjectSelection objectSelection = Instantiate(_selectionObjectPrefab, _movementArea).GetComponent<SegmentObjectSelection>();
            objectSelection.GetComponent<RectTransform>().anchoredPosition = segmentData.CanvasSocketPosition;
            objectSelection.Initialize(segmentData.SegmentID, segmentData.AssignedLandmarkObjectID, _toggleGroup);
            Vector3 worldPosition = _canvasCamera.ScreenCoordinatesToWorldSpace(segmentData.CanvasSocketPosition);
            worldPosition.y = 1;
            objectSelection.InstantiateWorldObject(worldPosition, _pathPreviewCreator.gameObject.transform);
            objectSelection.SelectedObjectChanged += OnSelectedObjectChanged;
            _objectPositionData.Add(objectSelection);            
        }
    }

    private void UpdateSegmentData()
    {
        GameObject objective = _pathPreviewCreator.SpawnedSegments.Find(objective => objective.SegmentID == _selectedSegmentObject.SegmentID).gameObject;
        _selectedSegmentObject.DistanceToObjective = CalculateDistance(objective, _selectedSegmentObject.WorldObject);
        Vector3 realSpawnpoint = objective.transform.position + AssessmentManager.Instance.CurrentPath.GetSegmentData(_selectedSegmentObject.SegmentID).RelativeObjectPositionToObjective;
        Vector3 objectPosition = _selectedSegmentObject.WorldObject.transform.position;
        objectPosition.y = 0;
        _selectedSegmentObject.DifferenceToRealPosition = Vector3.Distance(realSpawnpoint, objectPosition);

        AssessmentManager.Instance.SetSegmentLandmarkObjectDistance(_selectedSegmentObject.SegmentID, _selectedSegmentObject.DistanceToObjective, _selectedSegmentObject.DifferenceToRealPosition);

        _textHorizontalPosition.text = GetHorizontalValue().ToString("F2", CultureInfo.InvariantCulture) + "m";
        _textVerticalPosition.text =  GetVerticalValue().ToString("F2", CultureInfo.InvariantCulture) + "m";

        _textDistanceValue.text = _selectedSegmentObject.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + " m";
        _textDistanceValueBig.text = _selectedSegmentObject.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + " m";

        List<Transform> lineTransforms = new()
        {
            objective.transform,
            _selectedSegmentObject.WorldObject.transform
        };
        _lineRender.SetLinePoints(lineTransforms);

        _continueButton.interactable = VerifyPositionValues();
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

    public float CalculateDistance(GameObject from, GameObject to)
    {
        Vector3 fromPos = from.transform.position;
        fromPos.y = 0f; 
        Vector3 ToPos = to.transform.position;
        ToPos.y = 0f; 
        return Vector3.Distance(fromPos, ToPos);
    }

    public float GetHorizontalValue()
    {
        return Math.Abs(_pathPreviewCreator.SpawnedSegments.Find(objective => objective.SegmentID == _selectedSegmentObject.SegmentID).gameObject.transform.position.z - _selectedSegmentObject.WorldObject.transform.position.z);     
    }

    public float GetVerticalValue()
    {
        return Math.Abs(_pathPreviewCreator.SpawnedSegments.Find(objective => objective.SegmentID == _selectedSegmentObject.SegmentID).gameObject.transform.position.x - _selectedSegmentObject.WorldObject.transform.position.x);     
    }

    private bool VerifyPositionValues()
    {
        foreach (var positionData in _objectPositionData)
        {
            if (positionData.DistanceToObjective <= 0)
                return false;
        }
        return true;
    }

    public void ResetPanelData()
    {
    }
}
