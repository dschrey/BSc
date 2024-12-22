using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class UIGridObjectSpawner : MonoBehaviour
{
    [SerializeField] private RawImage _objectImage;
    [SerializeField] private XRSocketInteractor _socket;
    [SerializeField] private Texture _emptyTexture;
    public GameObject SpawnedObject = null;
    public int SelectionObjectID;
    public Action<int> ObjectGrabbed;
    private GameObject _selectionObject = null;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        _objectImage.texture = _emptyTexture;
        _socket.selectEntered.AddListener(OnObjectPlaced);
        _socket.selectExited.AddListener(OnObjectTaken);
    }

    private void OnDisable() 
    {
        _socket.selectEntered.RemoveListener(OnObjectPlaced);
        _socket.selectExited.RemoveListener(OnObjectTaken);
        if (SpawnedObject != null)
        {
            Destroy(SpawnedObject);
        }
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private async void OnObjectTaken(SelectExitEventArgs args)
    {
        ObjectGrabbed?.Invoke(SelectionObjectID);
        await SpawnNewObject(500);
    }

    private void OnObjectPlaced(SelectEnterEventArgs args)
    {
        SpawnedObject = args.interactableObject.transform.gameObject;
        Utils.ToggleObjectColliders(SpawnedObject, false);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int objectID, RenderTexture texture)
    {
        SelectionObjectID = objectID;
        _objectImage.texture = texture;
    }

    private async Task SpawnNewObject(int delay)
    {
        if (_selectionObject == null) return;
        if (_socket.hasSelection) return;

        GameObject newObject = Instantiate(_selectionObject, transform.position, transform.rotation);
        Utils.ToggleObjectColliders(newObject, false);
        XRGrabInteractable interactable = newObject.AddComponent<XRGrabInteractable>();
        interactable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        newObject.transform.localScale *= 0.35f;
        
        IXRSelectInteractable baseInteractor = interactable.GetComponent<XRGrabInteractable>();
        _socket.StartManualInteraction(baseInteractor);

        newObject.AddComponent<ObjectHoldOrDestroy>().Initialize(interactable);

        await Task.Delay(delay);
        if (newObject != null)
        {
            Utils.ToggleObjectColliders(newObject, true);
        }
    }

    public async void SpawnObject(GameObject obj)
    {
        _selectionObject = obj;
        await SpawnNewObject(0);
    }

    public async void DisableSelectionObject(int time)
    {
        await DisableObject(time);
    }

    private async Task DisableObject(int time)
    {
        Utils.ToggleObjectColliders(SpawnedObject, false);
        await Task.Delay(time);
        if (this != null && SpawnedObject != null)
        {
            Utils.ToggleObjectColliders(SpawnedObject, true);
        }
    }
}
