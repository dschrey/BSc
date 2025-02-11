using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExperimentSettings : MonoBehaviour
{
    [SerializeField] private Settings _settings;

    [SerializeField] private Slider _sliderPlayerDetectionRadius;
    [SerializeField] private Slider _sliderDefaultSegmentLength;
    [SerializeField] private TMP_InputField _inputPlayerDetectionValue;
    [SerializeField] private TMP_InputField _inputDefaultSegmentLength;
    [SerializeField] private TMP_InputField _inputRevealTime;
    [SerializeField] private TMP_InputField _inputTransitionDuration;
    [SerializeField] private Button _buttonReset;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _sliderPlayerDetectionRadius.onValueChanged.AddListener(OnPlayerDetectionRadiusChanged);
        _sliderDefaultSegmentLength.onValueChanged.AddListener(OnDefaultSegmentLengthChanged);
        _inputPlayerDetectionValue.onEndEdit.AddListener(OnPlayerDetectionInputChanged);
        _inputDefaultSegmentLength.onEndEdit.AddListener(OnSegmentLengthInputChanged);
        _inputRevealTime.onEndEdit.AddListener(OnRevealTimeChanged);
        _inputTransitionDuration.onEndEdit.AddListener(OnTransitionDurationChanged);
        _buttonReset.onClick.AddListener(OnResetButtonPressed);
    }


    private void OnDisable()
    {
        _sliderPlayerDetectionRadius.onValueChanged.RemoveListener(OnPlayerDetectionRadiusChanged);
        _sliderDefaultSegmentLength.onValueChanged.RemoveListener(OnDefaultSegmentLengthChanged);
        _inputPlayerDetectionValue.onEndEdit.RemoveListener(OnPlayerDetectionInputChanged);
        _inputDefaultSegmentLength.onEndEdit.RemoveListener(OnSegmentLengthInputChanged);
        _inputRevealTime.onEndEdit.RemoveListener(OnRevealTimeChanged);
        _inputTransitionDuration.onEndEdit.RemoveListener(OnTransitionDurationChanged);
        _buttonReset.onClick.RemoveListener(OnResetButtonPressed);
        DataManager.Instance.SaveSettings();
    }

    private void Start()
    {
        _inputRevealTime.text = _settings.ObjectiveRevealTime.ToString(CultureInfo.InvariantCulture) + "s";
        SetSliderSettings(_sliderPlayerDetectionRadius, 0, 2, _settings.PlayerDetectionRadius);
        _inputPlayerDetectionValue.text = _settings.PlayerDetectionRadius.ToString(CultureInfo.InvariantCulture) + "m";
        _inputTransitionDuration.text = _settings.TransitionDuration.ToString(CultureInfo.InvariantCulture) + "s";
        SetSliderSettings(_sliderDefaultSegmentLength, 0, 2.5f, _settings.SegmentLength);
        _inputDefaultSegmentLength.text = _settings.SegmentLength.ToString(CultureInfo.InvariantCulture) + "m";
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnPlayerDetectionRadiusChanged(float value)
    {
        float roundedValue = Mathf.Round(value * 100f) / 100f;
        _inputPlayerDetectionValue.text = roundedValue.ToString("F2", CultureInfo.InvariantCulture) + "m"; ;
        _settings.PlayerDetectionRadius = roundedValue;
    }

    private void OnPlayerDetectionInputChanged(string input)
    {
        input = input.Replace(",", ".");
        input = input.Replace("m", "");
        float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
        value = Math.Min(value, _sliderPlayerDetectionRadius.maxValue);
        float roundedValue = Mathf.Round(value * 100f) / 100f;

        _inputPlayerDetectionValue.text = roundedValue.ToString("F2", CultureInfo.InvariantCulture) + "m";
        _sliderPlayerDetectionRadius.value = roundedValue;
        _settings.PlayerDetectionRadius = roundedValue;
    }

    private void OnRevealTimeChanged(string input)
    {
        input = input.Replace(",", ".");
        input = input.Replace("s", "");
        float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
        float roundedValue = Mathf.Round(value * 100f) / 100f;
        _inputRevealTime.text = roundedValue.ToString(CultureInfo.InvariantCulture) + "s";
        _settings.ObjectiveRevealTime = roundedValue;
    }

    private void OnTransitionDurationChanged(string input)
    {
        input = input.Replace(",", ".");
        input = input.Replace("s", "");
        float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
        float roundedValue = Mathf.Round(value * 100f) / 100f;
        _inputTransitionDuration.text = roundedValue.ToString(CultureInfo.InvariantCulture) + "s";
        _settings.TransitionDuration = roundedValue;
    }

    private void OnSegmentLengthInputChanged(string input)
    {
        input = input.Replace(",", ".");
        input = input.Replace("m", "");
        float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
        value = Math.Min(value, _sliderDefaultSegmentLength.maxValue);
        float roundedValue = Mathf.Round(value * 100f) / 100f;

        _inputDefaultSegmentLength.text = roundedValue.ToString("F2", CultureInfo.InvariantCulture) + "m";
        _sliderDefaultSegmentLength.value = roundedValue;
        _settings.SegmentLength = roundedValue;
    }

    private void OnDefaultSegmentLengthChanged(float value)
    {
        float roundedValue = Mathf.Round(value * 100f) / 100f;
        _inputDefaultSegmentLength.text = roundedValue.ToString("F2", CultureInfo.InvariantCulture) + "m"; ;
        _settings.SegmentLength = roundedValue;
    }

    private void OnResetButtonPressed()
    {
        _sliderPlayerDetectionRadius.value = _settings.DefaultPlayerDetectionRadius;
        _settings.PlayerDetectionRadius = _settings.DefaultPlayerDetectionRadius;
        _inputPlayerDetectionValue.text = _settings.DefaultPlayerDetectionRadius.ToString("F2", CultureInfo.InvariantCulture) + "m";
        _inputRevealTime.text = _settings.DefaultObjectiveRevealTime.ToString(CultureInfo.InvariantCulture) + "s";
        _settings.ObjectiveRevealTime = _settings.DefaultObjectiveRevealTime;
        _inputTransitionDuration.text = _settings.DefaultTransitionDuration.ToString(CultureInfo.InvariantCulture) + "s";
        _settings.TransitionDuration = _settings.DefaultTransitionDuration;
        _sliderDefaultSegmentLength.value = _settings.DefaultSegmentLength;
        _settings.SegmentLength = _settings.DefaultSegmentLength;
        _inputDefaultSegmentLength.text = _settings.DefaultSegmentLength.ToString("F2", CultureInfo.InvariantCulture) + "m";
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



