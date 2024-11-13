using UnityEngine;
using UnityEngine.UI;

public class UISegmentIndicator : MonoBehaviour
{
    [SerializeField] Color _colorActiveDefault;
    [SerializeField] Color _colorInactiveDefault;
    [SerializeField] Color _colorActiveCompleted;
    [SerializeField] Color _colorInactiveCompleted;
    [SerializeField] Image _activeDot;
    [SerializeField] Image _inactiveDot;

    private bool _isActive;
    private bool _isCompleted;

    private void Start()
    {
        _isActive = false;
        _isCompleted = false;
    }

    public void Toggle(bool isOn)
    {
        _isActive = isOn;
        _activeDot.enabled = isOn;
        _inactiveDot.enabled = ! isOn;

        Color targetColor;

        if (_isActive)
        {
            targetColor = _isCompleted ? _colorActiveCompleted : _colorActiveDefault;
        }
        else
        {
            targetColor = _isCompleted ? _colorInactiveCompleted : _colorInactiveDefault;
        }

        _activeDot.color = targetColor;
        _inactiveDot.color = targetColor;
    }

    public void SetState(bool isCompleted)
    {
        _isCompleted = isCompleted;
        if (isCompleted)
        {
            _activeDot.color = _colorActiveCompleted;
            _inactiveDot.color = _colorInactiveCompleted;
        }
        else
        {
            _activeDot.color = _colorActiveDefault;
            _inactiveDot.color = _colorInactiveDefault;
        }
    }
}
