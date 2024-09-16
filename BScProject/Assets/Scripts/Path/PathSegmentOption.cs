
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle)), RequireComponent(typeof(TMP_Text))]
public class PathSegmentOption : MonoBehaviour
{
    public int SegmentID;
    public Toggle toggle;
    public Color color;
    public TMP_Text distanceText;

    [SerializeField] private UIItemDistanceSelection _parentPanel;

    private void OnEnable() 
    {
        distanceText.color = color;
    }

    // Start is called before the first frame update

    public void HandleSelectEvent()
    {
        if (_parentPanel == null)
        {
            Debug.LogError($"Could not find parent panel of {this}");
            return;
        }
        _parentPanel.SelectedSegmentChanged?.Invoke(this);
    }

}