using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIItemDistanceSelection : MonoBehaviour
{
    [SerializeField] private List<Toggle> _segmentDistanceToggles = new();
    [SerializeField] private TMP_Text _selectionDisplay;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private UINumpadInputHandler _numpadInput;
    public UnityEvent<PathSegmentOption> SelectedSegmentChanged = new();
    private PathSegmentOption _selectedSegment;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        SelectedSegmentChanged.AddListener(OnSelectedSegmentChanged);
        _confirmButton.onClick.AddListener(OnDistanceSelectionConfirmed);
        _numpadInput.InputChangedEvent += OnNumpadInputChanged;
    }


    private void OnDisable() 
    {
        SelectedSegmentChanged.RemoveListener(OnSelectedSegmentChanged);
        _confirmButton.onClick.RemoveListener(OnDistanceSelectionConfirmed);
    }
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnDistanceSelectionConfirmed()
    {
        EvaluationManager.Instance.ProceedToNextEvaluationStep();
    }

    private void OnSelectedSegmentChanged(PathSegmentOption segment)
    {
        _selectedSegment = segment;
        _selectionDisplay.color = segment.color;
        _confirmButton.interactable = CheckAllSegmentsVisited();
        if (_numpadInput != null)
            _numpadInput.ResetInput();
    }

    private void OnNumpadInputChanged(string numpadInput)
    {
        if (_selectedSegment == null)
            return;
        _selectedSegment.distanceText.text = numpadInput + "m";
        EvaluationManager.Instance.SelectedPath.Segments.ForEach(segment =>
        {
            if (segment.SegmentID == _selectedSegment.SegmentID)
            {
                if (! float.TryParse(numpadInput, out float floatInput))
                {
                    Debug.LogError($"Could not parse numpard input into float.");
                    return;
                }
                // if (EvaluationManager.Instance.EvaluationSettings.EvaluationType == EvaluationType.STANDARD)
                // {
                //     segment.DistanceDefault = floatInput;
                // }
                // else if (EvaluationManager.Instance.EvaluationSettings.EvaluationType == EvaluationType.EXTENDED)
                // {
                //     segment.DistanceExtended = floatInput;
                // }
            }
        });
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private bool CheckAllSegmentsVisited()
    {
        foreach (Toggle toggle in _segmentDistanceToggles)
        {
            if (! toggle.isOn)
                return false;
        }
        return true;
    }


}
