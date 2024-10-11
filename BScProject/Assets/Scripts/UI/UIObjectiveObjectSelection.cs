using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectiveObjectSelection : MonoBehaviour
{
    [SerializeField] private GameObject _segmentObjectSelectionPrefab;
    [SerializeField] private Transform _segmentSelectionParent;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Image _selectedPathImage;
    private List<SegmentObjectSelection> _segmentObjects = new();

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        _confirmButton.onClick.AddListener(OnSegmentObjectiveObjectAssigned);
        _confirmButton.interactable = false;

        for(int i = 0; i < AssessmentManager.Instance.CurrentPath.Segments.Count; i++)
        {
            SegmentObjectSelection segmentObjectSelection = Instantiate(_segmentObjectSelectionPrefab, _segmentSelectionParent).GetComponent<SegmentObjectSelection>();
            segmentObjectSelection.SetSegmentLabel(i, AssessmentManager.Instance.CurrentPath.Segments[i].SegmentColor);
            segmentObjectSelection.AddObjectChoise(AssessmentManager.Instance.CurrentPath.Segments[i].ObjectiveObjectRenderTexture);
            segmentObjectSelection.SelectedObjectChanged += OnObjectiveObjectChanged;
            _segmentObjects.Add(segmentObjectSelection);
        }

        _selectedPathImage.sprite = AssessmentManager.Instance.CurrentPath.PathImage;
    }

    private void OnDisable() 
    {
        _confirmButton.onClick.RemoveListener(OnSegmentObjectiveObjectAssigned);

        for(int i = 0; i < _segmentObjects.Count; i++)
        {
            if (i < AssessmentManager.Instance.CurrentPath.Segments.Count)
            {
                _segmentObjects[i].SelectedObjectChanged -= OnObjectiveObjectChanged;
            }
        }
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSegmentObjectiveObjectAssigned()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnObjectiveObjectChanged(int segmentID)
    {
        AssessmentManager.Instance.AssignPathSegmentObjectiveObject(segmentID, _segmentObjects[segmentID].GetSelectedRenderTexture());
        
        foreach (SegmentObjectSelection segmentObjectSelection in _segmentObjects)
        {
            if (segmentObjectSelection.GetSelectedRenderTexture() == null)
                return;
        }

        _confirmButton.interactable = true;
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------



}
