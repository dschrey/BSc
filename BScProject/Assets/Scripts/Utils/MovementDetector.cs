using System;
using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public class MovementDetection : MonoBehaviour
{
    public event Action ExitedDectectionZone;
    public event Action EnteredDectectionZone;
    private SphereCollider _collider;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _collider = GetComponent<SphereCollider>();
        _collider.radius = DataManager.Instance.Settings.PlayerDetectionRadius;
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            EnteredDectectionZone?.Invoke();
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            ExitedDectectionZone?.Invoke();
        }        
    }
}
