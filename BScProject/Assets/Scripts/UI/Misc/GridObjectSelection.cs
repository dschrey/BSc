using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridObjectSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RawImage _objectImage;
    [SerializeField] private Texture _emptyTexture;
    public event Action<int> SelectedObjectChanged;
    public event Action<int> PreviewObjectChanged;
    public event Action PreviewObjectRemoved;
    private Toggle _toggle;
    public int ObjectTextureID;
    public bool IsSegmentSwap = false;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnObjectSelected);
        _objectImage.texture = _emptyTexture;
    }


    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(OnObjectSelected);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnObjectSelected(bool state)
    {
        if (state)
        {
            SelectedObjectChanged?.Invoke(ObjectTextureID);
        }
        else
        {
            SelectedObjectChanged?.Invoke(-1);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        PreviewObjectChanged?.Invoke(ObjectTextureID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PreviewObjectRemoved?.Invoke();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int objectID, RenderTexture texture, ToggleGroup group)
    {
        ObjectTextureID = objectID;
        _objectImage.texture = texture;
        _toggle.group = group;
    }

    public void Select()
    {
        _toggle.isOn = true;
    }

    public void DeSelect()
    {
        _toggle.isOn = false;
    }

}
