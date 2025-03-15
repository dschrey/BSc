using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CanvasSegmentSelector : MonoBehaviour
{
    public int BelongsToSegmentID;
    [SerializeField] private Image _socketIndicator;
    [SerializeField] private Image _highlightImage;
    private Toggle _toggle;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _toggle = GetComponent<Toggle>();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int belongsToSegment, Color color, ToggleGroup group)
    {
        BelongsToSegmentID = belongsToSegment;
        _socketIndicator.color = color;
        Color highlightColor = color;
        highlightColor.a = 0.15f;
        _highlightImage.color = highlightColor;
        _toggle.group = group;
    }

    public void Select()
    {
        _toggle.isOn = true;
    }
}
