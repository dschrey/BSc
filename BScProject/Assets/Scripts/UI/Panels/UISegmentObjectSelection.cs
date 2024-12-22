using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Class is deprecated and will be removed in the future.")]
public class UISegmentObjectSelection : MonoBehaviour
{
    [Header("Segment Selection")]
    [SerializeField] private Transform _segmentIndicatorParent;
    [SerializeField] private GameObject _segmentIndicatorPrefab;
    [SerializeField] private Button _buttonPrevious;
    [SerializeField] private Button _buttonNext;
    [SerializeField] private TMP_Text _textSelectedSegment;

    [Header("Object Selection")]
    [SerializeField] private Transform _objectSelectionParent;
    [SerializeField] private GameObject _objectSelectionPrefab;
    [SerializeField] private GameObject _objectDisplay;
    [SerializeField] private Transform _objectDisplaySpawnpoint;
    [SerializeField] private ToggleGroup _toggleGroup;

    [Header("Misc")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Image _selectedPathImage;

    private readonly List<GridObjectSelection> _selectionObjects = new();
    private readonly List<UISegmentIndicator> _segmentIndicators = new();
    private readonly List<PathSegmentObjectData> _segmentObjectData = new();
    private PathSegmentObjectData _currentSegment;
    private int _selectedSegmentID;
    private GameObject _displayObject;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        _buttonPrevious.onClick.AddListener(OnPreviousSegmentButtonClick);
        _buttonNext.onClick.AddListener(OnNextSegmentButtonClick);
        _continueButton.onClick.AddListener(OnContinueButtonClicked);
        _backButton.onClick.AddListener(OnBackButtonClicked);

        _objectDisplay.SetActive(true);

        if (_segmentObjectData.Count == 0)
        {
            _selectedSegmentID = 0;
            AssessmentManager.Instance.CurrentPath.SegmentsData.ForEach(s =>
            {
                _segmentObjectData.Add(new PathSegmentObjectData(s));
                UISegmentIndicator segmentIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
                _segmentIndicators.Add(segmentIndicator);
            });
        }

        foreach (var obj in ResourceManager.Instance.ShuffleLandmarkObjects(420))
        {
            GridObjectSelection objectSelection = Instantiate(_objectSelectionPrefab, _objectSelectionParent).GetComponent<GridObjectSelection>();
            objectSelection.Initialize(obj.ID, obj.RenderTexture, _toggleGroup);
            objectSelection.SelectedObjectChanged += OnObjectChanged;
            objectSelection.HoverObjectChanged += OnDisplayObjectChanged;
            objectSelection.HoverObjectRemoved += OnDisplayObjectRemoved;
            _selectionObjects.Add(objectSelection);
        }

        UpdateSelectedSegment();

        _continueButton.interactable = VerifySelectionValues();
        // _selectedPathImage.sprite = AssessmentManager.Instance.CurrentPathAssessment.SelectedPathLayoutID;
    }

    private void OnDisable() 
    {
        _objectDisplay.SetActive(false); 
        Destroy(_displayObject);

        _buttonPrevious.onClick.RemoveListener(OnPreviousSegmentButtonClick);
        _buttonNext.onClick.RemoveListener(OnNextSegmentButtonClick);
        _continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        _backButton.onClick.RemoveListener(OnBackButtonClicked);

        _selectionObjects.ForEach(obj => 
        {
            obj.SelectedObjectChanged -= OnObjectChanged;
            obj.HoverObjectChanged -= OnDisplayObjectChanged;
            obj.HoverObjectRemoved -= OnDisplayObjectRemoved;
            Destroy(obj.gameObject);
        });

        _selectionObjects.Clear();
    }
    
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnNextSegmentButtonClick()
    {
        _segmentIndicators[_selectedSegmentID].Toggle(false);
        _selectedSegmentID = (_selectedSegmentID + 1) % _segmentObjectData.Count;
        UpdateSelectedSegment();
    }

    private void OnPreviousSegmentButtonClick()
    {
        _segmentIndicators[_selectedSegmentID].Toggle(false);
        _selectedSegmentID = (_selectedSegmentID - 1 + _segmentObjectData.Count) % _segmentObjectData.Count;
        UpdateSelectedSegment();
    }

    private void OnContinueButtonClicked()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnBackButtonClicked()
    {
        AssessmentManager.Instance.GoToPreviousAssessmentStep();
    }

    private void OnDisplayObjectChanged(int objectID)
    {
        if (_currentSegment.SelectedObjectID != -1)
            return; 
        UpdateDisplayObject(ResourceManager.Instance.GetLandmarkObject(objectID));
    }


    private void OnObjectChanged(int objectID)
    {
        _currentSegment.SelectedObjectID = objectID;
        if (objectID == -1)
        {
            _segmentIndicators[_selectedSegmentID].SetState(false);
            _continueButton.interactable = false;
            return; 
        }
        
        _segmentIndicators[_selectedSegmentID].SetState(true);
        AssessmentManager.Instance.AssignSegmentLandmarkObject(_currentSegment.PathSegmentData.SegmentID, objectID);
        UpdateDisplayObject(ResourceManager.Instance.GetLandmarkObject(objectID));

        _continueButton.interactable = VerifySelectionValues();
    }

    private void OnDisplayObjectRemoved()
    {
        if (_currentSegment.SelectedObjectID != -1)
            return;
        Destroy(_displayObject);
    }


    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void UpdateSelectedSegment()
    {
        // Making sure the "old" _currentSegment is not reseting its selection.
        if (_currentSegment != null)
        {
            GridObjectSelection currentGridSelection = _selectionObjects.Find(g => g.ObjectTextureID == _currentSegment.SelectedObjectID);
            if (currentGridSelection != null)
            {
                currentGridSelection.IsSegmentSwap = true;
            }
        }

        _currentSegment = _segmentObjectData[_selectedSegmentID];
        _textSelectedSegment.color = _currentSegment.PathSegmentData.SegmentColor;
        _textSelectedSegment.text = (_selectedSegmentID + 1).ToString();
        _segmentIndicators[_selectedSegmentID].Toggle(true);

        Toggle toggle = _toggleGroup.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            toggle.isOn = false;
        }

        GridObjectSelection gridObjectSelection = _selectionObjects.Find(o => o.ObjectTextureID == _currentSegment.SelectedObjectID);
        if (gridObjectSelection == null)
        {
            UpdateDisplayObject(null);
        }
        else
        { 
            UpdateDisplayObject(ResourceManager.Instance.GetLandmarkObject(_currentSegment.SelectedObjectID));
            _segmentIndicators[_selectedSegmentID].SetState(true);
            gridObjectSelection.Select();
        }
    }

    public void UpdateDisplayObject(GameObject obj)
    {
        if (_displayObject != null)
        {
            Destroy(_displayObject);
        }

        if (obj != null)
            _displayObject = Instantiate(obj, _objectDisplaySpawnpoint.position, Quaternion.identity, _objectDisplaySpawnpoint);
    }

    private bool VerifySelectionValues()
    {
        foreach (PathSegmentObjectData segment in _segmentObjectData)
        {
            if (segment.SelectedObjectID == -1)
                return false;
        }
        return true;
    }

    public void ResetPanelData()
    {
        _currentSegment = null;
        _selectedSegmentID = -1;
        _segmentIndicators.ForEach(i => Destroy(i.gameObject));
        _segmentIndicators.Clear();
        _segmentObjectData.Clear();
        _continueButton.interactable = false;
    }
}
