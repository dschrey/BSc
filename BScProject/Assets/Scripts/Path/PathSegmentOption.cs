
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle)), RequireComponent(typeof(TMP_Text))]
public class PathSegmentOption : MonoBehaviour
{
    public int SegmentID { get; private set;  }
    public Color Color { get; private set;  }
    public Image CheckmarkText;
    public TMP_Text distanceText;
    public float DistanceValue;
    [SerializeField] private Image _segmentIndicator;
    [SerializeField] private UIObjectiveDistanceSelection _parentPanel;
    private Toggle _toggle;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        DistanceValue = -1f;
        distanceText = GetComponent<TMP_Text>();
        _toggle = GetComponent<Toggle>();
    
        if (_toggle == null)
        {
            Debug.LogError($"Toggle component could not be found.");
            return;
        }
        _toggle.onValueChanged.AddListener(OnToggleStateChanged);

        CheckmarkText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.AddListener(OnToggleStateChanged);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnToggleStateChanged(bool state)
    {
        if (! state)
        {
            _toggle.isOn = true;
        }

        if (_parentPanel == null)
        {
            Debug.LogError($"PathSegmentOption :: OnToggleStateChanged() : Could not find parent panel of {this}");
            return;
        }
        _parentPanel.SelectedSegmentChanged?.Invoke(this);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void SetSegmentLabel(int segmentID, Color color)
    {
        SegmentID = segmentID;
        Color = color;
        _segmentIndicator.color = Color;
    }

}