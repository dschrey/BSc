using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathGuidance : MonoBehaviour
{
    [SerializeField] private int _numPoints = 20;
    [SerializeField] private float _arcOffset = 1.0f;
    [SerializeField] private float _arcWidth = 1.0f;
    [SerializeField] private float _arcDepth = 0.2f;
    private LineRenderer lineRenderer;
    [SerializeField] private Vector3 _playerPosition;
    [SerializeField] private Vector3 _targetPosition;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = _numPoints;
    }
    void Update()
    {
        DrawArc();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(Transform player, Transform target)
    {
        _playerPosition = player.position;
        _targetPosition = target.position;
    }

    public void DrawArc()
    {
        if (!lineRenderer) return;

        Vector3 direction = (_targetPosition - _playerPosition).normalized;
        direction.y = 0;

        Vector3 arcMiddle = _playerPosition + direction * _arcOffset;
        arcMiddle.y = 0;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

        Vector3 leftPoint = arcMiddle - 0.5f * _arcWidth * perpendicular + direction * _arcDepth;
        Vector3 rightPoint = arcMiddle + 0.5f * _arcWidth * perpendicular + direction * _arcDepth;

        DrawBezierArc(leftPoint, arcMiddle, rightPoint);
    }

    private void DrawBezierArc(Vector3 left, Vector3 mid, Vector3 right)
    {
        Vector3[] positions = new Vector3[_numPoints];

        for (int i = 0; i < _numPoints; i++)
        {
            float t = i / (float)(_numPoints - 1);
            positions[i] = QuadraticBezier(left, mid, right, t);
        }

        lineRenderer.SetPositions(positions);
    }

    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
    }
}
