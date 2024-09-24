using UnityEngine;

public class ExperimentStartPointHandler : MonoBehaviour
{
    [SerializeField] private MovementDetection _experimentSpawnMovementDetection;

    public bool ExperimentReadyToStart = false;

    [SerializeField] private ParticleSystem StartPositionHighlights;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void Start()
    {
        if (_experimentSpawnMovementDetection != null)
        {
            _experimentSpawnMovementDetection.EnteredDectectionZone += HandleReadyToStart;
            _experimentSpawnMovementDetection.ExitedDectectionZone += HandleNotReadyToStart;
        }
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void HandleNotReadyToStart()
    {
        ExperimentReadyToStart = true;
    }

    private void HandleReadyToStart()
    {
        ExperimentReadyToStart = false;
    }
    
    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void SetExperimentStartPosition(Transform newTransform)
    {
        Vector3 newPosition = newTransform.position;
        newPosition.y = 0;
        transform.SetLocalPositionAndRotation(newPosition, newTransform.rotation);

    }

    public void ToggleStartPositionHighlight(bool state)
    {
        StartPositionHighlights.gameObject.SetActive(state);
    }
}

