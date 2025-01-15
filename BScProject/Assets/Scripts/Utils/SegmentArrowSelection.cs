using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SegmentArrowSelection : MonoBehaviour
{
    public RectTransform RectTransform;
    public event Action<int> SelectedSegmentChanged;
    public int SegmentID = -1;
    public float Length;
    public bool hitBoundary = false;
    public bool isAdjusted = false;
    private Toggle _toggle;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable() 
    {
        RectTransform = GetComponent<RectTransform>();
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggleSelected);
    }

    void OnDestroy() 
    {
        _toggle.onValueChanged.AddListener(OnToggleSelected);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnToggleSelected(bool state)
    {
        if (state)
        {
            SelectedSegmentChanged?.Invoke(SegmentID);
        }
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int segmentID, float defaultLength, ToggleGroup toggleGroup)
    {
        SegmentID = segmentID;
        _toggle.group = toggleGroup;
        Length = defaultLength;
    }
}
