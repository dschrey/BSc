using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor), typeof(Toggle))]
public class ObjectPlacementSocket : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int BelongsToSegmentID;
    public int SocketID;
    public event Action<int, int> OnSocketObjectChanged;
    public event Action<int, int> PreviewObjectChanged;
    public event Action<int, int> PreviewObjectRemoved;
    public GameObject SocketObject = null;
    [SerializeField] private Image _socketIndicator;
    [SerializeField] private Image _highlightImage;
    private XRSocketInteractor _socket;
    public bool IsOccupied => _socket.hasSelection;
    private Toggle _toggle;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _socket = GetComponent<XRSocketInteractor>();
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnSocketStateChanged);
    }


    void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(OnSocketStateChanged);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        PreviewObjectChanged?.Invoke(BelongsToSegmentID, SocketID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PreviewObjectRemoved?.Invoke(BelongsToSegmentID, SocketID);
    }

    private void OnSocketStateChanged(bool state)
    {
        if (state)
        {
            OnSocketObjectChanged?.Invoke(BelongsToSegmentID, SocketID);
        }
        else
        {
            OnSocketObjectChanged?.Invoke(BelongsToSegmentID, -1);
        }
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int belongsToSegment, int socketID, Color color, ToggleGroup group)
    {
        BelongsToSegmentID = belongsToSegment;
        SocketID = socketID;
        _socketIndicator.color = color;
        Color highlightColor = color;
        highlightColor.a = 0.15f;
        _highlightImage.color = highlightColor;
        _toggle.group = group;
    }

    [Obsolete]
    public void DeleteSocketObject()
    {
        if (_socket.hasSelection)
        {
            IXRHoverInteractable interactable = _socket.GetOldestInteractableHovered();
            if (interactable != null)
            {
                GameObject placedObject = interactable.transform.gameObject;
                if (placedObject != null)
                {
                    Destroy(placedObject);
                }
            }
        }
    }

    public void PlaceSocketObject(GameObject socketObject)
    {
        SocketObject = Instantiate(socketObject, _socket.attachTransform.position, _socket.attachTransform.rotation);
        Utils.SetObjectColliders(SocketObject, false);
        SocketObject.transform.localScale *= 0.35f;

        IXRSelectInteractable baseInteractor = SocketObject.AddComponent<XRGrabInteractable>();
        _socket.StartManualInteraction(baseInteractor);
    }

    public void RemoveSocketObject()
    {
        if (SocketObject != null)
            Destroy(SocketObject);
        SocketObject = null;
    }

    public void Select()
    {
        _toggle.isOn = true;
    }

    public void Deselect()
    {
        _toggle.isOn = false;
    }
}
