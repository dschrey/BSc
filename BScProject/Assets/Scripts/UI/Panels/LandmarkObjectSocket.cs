using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class LandmarkObjectSocket : MonoBehaviour
{
    public GameObject SocketObject = null;
    private RectTransform _rectTransform;
    public Vector2 Position
    {
        get => _rectTransform.anchoredPosition;
        set => _rectTransform.anchoredPosition = value;
    }
    private XRSocketInteractor _socket;
    public bool IsOccupied => _socket.hasSelection;
    [SerializeField] private Image _background;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _rectTransform = GetComponent<RectTransform>();
        _socket = GetComponent<XRSocketInteractor>();
    }

    // ---------- Listener Methods ---------------------------------------------------------------------------------------------------------------------
    
    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(Color color)
    {
        _background.color = color;
    }

    public void PlaceSocketObject(GameObject socketObject)
    {
        SocketObject = Instantiate(socketObject, _socket.attachTransform.position, _socket.attachTransform.rotation);
        Utils.SetObjectColliders(SocketObject, false);
        SocketObject.transform.localScale *= 0.2f;

        IXRSelectInteractable baseInteractor = SocketObject.AddComponent<XRGrabInteractable>();
        _socket.StartManualInteraction(baseInteractor);
    }

    public void RemoveSocketObject()
    {
        if (SocketObject != null)
            Destroy(SocketObject);
        SocketObject = null;
    }

    public void ToggleSocketVisual(bool state)
    {
        if (SocketObject != null)
            SocketObject.SetActive(state);
        _background.enabled = state;
        if (! state) RemoveSocketObject();
    }

    public void RespawnSocketObject()
    {
        GameObject currentObject = SocketObject;
        RemoveSocketObject();
        if (currentObject == null) return;
        SocketObject = Instantiate(currentObject, _socket.attachTransform.position, _socket.attachTransform.rotation);
        IXRSelectInteractable baseInteractor = SocketObject.GetComponent<XRGrabInteractable>();
        _socket.StartManualInteraction(baseInteractor);
    }
}
