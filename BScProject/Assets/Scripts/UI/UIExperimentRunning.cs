using UnityEngine;
using UnityEngine.UI;

public class UIExperimentRunning : MonoBehaviour
{

    [SerializeField] private Button _buttonShowHint;
    [SerializeField] private Button _buttonRestartSegment;
    [SerializeField] private Button _buttonStopExperiment;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _buttonShowHint.onClick.AddListener(OnShowObjectiveHintButtonClicked);
        _buttonRestartSegment.onClick.AddListener(OnRestartSegmentButtonClicked);
        _buttonStopExperiment.onClick.AddListener(OnStopExperimentClicked);
    }        

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnRestartSegmentButtonClicked()
    {
        PathManager.Instance.DisplaySegmentHint();
        gameObject.SetActive(false);
    }

    private void OnShowObjectiveHintButtonClicked()
    {
        PathManager.Instance.RestartSegment();
        gameObject.SetActive(false);
    }

    private void OnStopExperimentClicked()
    {
        ExperimentManager.Instance.StopExperiment();
        gameObject.SetActive(false);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------


}
