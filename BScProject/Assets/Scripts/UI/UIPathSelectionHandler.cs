using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPathSelectionHandler : MonoBehaviour
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private List<Toggle> _pathOptionToggles = new();


    // ---------- Unity Methods --------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _confirmButton.onClick.AddListener(OnPathSelectionConfirmed);
        _confirmButton.interactable = false;

        foreach (var toggle in _pathOptionToggles)
        {
            toggle.onValueChanged.AddListener(state =>
            {
                if (state)
                {
                    PathData path = toggle.GetComponent<PathOption>().PathData;
                    AssessmentManager.Instance.SelectedPath = path;
                    _confirmButton.interactable = state;
                }
            });
        }
    }

    private void OnDisable() 
    {
        _confirmButton.onClick.RemoveListener(OnPathSelectionConfirmed);

    }
        // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnPathSelectionConfirmed()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

}
