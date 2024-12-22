using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ObjectHoldOrDestroy : MonoBehaviour
{
    [SerializeField] private float _destroyDelaySec = 5f;
    private XRGrabInteractable _grabInteractable;
    private Coroutine _removalCoroutine;

    private bool initialized = false;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnDestroy() 
    {
        _grabInteractable.selectEntered.RemoveListener(OnObjectGrabbed);
        _grabInteractable.selectExited.RemoveListener(OnObjectReleased);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnObjectGrabbed(SelectEnterEventArgs args)
    {
        if (initialized)
            Utils.ToggleObjectRenderers(gameObject, true);
        if (_removalCoroutine != null)
        {
            StopCoroutine(_removalCoroutine);
            _removalCoroutine = null;
        }
    }

    private void OnObjectReleased(SelectExitEventArgs args)
    {
        _removalCoroutine = StartCoroutine(DestroyAfterDelay());
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(XRGrabInteractable interactable, bool visible = false)
    {
        _grabInteractable = interactable;
        _grabInteractable.selectEntered.AddListener(OnObjectGrabbed);
        _grabInteractable.selectExited.AddListener(OnObjectReleased);
        Utils.ToggleObjectRenderers(gameObject, visible);
        initialized = true;
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(_destroyDelaySec);
        Destroy(gameObject);
    }
}
