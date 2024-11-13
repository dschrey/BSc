using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINextPathPrompt : MonoBehaviour
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _remainingPaths;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        _confirmButton.onClick.AddListener(OnNextPathRequested);
        _remainingPaths.text = (ExperimentManager.Instance.Paths.Count - 1).ToString();
    }

    private void OnDisable() 
    {
        _confirmButton.onClick.RemoveListener(OnNextPathRequested);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnNextPathRequested()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

}
