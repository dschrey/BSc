using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExperimentSettings : MonoBehaviour
{
    [SerializeField] private ExperimentSettings _settings;
    
    [SerializeField] private Slider _sliderPlayerDetectionRadius;
    [SerializeField] private TMP_InputField _inputPlayerDetectionValue;
    [SerializeField] private TMP_InputField _inputRevealTime;
    [SerializeField] private Button _ButtonReset;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _sliderPlayerDetectionRadius.onValueChanged.AddListener(OnPlayerDetectionRadiusChanged);
        _inputPlayerDetectionValue.onEndEdit.AddListener(OnPlayerDetectionInputChanged);
        _inputRevealTime.onEndEdit.AddListener(OnRevealTimeChanged);
        _ButtonReset.onClick.AddListener(OnResetButtonPressed);
    }


    private void OnDisable()
    {
        _inputPlayerDetectionValue.onValueChanged.RemoveListener(OnPlayerDetectionInputChanged);
        _inputPlayerDetectionValue.onEndEdit.RemoveListener(OnPlayerDetectionInputChanged);
        _inputRevealTime.onEndEdit.RemoveListener(OnRevealTimeChanged);
        _ButtonReset.onClick.RemoveListener(OnResetButtonPressed);
        DataManager.Instance.SaveExperimentSettings();
    }

    private void Start() 
    {
        SetSliderSettings(_sliderPlayerDetectionRadius, 0, 2, 0);
        _inputRevealTime.text = _settings.ObjectiveRevealTime.ToString(CultureInfo.InvariantCulture) + "s";
        _sliderPlayerDetectionRadius.value = _settings.PlayerDetectionRadius;
        _inputPlayerDetectionValue.text = _settings.PlayerDetectionRadius.ToString(CultureInfo.InvariantCulture) + "s";
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnPlayerDetectionRadiusChanged(float value)
    {
        _inputPlayerDetectionValue.text = value.ToString("F2", CultureInfo.InvariantCulture) + "m";;
        _settings.PlayerDetectionRadius = value;
    }

    private void OnPlayerDetectionInputChanged(string input)
    {
        input = input.Replace(",", ".");
        input = input.Replace("m", "");
        float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
        value = Math.Min(value, _sliderPlayerDetectionRadius.maxValue);
        _inputPlayerDetectionValue.text = value.ToString("F2", CultureInfo.InvariantCulture) + "m";;
        _sliderPlayerDetectionRadius.value = value;
        _settings.PlayerDetectionRadius = value;
    }
    
    private void OnRevealTimeChanged(string input)
    {
        input = input.Replace(",", ".");
        input = input.Replace("s", "");
        float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
        _inputRevealTime.text = value.ToString(CultureInfo.InvariantCulture) + "s";
        _settings.ObjectiveRevealTime = value;
    }

    private void OnResetButtonPressed()
    {
        _sliderPlayerDetectionRadius.value = _settings.DefaultPlayerDetectionRadius;
        _settings.PlayerDetectionRadius = _settings.DefaultPlayerDetectionRadius;
        _inputPlayerDetectionValue.text = _settings.DefaultPlayerDetectionRadius.ToString("F2", CultureInfo.InvariantCulture) + "m";
        _inputRevealTime.text = _settings.DefaultObjectiveRevealTime.ToString(CultureInfo.InvariantCulture) + "s";
        _settings.ObjectiveRevealTime = _settings.DefaultObjectiveRevealTime;
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
            slider.value = currentValue;
    }
}



