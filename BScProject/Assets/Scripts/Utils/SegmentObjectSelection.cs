using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SegmentObjectSelection : MonoBehaviour
{
    public RectTransform RectTransform;
    public event Action<int> SelectedObjectChanged;
    public int SegmentID = -1;
    public int ObjectID = -1;
    public float DistanceToObjective;
    public float DifferenceToRealPosition;
    public float MovementAngle;
    public Vector3 MovementDirection;
    public Vector3 MaxLocalPosition = Vector3.zero;
    public GameObject WorldObject;
    private Toggle _toggle;
    [SerializeField] private Image _crosshair;


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
            SelectedObjectChanged?.Invoke(SegmentID);
        }
        _crosshair.gameObject.SetActive(state);
        _toggle.targetGraphic.gameObject.SetActive(! state);
        ToggleWorldObject(! state);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int segmentID, int objectID, float movementAngle, ToggleGroup toggleGroup)
    {
        SegmentID = segmentID;
        ObjectID = objectID;
        _toggle.group = toggleGroup;
        DistanceToObjective = 0f;
        DifferenceToRealPosition = 0f;
        MovementAngle = movementAngle;
        WorldObject = null;
    }

    public void InstantiateWorldObject(Vector3 Position, Transform parent)
    {
        WorldObject = Instantiate(ResourceManager.Instance.GetLandmarkObject(ObjectID),
            Position, Quaternion.identity, parent);
    }

    public void UpdateWorldObjectPosition(Vector3 newWorldPosition)
    {
        if (WorldObject == null) return;
        newWorldPosition.y = WorldObject.transform.localScale.y / 2;
        WorldObject.transform.position = newWorldPosition;
    }

    
    public void AutoSelect(bool autoSelect)
    {
        _toggle.isOn = autoSelect;
    }

    private void ToggleWorldObject(bool state) => WorldObject.SetActive(state);
}
