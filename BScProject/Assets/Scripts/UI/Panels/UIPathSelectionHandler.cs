using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPathSelectionHandler : MonoBehaviour
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private List<PathSelectionOption> _pathOptions = new();

    // ---------- Unity Methods --------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _confirmButton.onClick.AddListener(OnPathSelectionConfirmed);
        _confirmButton.interactable = false;

        List<int> pathLayoutIDs = new();
        pathLayoutIDs.AddRange(AssessmentManager.Instance.CurrentPath.PathLayoutDisplayOrder);
        for (int i = 0; i < pathLayoutIDs.Count; i++)
        {
            _pathOptions[i].Initialize(pathLayoutIDs[i], PathLayoutManager.Instance.GetPathLayout(pathLayoutIDs[i]).LayoutRenderTexture);
            _pathOptions[i].PathSelectionChanged.AddListener(OnSelectedPathChanged);
        }
    }

    private void OnDisable() 
    {
        _pathOptions.ForEach(x => x.PathSelectionChanged.RemoveListener(OnSelectedPathChanged));
        _confirmButton.onClick.RemoveListener(OnPathSelectionConfirmed);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSelectedPathChanged(int selectedPathLayoutID)
    {
        
        if (selectedPathLayoutID != -1)
        {
            _confirmButton.interactable = true;
        }
        else
        {
            _confirmButton.interactable = false;
        }
        AssessmentManager.Instance.SetSelectedPathLayout(selectedPathLayoutID);
    }

    private void OnPathSelectionConfirmed()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------
}
