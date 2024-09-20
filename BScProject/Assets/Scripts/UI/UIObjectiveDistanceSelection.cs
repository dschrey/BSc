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
        _confirmButton.onClick.AddListener(OnDistanceSelectionConfirmed);
        _confirmButton.interactable = false;
        

        foreach (PathSegmentData segmentData in AssessmentManager.Instance.SelectedPath.Segments)
        {
            PathSegmentOption segmentOption = Instantiate(_segmentDistanceTogglePrefab, _segmentDistanceToggleParent).GetComponent<PathSegmentOption>();
            segmentOption.SetSegmentLabel(segmentData.SegmentID, segmentData.SegmentColor);
            _segmentDistanceToggles.Add(segmentOption);
        }

        _numpadInput.InputChangedEvent += OnNumpadInputChanged;
        _selectedPathImage.sprite = AssessmentManager.Instance.SelectedPath.pathTexture;
    }

    private void OnDisable() 
    {
        SelectedSegmentChanged.RemoveListener(OnSelectedSegmentChanged);
        _confirmButton.onClick.RemoveListener(OnDistanceSelectionConfirmed);
        _numpadInput.InputChangedEvent -= OnNumpadInputChanged;
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnDistanceSelectionConfirmed()
    {
        _numpadInput.ResetInput();
        _numpadInput.gameObject.SetActive(false);
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnSelectedSegmentChanged(PathSegmentOption segment)
    {
        _selectedSegment = segment;
        _confirmButton.interactable = CheckAllSegmentsVisited();
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
        
        _selectedSegment.distanceText.text = numpadInput + "m";

        if (! float.TryParse(numpadInput, out float distanceValue))
        {
            Debug.LogError($"Could not parse numpad input into float.");
            return;
        }
        _selectedSegment.DistanceValue = distanceValue;
        AssessmentManager.Instance.SetPathSegmentDistance(_selectedSegment.SegmentID, _selectedSegment.DistanceValue);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private bool CheckAllSegmentsVisited()
    {
        foreach (PathSegmentOption pathSegmentOption in _segmentDistanceToggles)
        {
            if (pathSegmentOption.DistanceValue == -1f)
                return false;
        }
        return true;
    }
}
