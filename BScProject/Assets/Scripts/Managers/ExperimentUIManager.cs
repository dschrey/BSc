using UnityEngine;

public class ExperimentUIManager : MonoBehaviour
{

    [SerializeField] UINewExperiment _newExperimentPanel;
    [SerializeField] UIExperimentContinue _continueExperimentPanel;
    [SerializeField] GameObject _finishedExperimentPanel;

    void Start()
    {
        _newExperimentPanel.gameObject.SetActive(true);
    }

    public void LoadNextExperimentPanel(ExperimentData experiment, ExperimentState experimentState, AssessmentData assessmentData)
    {
        if (experimentState == ExperimentState.CANCELLED)
        {
            return;
        }
        if (experiment.paths.Count > 0)
        {
            _continueExperimentPanel.gameObject.SetActive(true);
            _newExperimentPanel.gameObject.SetActive(false);
            _continueExperimentPanel.ContinueExperiment(experiment, assessmentData);
            return;
        }

        if (assessmentData == null)
        {
            Debug.LogError($"Assessment data is null..");
            return;
        }
        _newExperimentPanel.gameObject.SetActive(false);
        assessmentData.Completed = true;
        DataManager.Instance.Settings.CompletedExperiments++;
        DataManager.Instance.SaveAssessmentData(assessmentData);
        DataManager.Instance.SaveSettings();
        _finishedExperimentPanel.SetActive(true);
    }
}
