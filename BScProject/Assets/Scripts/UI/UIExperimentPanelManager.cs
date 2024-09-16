using UnityEngine;
using UnityEngine.InputSystem;

public class UIExperimentPanelManager : MonoBehaviour
{

    [SerializeField] private GameObject _experimentSetupPanel;
    [SerializeField] private GameObject _experimentRunningPanel;
    [SerializeField] private GameObject _experimentFinishedPanel;
    [SerializeField] private InputActionProperty _activationButton;
    [SerializeField] private bool _isBillboard = true;
    private bool _menuVisible;


    // Update is called once per frame
    void Update()
    {
        if (_activationButton.action.WasPressedThisFrame())
        {
            if (ExperimentManager.Instance.ExperimentState == ExperimentState.RUNNING)
            {
                _menuVisible = _experimentRunningPanel.activeSelf;
                _menuVisible = !_menuVisible;
                _experimentRunningPanel.SetActive(_menuVisible);
            }
        }
    }

    public void OpenSetupPanel()
    {
        _experimentSetupPanel.SetActive(true);
    }

    public void CloseSetupPanel()
    {
        _experimentSetupPanel.SetActive(false);
    }

    public void ShowFinishPanel()
    {
        _experimentFinishedPanel.SetActive(true);
    }
}
