
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Class is deprecated and will be removed in the future.", true)]
[RequireComponent(typeof(Toggle))]
public class PathSegmentOption : MonoBehaviour
{
    public int SegmentID { get; private set;  }
    public Color Color { get; private set;  }
    public Image Checkmark;
    public TMP_Text DistanceText;
    public float DistanceValue;
    [SerializeField] private Image _segmentIndicator;
    public bool HasDistanceValue = false;
    private UIObjectiveDistanceSelection _parentPanel;
    private Toggle _toggle;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        DistanceValue = 0f;
        _toggle = GetComponent<Toggle>();
    
        if (_toggle == null)
        {
            Debug.LogError($"Toggle component could not be found.");
            return;
        }
        _toggle.onValueChanged.AddListener(OnToggleStateChanged);

        Checkmark.gameObject.SetActive(false);
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
        // _parentPanel.SelectedSegmentChanged?.Invoke(this);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int segmentID, Color color, UIObjectiveDistanceSelection parent)
    {
        SegmentID = segmentID;
        Color = color;
        _segmentIndicator.color = Color;
        _parentPanel = parent;
    }

}