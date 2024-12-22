using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class ObjectSelectionSocket : MonoBehaviour
{
    public int BelongsToSegmentID;
    public int SocketID;
    public event Action<int, int, GameObject> OnSocketObjectChanged;
    public GameObject SocketObject = null;
    public bool isOccupied = false;
    [SerializeField] private Image _socketIndicator;
    private XRSocketInteractor _socket;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _socket = GetComponent<XRSocketInteractor>();
        _socket.selectEntered.AddListener(OnObjectPlaced);
        _socket.selectExited.AddListener(OnObjectRemoved);
    }

    void OnDisable() 
    {
        _socket.selectEntered.RemoveListener(OnObjectPlaced);
        _socket.selectExited.RemoveListener(OnObjectRemoved);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------


    private void OnObjectPlaced(SelectEnterEventArgs args)
    {
        SocketObject = args.interactableObject.transform.gameObject;
        isOccupied = true;
        OnSocketObjectChanged?.Invoke(BelongsToSegmentID, SocketID, args.interactableObject.transform.gameObject);
    }

    private void OnObjectRemoved(SelectExitEventArgs args)
    {
        SocketObject = null;
        isOccupied = false;
        OnSocketObjectChanged?.Invoke(BelongsToSegmentID, SocketID, null);

    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------


    public void Initialize(int belongsToSegment, int socketID, Color color)
    {
        BelongsToSegmentID = belongsToSegment;
        SocketID = socketID;
        _socketIndicator.color = color;
    }

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
                    Debug.Log($"Deleting obecjt {placedObject.name} of socket {SocketID}");
                    Destroy(placedObject);
                }
            }
        }
    }

    public void InitializeSocketObject(GameObject socketObject)
    {
        GameObject newObject = Instantiate(socketObject, _socket.attachTransform.position, _socket.attachTransform.rotation);
        XRGrabInteractable interactable = newObject.AddComponent<XRGrabInteractable>();
        interactable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        newObject.transform.localScale *= 0.35f;
        
        IXRSelectInteractable baseInteractor = interactable.GetComponent<XRGrabInteractable>();
        _socket.StartManualInteraction(baseInteractor);

        newObject.AddComponent<ObjectHoldOrDestroy>().Initialize(interactable, true);
    }

}
