using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PathSegmentObjectData
{
    public PathSegmentData PathSegmentData;
    public int SelectedObjectID = -1;

    public PathSegmentObjectData(PathSegmentData data)
    {
        PathSegmentData = data;
    }
}

public class UIObjectiveObjectSelection : MonoBehaviour
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
    private readonly List<UISegmentIndicator> _segmentIndicators = new();
    private readonly List<PathSegmentObjectData> _segmentObjectData = new();
    private PathSegmentObjectData _currentSegment;
    private int _selectedSegmentID;
    private GameObject _displayObject;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        _buttonPrevious.onClick.AddListener(OnPreviousButtonClick);
        _buttonNext.onClick.AddListener(OnNextButtonClick);
        _confirmButton.onClick.AddListener(OnSegmentObjectiveObjectAssigned);

        _objectDisplay.SetActive(true);
        _confirmButton.interactable = false;
        _selectedSegmentID = 0;

        AssessmentManager.Instance.CurrentPath.SegmentsData.ForEach(s =>
        {
            _segmentObjectData.Add(new PathSegmentObjectData(s));
            UISegmentIndicator segmentIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
            _segmentIndicators.Add(segmentIndicator);
        });

        UpdateSelectedSegment();

        foreach (var obj in ResourceManager.Instance.ShuffleObjectiveObjects(420))
        {
            GridObjectSelection objectSelection = Instantiate(_objectSelectionPrefab, _objectSelectionParent).GetComponent<GridObjectSelection>();
            objectSelection.Initialize(obj.ID, obj.RenderTexture, _toggleGroup);
            objectSelection.SelectedObjectChanged += OnObjectiveObjectChanged;
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
            selectionObj.SelectedObjectChanged -= OnObjectiveObjectChanged;
            selectionObj.HoverObjectChanged -= OnDisplayObjectChanged;
            Destroy(selectionObj.gameObject);
        }

        _selectionObjects.Clear();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnNextButtonClick()
    {
        _segmentIndicators[_selectedSegmentID].Toggle(false);
        _selectedSegmentID = (_selectedSegmentID + 1) % _segmentObjectData.Count;
        UpdateSelectedSegment();
    }

    private void OnPreviousButtonClick()
    {
        _segmentIndicators[_selectedSegmentID].Toggle(false);
        _selectedSegmentID = (_selectedSegmentID - 1 + _segmentObjectData.Count) % _segmentObjectData.Count;
        UpdateSelectedSegment();
    }

    private void OnDisplayObjectChanged(int objectID)
    {
        if (_currentSegment.SelectedObjectID != -1)
            return; 
        UpdateDisplayObject(ResourceManager.Instance.GetObjectiveObject(objectID));
    }

    private void OnSegmentObjectiveObjectAssigned()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnObjectiveObjectChanged(int objectID)
    {
        _currentSegment.SelectedObjectID = objectID;
        if (objectID == -1)
        {
            _segmentIndicators[_selectedSegmentID].SetState(false);
            _confirmButton.interactable = false;
            return; 
        }
        
        _segmentIndicators[_selectedSegmentID].SetState(true);
        AssessmentManager.Instance.AssignPathSegmentObjectiveObject(_currentSegment.PathSegmentData.SegmentID, objectID);
        UpdateDisplayObject(ResourceManager.Instance.GetObjectiveObject(objectID));
        
        foreach (PathSegmentObjectData segment in _segmentObjectData)
        {
            if (segment.SelectedObjectID == -1)
                return;
        }

        _confirmButton.interactable = true;
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void UpdateSelectedSegment()
    {
        GridObjectSelection currentGridSelection = _selectionObjects.Find(g => g.ObjectTextureID == _currentSegment.SelectedObjectID);
        if (currentGridSelection != null)
        {
            currentGridSelection.IsSegmentSwap = true;
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
            UpdateDisplayObject(ResourceManager.Instance.GetObjectiveObject(_currentSegment.SelectedObjectID));
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
        {
            _displayObject = Instantiate(obj, _objectDisplaySpawnpoint.position, Quaternion.identity, _objectDisplaySpawnpoint);
            _displayObject.AddComponent<Rotate>();
        }
    }
}
