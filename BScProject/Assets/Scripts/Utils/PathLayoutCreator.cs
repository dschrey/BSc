using System.Collections.Generic;
using UnityEngine;

public class SegmentObjectData : MonoBehaviour
{
    public int SegmentID = -1;
    public int AssignedHoverObjectSocketID = -1;
    public int AssignedHoverObjectID = -1;
    public int AssignedLandmarkObjectSocketID = -1;
    public int AssignedLandmarkObjectID = -1;
    public Vector3 CanvasSocketPosition = Vector3.zero;
    private ParticleSystem _particleSystem;

    private void OnEnable()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        if (_particleSystem == null)
        {
            Debug.Log($"Could not find particle system.");
            return;
        }
    }

    public void SetColor(Color color)
    {
        var mainModule = _particleSystem.main;
        mainModule.startColor = color;
    }
}

public class PathLayoutCreator : MonoBehaviour
{
    public int PathLayoutID;
    public RenderTexture LayoutRenderTexture;
    public CanvasCameraHandler RenderCamera;
    [SerializeField] private GameObject _lineRenderPrefab;
    [SerializeField] private GameObject _arrowRenderPrefab;
    [SerializeField] private GameObject _pathSegmentPrefab;
    [SerializeField] private GameObject _previewArea;
    public LineController DistanceLineController;
    public List<Vector3> SegmentCanvasPositions = new();
    public List<SegmentObjectData> SpawnedSegments = new();
    public RenderTexture RenderTexture => RenderCamera.GetCamera().targetTexture;
    private List<GameObject> _spawnedArrows = new();
    [SerializeField] private float _startingSegmentLength = 2.0f;


    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void CreatePathLayout(List<PathSegmentData> pathSegments, List<float> fakeSegmentAngles = null)
    {
        List<GameObject> displayObjects = new();
        Vector3 currentPosition = transform.position;
        SegmentObjectData pathStart = Instantiate(_pathSegmentPrefab, currentPosition,
            Quaternion.identity, transform).AddComponent<SegmentObjectData>();
        SpawnedSegments.Add(pathStart);
        displayObjects.Add(pathStart.gameObject);

        int segmentCount = 0;
        foreach (PathSegmentData pathSegmentData in pathSegments)
        {
            float angleInRadians = 0f;
            if (fakeSegmentAngles != null)
            {
                angleInRadians = fakeSegmentAngles[segmentCount] * Mathf.Deg2Rad;
                segmentCount++;
            }
            else
            {
                angleInRadians = pathSegmentData.AngleFromPreviousSegment * Mathf.Deg2Rad;
            }

            
            Vector3 relativePosition = new (
                _startingSegmentLength * Mathf.Cos(angleInRadians),
                0,
                _startingSegmentLength * Mathf.Sin(angleInRadians)
            );
            Vector3 spawnPosition = currentPosition + relativePosition;

            // Draw arrow with offset
            float offset = 0.35f;
            Vector3 direction = (spawnPosition - currentPosition).normalized;
            Vector3 adjustedStart = currentPosition + direction * offset;
            Vector3 adjustedEnd = spawnPosition - direction * offset;

            LineRenderer lineRenderer = Instantiate(_arrowRenderPrefab, transform).GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, adjustedEnd);
            lineRenderer.SetPosition(1, adjustedStart);
            _spawnedArrows.Add(lineRenderer.gameObject);

            // Draw segment
            SegmentObjectData segment = Instantiate(_pathSegmentPrefab, spawnPosition, Quaternion.identity, transform).AddComponent<SegmentObjectData>();
            segment.SegmentID = pathSegmentData.SegmentID;
            segment.SetColor(pathSegmentData.SegmentColor);
            SpawnedSegments.Add(segment);
            currentPosition = spawnPosition; 
            displayObjects.Add(segment.gameObject);
        }

        displayObjects.AddRange(_spawnedArrows);
        Utils.AdjustCameraToObjects(RenderCamera.GetCamera(), displayObjects);
    }

    private void DrawCircle(Vector3 position, int points, float radius, Color color)
    {
        LineRenderer lineRenderer = Instantiate(_lineRenderPrefab, transform).GetComponent<LineRenderer>();

        lineRenderer.loop = true;
        lineRenderer.positionCount = points;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        for (int currentPoint = 0; currentPoint < points; currentPoint++)
        {
            float currentCircumference = (float)currentPoint / points;
            float currentRadian = currentCircumference * 2 * Mathf.PI;

            float x = radius * Mathf.Cos(currentRadian);
            float y = radius * Mathf.Sin(currentRadian);

            Vector3 drawPosition = position + new Vector3(x, 0.1f, y);
            lineRenderer.SetPosition(currentPoint, drawPosition);
        }
    }

    public void ClearPath()
    {
        SegmentCanvasPositions.Clear();
        SpawnedSegments.ForEach(obj => Destroy(obj.gameObject));
        _spawnedArrows.ForEach(obj => Destroy(obj));
    }

    public void ResetCameraView()
    {
        RenderCamera.GetCamera().orthographicSize = 3;
        RenderCamera.GetCamera().transform.position = new Vector3(_previewArea.transform.position.x, 3, _previewArea.transform.position.z);
    }

}
