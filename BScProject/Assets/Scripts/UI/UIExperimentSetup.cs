using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExperimentSetup : MonoBehaviour
{
    [SerializeField] private Button _buttonSetSpawn;
    [SerializeField] private Button _buttonStart;
    [SerializeField] private Slider _sliderMovementSpeed;
    [SerializeField] private TMP_Text _textMovementSpeed;
    [SerializeField] private ExperimentStartPointHandler _startPointHandler;
    [SerializeField] private UIExperimentPanelManager _panelManager;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _buttonSetSpawn.onClick.AddListener(OnSetSpawnButtonClicked);
        _buttonStart.onClick.AddListener(OnStartButtonClicked);
        _sliderMovementSpeed.onValueChanged.AddListener(OnMovementSpeedChanged);
        SetSliderSettings(_sliderMovementSpeed, ExperimentManager.Instance.ExperimentSettings.MinMovementSpeedMultiplier, 
            ExperimentManager.Instance.ExperimentSettings.MaxMovementSpeedMultiplier, ExperimentManager.Instance.ExperimentSettings.MovementSpeedMultiplier);
        _textMovementSpeed.text = GetSliderStepValue(ExperimentManager.Instance.ExperimentSettings.MovementSpeedMultiplier, 
            ExperimentManager.Instance.ExperimentSettings.MovementSpeedStepSize).ToString();
    }  

    private void OnDisable()
    {
        _buttonSetSpawn.onClick.RemoveListener(OnSetSpawnButtonClicked);
        _buttonStart.onClick.RemoveListener(OnStartButtonClicked);
        _sliderMovementSpeed.onValueChanged.RemoveListener(OnMovementSpeedChanged);
    }      


    private void Update() 
    {
        if (ExperimentManager.Instance.ExperimentState == ExperimentState.IDLE)
        {
            if (_startPointHandler.ExperimentReadyToStart)
            {
                _buttonStart.interactable = true;
            }
            else
            {
                _buttonStart.interactable = false;
            }
        }
    }


    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnStartButtonClicked()
    {
        ExperimentManager.Instance.StartExperiment();
    }

    private void OnSetSpawnButtonClicked()
    {
        // _startPointHandler.SetExperimentStartPosition(ExperimentManager.Instance._XROrigin.transform);
        ExperimentManager.Instance._XROrigin.SetPositionAndRotation(_startPointHandler.transform.position, _startPointHandler.transform.rotation);
        _panelManager.ResetPanelPosition();
    }

    private void OnMovementSpeedChanged(float value)
    {
        _sliderMovementSpeed.value = GetSliderStepValue(value, ExperimentManager.Instance.ExperimentSettings.MovementSpeedStepSize);
        ExperimentManager.Instance.ExperimentSettings.MovementSpeedMultiplier = GetSliderStepValue(value, ExperimentManager.Instance.ExperimentSettings.MovementSpeedStepSize);
        _textMovementSpeed.text = GetSliderStepValue(value, ExperimentManager.Instance.ExperimentSettings.MovementSpeedStepSize).ToString();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void SetSliderSettings(Slider slider, float minValue, float maxValue, float currentValue)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        if (currentValue > maxValue)
            slider.value = maxValue;
        else if (currentValue < minValue)
            slider.value = minValue;
        else
            slider.value = GetSliderStepValue(currentValue, ExperimentManager.Instance.ExperimentSettings.MovementSpeedStepSize);
    }

    private float GetSliderStepValue(float value, float stepSize)
    {
        return Mathf.Round(value / stepSize) * stepSize;
    }

}
