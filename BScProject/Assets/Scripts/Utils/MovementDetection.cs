using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class MovementDetection : MonoBehaviour
{
    public event Action PlayerEnteredDectectionZone;
    public event Action PlayerExitedDectectionZone;
    private SphereCollider _collider;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _collider = GetComponent<SphereCollider>();
        _collider.radius = DataManager.Instance.Settings.PlayerDetectionRadius;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, DataManager.Instance.Settings.PlayerDetectionRadius);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            PlayerEnteredDectectionZone?.Invoke();
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            PlayerExitedDectectionZone?.Invoke();
        }        
    }
}
