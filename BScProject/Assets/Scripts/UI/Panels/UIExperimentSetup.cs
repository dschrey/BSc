using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Class is deprecated and will be removed in the future.")]
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
        SetSliderSettings(_sliderMovementSpeed, DataManager.Instance.Settings.MinMovementSpeedMultiplier,
            DataManager.Instance.Settings.MaxMovementSpeedMultiplier, DataManager.Instance.Settings.MovementSpeedMultiplier);
        _textMovementSpeed.text = GetSliderStepValue(DataManager.Instance.Settings.MovementSpeedMultiplier,
            DataManager.Instance.Settings.MovementSpeedStepSize).ToString();
    }

    private void OnDisable()
    {
        _buttonSetSpawn.onClick.RemoveListener(OnSetSpawnButtonClicked);
        _buttonStart.onClick.RemoveListener(OnStartButtonClicked);
        _sliderMovementSpeed.onValueChanged.RemoveListener(OnMovementSpeedChanged);
    }


    private void Update()
    {
        return;
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
        // ExperimentManager.Instance.StartExperiment();
        // ExperimentManager.Instance.TeleportPlayer(ExperimentManager.Instance.AssessmentRoomSpawnPoint);
    }

    private void OnSetSpawnButtonClicked()
    {
        ExperimentManager.Instance.MoveXROrigin(_startPointHandler.transform);
        _panelManager.ResetPanelPosition();
    }

    private void OnMovementSpeedChanged(float value)
    {
        _sliderMovementSpeed.value = GetSliderStepValue(value, DataManager.Instance.Settings.MovementSpeedStepSize);
        DataManager.Instance.Settings.MovementSpeedMultiplier = GetSliderStepValue(value, DataManager.Instance.Settings.MovementSpeedStepSize);
        _textMovementSpeed.text = GetSliderStepValue(value, DataManager.Instance.Settings.MovementSpeedStepSize).ToString();
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
            slider.value = GetSliderStepValue(currentValue, DataManager.Instance.Settings.MovementSpeedStepSize);
    }

    private float GetSliderStepValue(float value, float stepSize)
    {
        return Mathf.Round(value / stepSize) * stepSize;
    }

}
