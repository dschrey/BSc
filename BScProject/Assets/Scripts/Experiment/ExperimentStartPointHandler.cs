using UnityEngine;

public class ExperimentStartPointHandler : MonoBehaviour
{
    [SerializeField] private MovementDetection _experimentSpawnMovementDetection;
    [SerializeField] private GameObject _startPositionHighlights;
    [SerializeField] private GameObject _experimentArea;
    public bool ExperimentReadyToStart = false;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void Start()
    {
        if (_experimentSpawnMovementDetection != null)
        {
            _experimentSpawnMovementDetection.EnteredDectectionZone += HandleReadyToStart;
            _experimentSpawnMovementDetection.ExitedDectectionZone += HandleNotReadyToStart;
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(6, 3, 10));
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void HandleReadyToStart()
    {
        ExperimentReadyToStart = true;
    }

    private void HandleNotReadyToStart()
    {
        ExperimentReadyToStart = false;
    }
    
    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void SetExperimentStartPosition(Transform newTransform)
    {
        Vector3 newPosition = newTransform.position;
        newPosition.y = 0;
        _experimentArea.transform.SetLocalPositionAndRotation(newPosition, newTransform.rotation);

    }

    public void ToggleStartPositionHighlight(bool state)
    {
        _startPositionHighlights.SetActive(state);
    }
}

