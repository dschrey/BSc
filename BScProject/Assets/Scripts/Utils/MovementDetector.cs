using System;
using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public class MovementDetection : MonoBehaviour
{
    [SerializeField] private float _detectionRadius;
    [SerializeField] private SphereCollider _collider;
    public event Action ExitedDectectionZone;
    public event Action EnteredDectectionZone;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _collider = GetComponent<SphereCollider>();
        _collider.radius = _detectionRadius;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
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
