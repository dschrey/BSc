using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Class is deprecated and will be removed in the future.")]
public class UISegmentObjectPosition : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private GameObject _instructionPanel;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private Button _completeTutorialButton;
    [SerializeField] private GameObject _movementArea;
    private bool _hasCompletedTutorial = false;
    
    [Header("Segment Selection")]
    [SerializeField] private Transform _segmentIndicatorParent;
    [SerializeField] private GameObject _segmentIndicatorPrefab;
    [SerializeField] private Image _selectedPathImage;
    [SerializeField] private Button _buttonPrevious;
    [SerializeField] private Button _buttonNext;
    [SerializeField] private TMP_Text _textSelectedSegment;
    private readonly List<UISegmentIndicator> _segmentIndicators = new();

    [Header("Object Positioning")]
    [SerializeField] private TMP_Text _textDistanceValue;
    [SerializeField] private TMP_Text _textDistanceValueBig;
    [SerializeField] private TMP_Text _textHorizontalPosition;
    [SerializeField] private Slider _sliderhorizontalPosition;
    [SerializeField] private TMP_Text _textVerticalPosition;
    [SerializeField] private Slider _sliderverticalPosition;
    [SerializeField] private GameObject _canvasEnvironment;
    [SerializeField] private CanvasCameraHandler _canvasCamera;
    [SerializeField] private LineController _lineRender;
    [SerializeField] private UIDragHandler _draggableCanvasObject;
    [SerializeField] private DraggableObject _draggableObject;

    [Header("Position Adjustment Options")]
    [SerializeField] private Button _buttonUpLarge;
    [SerializeField] private Button _buttonUpSmall;
    [SerializeField] private Button _buttonDownLarge;
    [SerializeField] private Button _buttonDownSmall;
    [SerializeField] private Button _buttonLeftLarge;
    [SerializeField] private Button _buttonLeftSmall;
    [SerializeField] private Button _buttonRightLarge;
    [SerializeField] private Button _buttonRightSmall;
    [SerializeField] private Button _buttonReset;

    [Header("Misc")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _backButton;

    private readonly List<SegmentObjectPositionData> _segmentPositionData = new();
    public SegmentObjectPositionData _currentSegment;
    private int _selectedSegmentID;
    private Path _currentPath;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _canvasEnvironment.SetActive(true);

        _buttonPrevious.onClick.AddListener(OnPreviousSegmentButtonClick);
        _buttonNext.onClick.AddListener(OnNextSegmentButtonClick);

        _buttonUpLarge.onClick.AddListener(OnUpLargeButtonClicked);
        _buttonUpSmall.onClick.AddListener(OnUpSmallButtonClicked);
        _buttonDownLarge.onClick.AddListener(OnDownLargeButtonClicked);
        _buttonDownSmall.onClick.AddListener(OnDownSmallButtonClicked);
        _buttonLeftLarge.onClick.AddListener(OnLeftLargeButtonClicked);
        _buttonLeftSmall.onClick.AddListener(OnLeftSmallButtonClicked);
        _buttonRightLarge.onClick.AddListener(OnRightargeButtonClicked);
        _buttonRightSmall.onClick.AddListener(OnRightSmallButtonClicked);
        _buttonReset.onClick.AddListener(OnResetButtonClicked);
        
        if (!_hasCompletedTutorial)
        {
            _tutorialPanel.SetActive(true);
            _instructionPanel.SetActive(false);
            _movementArea.SetActive(false);
            _inputPanel.SetActive(false);
            _completeTutorialButton.onClick.AddListener(OnCompleteTutorialButtonPressed);
        }
        else
        {
            _tutorialPanel.SetActive(false);
            _instructionPanel.SetActive(true);
            _movementArea.SetActive(true);
            _inputPanel.SetActive(true);
        }

        _continueButton.onClick.AddListener(OnContinueButtonPressed);
        _backButton.onClick.AddListener(OnBackButtonClicked);

        _draggableCanvasObject.WorldPositionChanged.AddListener(OnDraggableSegmentObjectPositionChanged);
        _draggableCanvasObject.SegmentObjectPositioned.AddListener(OnDraggableSegmentObjectPositioned);

        _draggableCanvasObject.DraggableWorldObject = _draggableObject;

        SetSliderSettings(_sliderhorizontalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.x * 100, 0f);
        SetSliderSettings(_sliderverticalPosition, 0, ExperimentManager.Instance.ExperimentSettings.MovementArea.y * 100, 0f);

        if (_segmentPositionData.Count == 0)
        {
            _selectedSegmentID = 0;
            PrepareSegments();
            AssessmentManager.Instance.CurrentPath.SegmentsData.ForEach(s =>
            {
                _segmentPositionData.Add(new SegmentObjectPositionData(s));
                UISegmentIndicator segmentIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
                _segmentIndicators.Add(segmentIndicator);
            });
        }
        
        _continueButton.interactable = VerifyPositionValues();
        // _selectedPathImage.sprite = AssessmentManager.Instance.CurrentPathAssessment.SelectedPathLayoutID;

        UpdateSelectedSegment(-1);
    }

    private void OnDisable() 
    {
        _buttonPrevious.onClick.RemoveListener(OnPreviousSegmentButtonClick);
        _buttonNext.onClick.RemoveListener(OnNextSegmentButtonClick);
        
        _buttonUpLarge.onClick.RemoveListener(OnUpLargeButtonClicked);
        _buttonUpSmall.onClick.RemoveListener(OnUpSmallButtonClicked);
        _buttonDownLarge.onClick.RemoveListener(OnDownLargeButtonClicked);
        _buttonDownSmall.onClick.RemoveListener(OnDownSmallButtonClicked);
        _buttonLeftLarge.onClick.RemoveListener(OnLeftLargeButtonClicked);
        _buttonLeftSmall.onClick.RemoveListener(OnLeftSmallButtonClicked);
        _buttonRightLarge.onClick.RemoveListener(OnRightargeButtonClicked);
        _buttonRightSmall.onClick.RemoveListener(OnRightSmallButtonClicked);
        _buttonReset.onClick.RemoveListener(OnResetButtonClicked);

        _continueButton.onClick.RemoveListener(OnContinueButtonPressed);
        _backButton.onClick.RemoveListener(OnBackButtonClicked);
        _draggableCanvasObject.WorldPositionChanged.RemoveListener(OnDraggableSegmentObjectPositionChanged);
        _draggableCanvasObject.SegmentObjectPositioned.RemoveListener(OnDraggableSegmentObjectPositioned);

        _completeTutorialButton.onClick.RemoveListener(OnCompleteTutorialButtonPressed);

        _canvasEnvironment.SetActive(false);
    }
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnNextSegmentButtonClick()
    {
        int oldSegmentID = _selectedSegmentID;
        _selectedSegmentID = (_selectedSegmentID + 1) % _segmentPositionData.Count;
        UpdateSelectedSegment(oldSegmentID);
    }

    private void OnPreviousSegmentButtonClick()
    {
        int oldSegmentID = _selectedSegmentID;
        _selectedSegmentID = (_selectedSegmentID - 1 + _segmentPositionData.Count) % _segmentPositionData.Count;
        UpdateSelectedSegment(oldSegmentID);
    }

    private void OnContinueButtonPressed()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }
    
    private void OnBackButtonClicked()
    {
        AssessmentManager.Instance.GoToPreviousAssessmentStep();
    }

    private void OnCompleteTutorialButtonPressed()
    {
        _hasCompletedTutorial = true;
        _tutorialPanel.SetActive(false);
        _instructionPanel.SetActive(true);
        _movementArea.SetActive(true);
        _inputPanel.SetActive(true);
    }

    private void OnDraggableSegmentObjectPositionChanged()
    {
        _currentSegment.WorldObjectPosition = _draggableObject.transform.position;
        _currentSegment.CanvasObjectPosition = _draggableCanvasObject.GetCurrentPosition();
        _currentSegment.DistanceToObjective = CalculateDistanceToReferenceObject(_currentPath.GetPathSegment(_selectedSegmentID).gameObject);
        _currentSegment.DistanceToRealObject = CalculateDistanceToReferenceObject(_currentPath.Segments.Find(s => s.PathSegmentData.SegmentID == _currentSegment.PathSegmentData.SegmentID).SegmentObject);
        _textHorizontalPosition.text = GetHorizontalValue().ToString("F2", CultureInfo.InvariantCulture) + "m";
        _textVerticalPosition.text =  GetVerticalValue().ToString("F2", CultureInfo.InvariantCulture) + "m";
        _textDistanceValue.text = _currentSegment.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + "m";
        _textDistanceValueBig.text = _currentSegment.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + "m";
    }

    private void OnDraggableSegmentObjectPositioned()
    {
        _segmentIndicators[_selectedSegmentID].SetState(true);
        _currentSegment.WorldObjectPosition = _draggableObject.transform.position;
        _currentSegment.CanvasObjectPosition = _draggableCanvasObject.GetCurrentPosition();
        AssessmentManager.Instance.SetSegmentLandmarkObjectDistance(_currentSegment.PathSegmentData.SegmentID, _currentSegment.DistanceToObjective, _currentSegment.DistanceToRealObject);
   
        _continueButton.interactable = VerifyPositionValues();
    }

    
    private void OnResetButtonClicked()
    {
        _draggableObject.transform.position = _currentPath.GetPathSegment(_selectedSegmentID).gameObject.transform.position;
        _currentSegment.WorldObjectPosition = _draggableObject.transform.position;
        _currentSegment.CanvasObjectPosition = _canvasCamera.WorldCoordinatesToScreenSpace(_draggableObject.transform.position);
        _draggableCanvasObject.PositionCanvasObject(_currentSegment.CanvasObjectPosition);
        _currentSegment.DistanceToObjective = 0;
        _currentSegment.DistanceToRealObject = -1;
        _draggableCanvasObject.UpdateSliderValues();
        _segmentIndicators[_selectedSegmentID].SetState(false);
        _continueButton.interactable = false;
    }

    private void OnRightSmallButtonClicked()
    {
        _draggableCanvasObject.AdjustHorizontalSlider(1f);
    }

    private void OnRightargeButtonClicked()
    {
        _draggableCanvasObject.AdjustHorizontalSlider(10f);
    }

    private void OnLeftSmallButtonClicked()
    {
        _draggableCanvasObject.AdjustHorizontalSlider(-1f);
    }

    private void OnLeftLargeButtonClicked()
    {
        _draggableCanvasObject.AdjustHorizontalSlider(-10f);
    }

    private void OnDownSmallButtonClicked()
    {
        _draggableCanvasObject.AdjustVerticalSlider(-1f);
    }

    private void OnDownLargeButtonClicked()
    {
        _draggableCanvasObject.AdjustVerticalSlider(-10f);
    }

    private void OnUpSmallButtonClicked()
    {
        _draggableCanvasObject.AdjustVerticalSlider(1f);
    }

    private void OnUpLargeButtonClicked()
    {
        _draggableCanvasObject.AdjustVerticalSlider(10f);
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

    private void UpdateSelectedSegment(int oldSegmentID)
    {
        Debug.Log($"UpdateSelectedSegment() :: Switching segments: {oldSegmentID} -> {_selectedSegmentID}.");

        if (oldSegmentID != -1 && _currentSegment != null)
        {
            _segmentIndicators[_selectedSegmentID].Toggle(false);
            _currentPath.GetPathSegment(oldSegmentID).gameObject.SetActive(false);
        }

        _currentSegment = _segmentPositionData[_selectedSegmentID];
        _currentPath.GetPathSegment(_selectedSegmentID).gameObject.SetActive(true);
                
        _textSelectedSegment.color = _currentSegment.PathSegmentData.SegmentColor;
        _textSelectedSegment.text = (_selectedSegmentID + 1).ToString();
        _segmentIndicators[_selectedSegmentID].Toggle(true);

        _textDistanceValue.text = _currentSegment.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + "m";
        _textDistanceValueBig.text = _currentSegment.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + "m";

        PositionDraggableObjects();
        _draggableCanvasObject.UpdateSliderValues();
        _textHorizontalPosition.text = GetHorizontalValue().ToString("F2", CultureInfo.InvariantCulture) + "m";
        _textVerticalPosition.text =  GetVerticalValue().ToString("F2", CultureInfo.InvariantCulture) + "m";

        List<Transform> lineTransforms = new()
        {
            _currentPath.GetPathSegment(_selectedSegmentID).gameObject.transform,
            _draggableObject.transform
        };
        _lineRender.SetLinePoints(lineTransforms);
    }

    private void PositionDraggableObjects()
    {
        if (_currentSegment.WorldObjectPosition != Vector3.zero)
        {
            _draggableObject.transform.position = _currentSegment.WorldObjectPosition;
        }
        else
        {
            _draggableObject.transform.position = _currentPath.GetPathSegment(_selectedSegmentID).gameObject.transform.position;
            _currentSegment.CanvasObjectPosition = _canvasCamera.WorldCoordinatesToScreenSpace(_draggableObject.transform.position);
        }
        
        _draggableCanvasObject.PositionCanvasObject(_currentSegment.CanvasObjectPosition);
    }

    public float CalculateDistanceToReferenceObject(GameObject referenceObject)
    {
        Vector3 objectPos = _draggableObject.transform.position;
        objectPos.y = 0f; 
        Vector3 objectivePos = referenceObject.transform.position;
        objectivePos.y = 0f; 
        return Vector3.Distance(objectPos, objectivePos);
    }

    public float GetHorizontalValue()
    {
        return Math.Abs(_currentPath.GetPathSegment(_selectedSegmentID).gameObject.transform.position.z - _draggableObject.transform.position.z);     
    }

    public float GetVerticalValue()
    {
        return Math.Abs(_currentPath.GetPathSegment(_selectedSegmentID).gameObject.transform.position.x - _draggableObject.transform.position.x);     
    }

    private bool VerifyPositionValues()
    {
        foreach (SegmentObjectPositionData segmentObjectPosition in _segmentPositionData)
        {
            if (segmentObjectPosition.DistanceToObjective <= 0)
                return false;
        }
        return true;
    }

    public void ResetPanelData()
    {
        _currentPath = null;
        _currentSegment = null;
        _selectedSegmentID = -1;
        _segmentIndicators.ForEach(i => Destroy(i.gameObject));
        _segmentIndicators.Clear();
        _segmentPositionData.Clear();
        _continueButton.interactable = false;
    }
}
