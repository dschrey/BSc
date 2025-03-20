using UnityEngine;

public enum SpawnPointType { Experiment, Assessment };
public class SpawnPoint : MonoBehaviour
{
    public SpawnPointType type;
    [SerializeField] private GameObject _startPositionHighlights;
    [SerializeField] private bool _showHighlight;
    [SerializeField] private bool _addMovementDetection;
    private MovementDetection _movementDetection;

    void Start()
    {
        if (_addMovementDetection)
        {
            if (!gameObject.TryGetComponent<MovementDetection>(out _movementDetection))
                _movementDetection = gameObject.AddComponent<MovementDetection>();
        }
        _startPositionHighlights.SetActive(_showHighlight);
    }

    public void ToggleStartPositionHighlight(bool state)
    {
        _startPositionHighlights.SetActive(state);
    }

    public MovementDetection GetMovementDetection() => _movementDetection;

}