using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PathSelectionOption : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Toggle _toggle;
    private Sprite _pathImage;
    public UnityEvent<Sprite> PathSelectionChanged;

    // ---------- Unity Methods --------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _toggle.onValueChanged.AddListener(OnToggleSelected);
    }

    private void OnDisable() 
    {
        _toggle.onValueChanged.RemoveListener(OnToggleSelected);
    }

    // ---------- Listener Methods --------------------------------------------------------------------------------------------------------------------------------

    private void OnToggleSelected(bool state)
    {
        if (state)
        {
            PathSelectionChanged?.Invoke(_pathImage);
        }
        else
        {
            PathSelectionChanged?.Invoke(null);
        }
    }

    // ---------- Class Methods --------------------------------------------------------------------------------------------------------------------------------

    public void Initialize(Sprite image, ToggleGroup group)
    {
        _toggle.group = group;
        _image.sprite = image;
        _pathImage = image;
    }

}