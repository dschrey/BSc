using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PathSelectionOption : MonoBehaviour
{
    private int _layoutID = -1;
    [SerializeField] private RawImage _image;
    [SerializeField] private Toggle _toggle;
    public UnityEvent<int> PathSelectionChanged;


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
            PathSelectionChanged?.Invoke(_layoutID);
        }
        else
        {
            PathSelectionChanged?.Invoke(-1);
        }
    }

    // ---------- Class Methods --------------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int pathLayoutID, RenderTexture pathLayoutTexture)
    {
        _layoutID = pathLayoutID;
        _image.texture = pathLayoutTexture;
    }
}