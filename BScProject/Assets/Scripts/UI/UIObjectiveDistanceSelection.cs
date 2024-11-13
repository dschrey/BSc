using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIObjectiveDistanceSelection : MonoBehaviour
{

    [SerializeField] private GameObject _segmentDistanceTogglePrefab;
    [SerializeField] private Transform _segmentDistanceToggleParent;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Image _selectedPathImage;
    [SerializeField] private UINumpadInputHandler _numpadInput;
    public UnityEvent<PathSegmentOption> SelectedSegmentChanged = new();
    private List<PathSegmentOption> _segmentDistanceToggles = new();
    private PathSegmentOption _selectedSegment;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        SelectedSegmentChanged.AddListener(OnSelectedSegmentChanged);
        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        _confirmButton.interactable = false;
        
        foreach (PathSegmentData segmentData in AssessmentManager.Instance.CurrentPath.SegmentsData)
        {
            PathSegmentOption segmentOption = Instantiate(_segmentDistanceTogglePrefab, _segmentDistanceToggleParent).GetComponent<PathSegmentOption>();
            segmentOption.Initialize(segmentData.SegmentID, segmentData.SegmentColor, this);
            _segmentDistanceToggles.Add(segmentOption);
        }

        _numpadInput.InputChangedEvent += OnNumpadInputChanged;
        _selectedPathImage.sprite = AssessmentManager.Instance.CurrentPathAssessment.SelectedPathSprite;
    }

    private void OnDisable() 
    {
        SelectedSegmentChanged.RemoveListener(OnSelectedSegmentChanged);
        _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        _numpadInput.InputChangedEvent -= OnNumpadInputChanged;

        _segmentDistanceToggles.ForEach(x => Destroy(x.gameObject));
        _segmentDistanceToggles.Clear();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnConfirmButtonClicked()
    {
        _numpadInput.ResetInput();
        _numpadInput.gameObject.SetActive(false);
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnSelectedSegmentChanged(PathSegmentOption segment)
    {
        _selectedSegment = segment;
        if (!_numpadInput.gameObject.activeSelf)
            _numpadInput.gameObject.SetActive(true);
        _numpadInput.ResetInput();
        _numpadInput.SelectionIndicator.color = _selectedSegment.Color;
    }

    private void OnNumpadInputChanged(string numpadInput)
    {
        if (_selectedSegment == null)
        {
            _numpadInput.ResetInput();
            return;
        }

        _selectedSegment.DistanceText.text = numpadInput + "m";

        if (! float.TryParse(numpadInput, out float distanceValue))
        {
            Debug.LogError($"Could not parse numpad input into float.");
            return;
        }

        if (distanceValue > 0)
        {
            _selectedSegment.HasDistanceValue = true;
            _selectedSegment.Checkmark.gameObject.SetActive(true);
        }
        else
        {
            _selectedSegment.HasDistanceValue = false;
            _selectedSegment.Checkmark.gameObject.SetActive(false);
        }

        _selectedSegment.DistanceValue = distanceValue;
        AssessmentManager.Instance.SetPathSegmentObjectiveDistance(_selectedSegment.SegmentID, _selectedSegment.DistanceValue);
        _confirmButton.interactable = CheckAllSegmentsVisited();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private bool CheckAllSegmentsVisited()
    {
        foreach (PathSegmentOption pathSegmentOption in _segmentDistanceToggles)
        {
            if (! pathSegmentOption.HasDistanceValue)
                return false;
        }
        return true;
    }
}
