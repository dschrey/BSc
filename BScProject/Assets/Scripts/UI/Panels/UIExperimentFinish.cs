using UnityEngine;
using UnityEngine.UI;

public class UIExperimentFinish : MonoBehaviour
{
    [SerializeField] private Button _buttonRestart;
    [SerializeField] private UIExperimentPanelManager _panelManager;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _buttonRestart.onClick.AddListener(OnRestartExperimentButtonClicked);
    }

    private void OnDisable() 
    {
        _buttonRestart.onClick.RemoveListener(OnRestartExperimentButtonClicked);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnRestartExperimentButtonClicked()
    {
        _panelManager.ShowSetupPanel();
        ExperimentManager.Instance.ExperimentState = ExperimentState.Idle;
        gameObject.SetActive(false);
    }

}
