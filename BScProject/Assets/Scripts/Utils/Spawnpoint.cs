using UnityEngine;

public enum SpawnPointType { Experiment, Assessment };
public class SpawnPoint : MonoBehaviour
{
    public SpawnPointType type;
    [SerializeField] private GameObject _startHighlights;
    [SerializeField] private bool _showHighlight;
    [SerializeField] private bool _addMovementDetection;
    private MovementDetection _movementDetection;

    void Start()
    {
        if (_addMovementDetection)
        {
            if (!gameObject.TryGetComponent(out _movementDetection))
                _movementDetection = gameObject.AddComponent<MovementDetection>();
        }
        _startHighlights.SetActive(_showHighlight);
    }

    public void ToggleHighlight(bool state)
    {
        _startHighlights.SetActive(state);
    }

    public MovementDetection GetMovementDetection() => _movementDetection;

}