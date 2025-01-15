using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINextPathPrompt : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _remainingPaths;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        _continueButton.onClick.AddListener(OnNextPathRequested);
        _backButton.onClick.AddListener(OnBackButtonPressed);
        _remainingPaths.text = (ExperimentManager.Instance.Paths.Count - (ExperimentManager.Instance.CompletedPaths + 1)).ToString();
    }

    private void OnDisable() 
    {
        _continueButton.onClick.RemoveListener(OnNextPathRequested);
        _backButton.onClick.RemoveListener(OnBackButtonPressed);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnNextPathRequested()
    {
        PathLayoutManager.Instance.PathLayouts.ForEach(layout => layout.ClearPath());
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnBackButtonPressed()
    {
        AssessmentManager.Instance.GoToPreviousAssessmentStep();
    }

}
