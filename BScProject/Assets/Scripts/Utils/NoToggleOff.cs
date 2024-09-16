using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class NoToggleOff : MonoBehaviour
{
    private Toggle _toggle;

    private void OnEnable()
    {
        _toggle = GetComponent<Toggle>();
        if (_toggle == null)
        {
            Debug.LogError($"Toggle component could not be found.");
            return;
        }
        _toggle.onValueChanged.AddListener(HandleToggleStateChanged);
    }
    private void OnDisable()
    {
        _toggle.onValueChanged.AddListener(HandleToggleStateChanged);
    }

    private void HandleToggleStateChanged(bool state)
    {
        if (! state)
        {
            _toggle.isOn = true;
        }
    }
}
