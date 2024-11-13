using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISegmentObjectSelection : MonoBehaviour
{
    [SerializeField] private Transform _objectSelectionParent;
    [SerializeField] private GameObject _objectSelectionPrefab;
    [SerializeField] private Transform _segmentIndicatorParent;
    [SerializeField] private GameObject _segmentIndicatorPrefab;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Image _selectedPathImage;
    [SerializeField] private Button _buttonPrevious;
    [SerializeField] private Button _buttonNext;
    [SerializeField] private TMP_Text _textSelectedSegment;
    [SerializeField] private GameObject _objectDisplay;
    [SerializeField] private Transform _objectDisplaySpawnpoint;
    [SerializeField] private ToggleGroup _toggleGroup;
    private readonly List<GridObjectSelection> _selectionObjects = new();
    private readonly List<UISegmentIndicator> _segmentIndicator = new();
    private readonly List<PathSegmentObjectData> _segmentsToAssign = new();
    private PathSegmentObjectData _currentSegment;
    private int _selectedSegment;
    private GameObject _displayObject;
    

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        _buttonPrevious.onClick.AddListener(OnPreviousButtonClick);
        _buttonNext.onClick.AddListener(OnNextButtonClick);
        _confirmButton.onClick.AddListener(OnSegmentObjectiveObjectAssigned);

        _objectDisplay.SetActive(true);
        _confirmButton.interactable = false;
        _selectedSegment = 0;

        AssessmentManager.Instance.CurrentPath.SegmentsData.ForEach(s =>
        {
            _segmentsToAssign.Add(new PathSegmentObjectData(s));
            UISegmentIndicator segmentIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
            _segmentIndicator.Add(segmentIndicator);
        });

        UpdateSelectedSegment();

        foreach (var obj in ResourceManager.Instance.ShuffleSegmentObjects(420))
        {
            GridObjectSelection objectSelection = Instantiate(_objectSelectionPrefab, _objectSelectionParent).GetComponent<GridObjectSelection>();
            objectSelection.Initialize(obj.ID, obj.RenderTexture, _toggleGroup);
            objectSelection.SelectedObjectChanged += OnObjectChanged;
            objectSelection.HoverObjectChanged += OnDisplayObjectChanged;
            _selectionObjects.Add(objectSelection);
        }
        
        _selectedPathImage.sprite = AssessmentManager.Instance.CurrentPathAssessment.SelectedPathSprite;
    }

    private void OnDisable() 
    {
        _objectDisplay.SetActive(false); 
        Destroy(_displayObject);

        _buttonPrevious.onClick.RemoveListener(OnPreviousButtonClick);
        _buttonNext.onClick.RemoveListener(OnNextButtonClick);
        _confirmButton.onClick.RemoveListener(OnSegmentObjectiveObjectAssigned);

        foreach (var selectionObj in _selectionObjects)
        {
            selectionObj.SelectedObjectChanged -= OnObjectChanged;
            selectionObj.HoverObjectChanged -= OnDisplayObjectChanged;
            Destroy(selectionObj.gameObject);
        }

        _selectionObjects.Clear();
    }
    
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnNextButtonClick()
    {
        _segmentIndicator[_selectedSegment].Toggle(false);
        _selectedSegment = (_selectedSegment + 1) % _segmentsToAssign.Count;
        UpdateSelectedSegment();
    }

    private void OnPreviousButtonClick()
    {
        _segmentIndicator[_selectedSegment].Toggle(false);
        _selectedSegment = (_selectedSegment - 1 + _segmentsToAssign.Count) % _segmentsToAssign.Count;
        UpdateSelectedSegment();
    }

    private void OnDisplayObjectChanged(int objectID)
    {
        if (_currentSegment.selectedObjectID != -1)
            return; 
        UpdateDisplayObject(ResourceManager.Instance.GetSegmentObject(objectID));
    }

    private void OnSegmentObjectiveObjectAssigned()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnObjectChanged(int objectID)
    {
        _currentSegment.selectedObjectID = objectID;
        if (objectID == -1)
        {
            _segmentIndicator[_selectedSegment].SetState(false);
            _confirmButton.interactable = false;
            return; 
        }
        
        _segmentIndicator[_selectedSegment].SetState(true);
        AssessmentManager.Instance.AssignPathSegmentObject(_currentSegment.PathSegmentData.SegmentID, objectID);
        UpdateDisplayObject(ResourceManager.Instance.GetSegmentObject(objectID));
        
        foreach (PathSegmentObjectData segment in _segmentsToAssign)
        {
            if (segment.selectedObjectID == -1)
                return;
        }

        _confirmButton.interactable = true;
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void UpdateSelectedSegment()
    {
        GridObjectSelection currentGridSelection = _selectionObjects.Find(g => g.ObjectTextureID == _currentSegment.selectedObjectID);
        if (currentGridSelection != null)
        {
            currentGridSelection.IsSegmentSwap = true;
        }

        _currentSegment = _segmentsToAssign[_selectedSegment];
        _textSelectedSegment.color = _currentSegment.PathSegmentData.SegmentColor;
        _textSelectedSegment.text = (_selectedSegment + 1).ToString();
        _segmentIndicator[_selectedSegment].Toggle(true);

        Toggle toggle = _toggleGroup.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            toggle.isOn = false;
        }

        GridObjectSelection gridObjectSelection = _selectionObjects.Find(o => o.ObjectTextureID == _currentSegment.selectedObjectID);
        if (gridObjectSelection == null)
        {
            UpdateDisplayObject(null);
        }
        else
        { 
            UpdateDisplayObject(ResourceManager.Instance.GetSegmentObject(_currentSegment.selectedObjectID));
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

}
