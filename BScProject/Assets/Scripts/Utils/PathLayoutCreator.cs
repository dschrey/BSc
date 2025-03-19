using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SegmentObjectData : MonoBehaviour
{
    public int SegmentID = -1;
    // public int AssignedHoverObjectSocketID = -1;
    // public int AssignedHoverObjectID = -1;
    // public int AssignedLandmarkObjectSocketID = -1;
    // public int AssignedLandmarkObjectID = -1;
    public float AngleFromPrevSegmentRad;
    public LineRenderer ArrowRenderer;
    public Vector3 ArrowDirection;
    private ParticleSystem _particleSystem;
    public Color SegmentColor = Color.white;

    private void OnEnable()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        if (_particleSystem == null)
        {
            Debug.Log($"Could not find particle system.");
            return;
        }
    }

    public void SetParticleScale()
    {
        Vector3 particleScale = new()
        {
            x = 2 * DataManager.Instance.Settings.PlayerDetectionRadius,
            y = _particleSystem.transform.localScale.y,
            z = 2 * DataManager.Instance.Settings.PlayerDetectionRadius
        };
        _particleSystem.transform.localScale = particleScale;
    }

    public void SetColor(Color color)
    {
        var mainModule = _particleSystem.main;
        mainModule.startColor = color;
        SegmentColor = color;
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
    [SerializeField] private float _arrowOffest = 0.0f;
    public LineController DistanceLineController;
    public List<SegmentObjectData> SpawnedSegments = new();
    public RenderTexture RenderTexture => RenderCamera.GetCamera().targetTexture;
    private List<GameObject> _spawnedArrows = new();

    [Header("Segment Distance Properties")]
    public RectTransform CanvasMovementArea;
    public event Action<int, bool> SegmentBoundaryStatusUpdate;

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void CreatePathLayout(List<PathSegmentData> pathSegments, List<float> fakeSegmentAngles = null)
    {
        _arrowOffest = DataManager.Instance.Settings.PlayerDetectionRadius;

        List<GameObject> displayObjects = new();
        Vector3 currentPosition = transform.position;
        SegmentObjectData pathStart = Instantiate(_pathSegmentPrefab, currentPosition,
            Quaternion.identity, transform).AddComponent<SegmentObjectData>();
        pathStart.SetParticleScale();
        pathStart.SetColor(Color.white);
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

            Vector3 relativePosition = new(
                DataManager.Instance.Settings.SegmentLength * Mathf.Cos(angleInRadians),
                0,
                DataManager.Instance.Settings.SegmentLength * Mathf.Sin(angleInRadians)
            );

            Vector3 spawnPosition = currentPosition + relativePosition;
            Vector3 direction = (spawnPosition - currentPosition).normalized;
            Vector3 arrowBase = currentPosition + direction * _arrowOffest;
            Vector3 arrowHead = spawnPosition - direction * _arrowOffest;

            LineRenderer lineRenderer = Instantiate(_arrowRenderPrefab, transform).GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, arrowHead);
            lineRenderer.SetPosition(1, arrowBase);
            _spawnedArrows.Add(lineRenderer.gameObject);

            SegmentObjectData segment = Instantiate(_pathSegmentPrefab, spawnPosition, Quaternion.identity, transform).AddComponent<SegmentObjectData>();
            segment.SegmentID = pathSegmentData.SegmentID;
            segment.SetParticleScale();
            segment.SetColor(pathSegmentData.SegmentColor);
            segment.ArrowRenderer = lineRenderer;
            segment.ArrowDirection = direction;
            segment.AngleFromPrevSegmentRad = angleInRadians;
            SpawnedSegments.Add(segment);
            currentPosition = spawnPosition;
            displayObjects.Add(segment.gameObject);
        }

        displayObjects.AddRange(_spawnedArrows);
        Utils.AdjustCameraToObjects(RenderCamera.GetCamera(), displayObjects);
    }

    public void ClearPath()
    {
        SpawnedSegments.ForEach(obj => Destroy(obj.gameObject));
        SpawnedSegments.Clear();
        _spawnedArrows.ForEach(obj => Destroy(obj));
        _spawnedArrows.Clear();
    }

    public void ResetCameraView()
    {
        RenderCamera.GetCamera().orthographicSize = 3;
        RenderCamera.GetCamera().transform.position = new Vector3(_previewArea.transform.position.x, 3, _previewArea.transform.position.z);
    }

    // ---------- Segment Distance Functions ------------------------------------------------------------------------------------------------------

    public (Vector3 position, Vector2 size) CalculateSelectorProperties(LineRenderer lineRenderer)
    {
        Bounds bounds = Utils.CalculateLineRendererBounds(lineRenderer);

        Vector3 centerBounds = bounds.center;
        Vector3 extents = bounds.extents;
        Vector3 topLeftScreen = RenderCamera.WorldCoordinatesToScreenSpace(centerBounds + new Vector3(-extents.x, extents.y, -extents.z));
        Vector3 bottomRightScreen = RenderCamera.WorldCoordinatesToScreenSpace(centerBounds + new Vector3(extents.x, -extents.y, extents.z));
        Vector3 canvasArrowCenter = RenderCamera.WorldCoordinatesToScreenSpace(centerBounds);
        Vector2 screenSize = new(
            Mathf.Abs(topLeftScreen.x - bottomRightScreen.x),
            Mathf.Abs(topLeftScreen.y - bottomRightScreen.y)
        );

        Vector2 objectSize = screenSize * (CanvasMovementArea.sizeDelta / new Vector2(Screen.width, Screen.height));
        if (objectSize.x == 0)
            objectSize = new Vector2(75.0f, objectSize.y);
        if (objectSize.y == 0)
            objectSize = new Vector2(objectSize.x, 100.0f);
        return (canvasArrowCenter, objectSize);
    }

    public async Task<bool> AdjustArrowLength(int segmentID, float newLength, bool posAdjustment, List<SegmentArrowSelection> segmentDistanceData)
    {
        SegmentObjectData segment = SpawnedSegments.Find(s => s.SegmentID == segmentID).GetComponent<SegmentObjectData>();

        Vector3 arrowDirection = segment.ArrowDirection;
        Vector3 previousSegmentPos = SpawnedSegments.Find(s => s.SegmentID == (segmentID - 1)).transform.position;

        Vector3 newSegmentPosition = previousSegmentPos + (arrowDirection * newLength);
        Vector3 arrowBase = previousSegmentPos + arrowDirection * _arrowOffest;
        Vector3 arrowHead = newSegmentPosition - arrowDirection * _arrowOffest;

        Vector3 localSegmentPos = segment.transform.parent.InverseTransformPoint(newSegmentPosition);

        bool validPosition = ValidatePosition(localSegmentPos);
        if (!validPosition)
        {
            SegmentBoundaryStatusUpdate?.Invoke(segmentID, true);
            return false;
        }
        SegmentArrowSelection segmentArrow = segmentDistanceData.Find(data => data.SegmentID == segment.SegmentID);

        if (segmentArrow.hitBoundary && posAdjustment)
        {
            return false;
        }

        SegmentBoundaryStatusUpdate?.Invoke(segmentID, false);

        return await PerformSegmentdjustment(segment, newSegmentPosition, arrowBase, arrowHead, segmentDistanceData);
    }

    private async Task<bool> PerformSegmentdjustment(SegmentObjectData segment, Vector3 newSegmentPosition, Vector3 arrowBase, Vector3 arrowHead, List<SegmentArrowSelection> segmentDistanceData)
    {
        bool success = await AdjustSegment(segment.SegmentID + 1, newSegmentPosition, segmentDistanceData);

        foreach (SegmentArrowSelection segmentData in segmentDistanceData)
        {
            if (segmentData.SegmentID <= segment.SegmentID) continue;
            if (segmentData.hitBoundary)
            {
                return false;
            }
        }

        segment.transform.position = newSegmentPosition;

        LineRenderer lineRenderer = SpawnedSegments.Find(s => s.SegmentID == segment.SegmentID).ArrowRenderer;
        lineRenderer.SetPosition(0, arrowHead);
        lineRenderer.SetPosition(1, arrowBase);

        SegmentArrowSelection segmentArrow = segmentDistanceData.Find(data => data.SegmentID == segment.SegmentID);
        (Vector3 position, Vector2 size) = CalculateSelectorProperties(lineRenderer);
        segmentArrow.RectTransform.anchoredPosition = position;
        segmentArrow.RectTransform.sizeDelta = size;

        return true;
    }

    private async Task<bool> AdjustSegment(int segmentID, Vector3 newPosOfPreviousSegment, List<SegmentArrowSelection> segmentDistanceData)
    {
        SegmentObjectData segment = SpawnedSegments.Find(s => s.SegmentID == segmentID);
        if (segment == null) return false;

        SegmentArrowSelection segmentArrow = segmentDistanceData.Find(data => data.SegmentID == segmentID);

        float distance = segmentArrow.Length;

        Vector3 relativePosition = new(
            distance * Mathf.Cos(segment.AngleFromPrevSegmentRad),
            0,
            distance * Mathf.Sin(segment.AngleFromPrevSegmentRad)
        );

        Vector3 oldSegmentPosition = segment.transform.position;
        Vector3 newSegmentPosition = newPosOfPreviousSegment + relativePosition;

        Vector3 localSegmentPos = segment.transform.parent.InverseTransformPoint(newSegmentPosition);
        bool validPosition = ValidatePosition(localSegmentPos);
        if (!validPosition)
        {
            SegmentBoundaryStatusUpdate?.Invoke(segmentID, true);
            return false;
        }

        if (segmentArrow.hitBoundary)
        {
            SegmentBoundaryStatusUpdate?.Invoke(segmentID, false);
        }

        await AdjustSegment(segmentID + 1, newSegmentPosition, segmentDistanceData);

        foreach (SegmentArrowSelection segmentData in segmentDistanceData)
        {
            if (segmentData.SegmentID <= segmentID) continue;
            if (segmentData.hitBoundary)
            {
                return false;
            }
        }

        segment.transform.position = newSegmentPosition;

        LineRenderer lineRenderer = SpawnedSegments.Find(s => s.SegmentID == segmentID).ArrowRenderer;
        Vector3 arrowBase = newPosOfPreviousSegment + segment.ArrowDirection * _arrowOffest;
        Vector3 arrowHead = newSegmentPosition - segment.ArrowDirection * _arrowOffest;
        lineRenderer.SetPosition(0, arrowHead);
        lineRenderer.SetPosition(1, arrowBase);

        (Vector3 position, Vector2 size) = CalculateSelectorProperties(lineRenderer);
        segmentArrow.RectTransform.anchoredPosition = position;
        segmentArrow.RectTransform.sizeDelta = size;

        return true;
    }

    private bool ValidatePosition(Vector3 objectPosition)
    {
        float halfWidth = DataManager.Instance.Settings.MovementArea.x / 2;
        float halfHeight = DataManager.Instance.Settings.MovementArea.y / 2;

        float objectRadius = DataManager.Instance.Settings.PlayerDetectionRadius;

        Vector3[] cornerPositions =
        {
            new(objectPosition.x + objectRadius, objectPosition.y , objectPosition.z + objectRadius),
            new(objectPosition.x + objectRadius, objectPosition.y , objectPosition.z - objectRadius),
            new(objectPosition.x - objectRadius, objectPosition.y , objectPosition.z + objectRadius),
            new(objectPosition.x - objectRadius, objectPosition.y , objectPosition.z - objectRadius)
        };

        foreach (Vector3 corner in cornerPositions)
        {
            if (corner.z < -halfWidth || corner.z > halfWidth)
            {
                return false;
            }
            else if (corner.x < -halfHeight || corner.x > halfHeight)
            {
                return false;
            }
        }

        return true;
    }

    public LineController CreateLineController()
    {
        return Instantiate(_lineRenderPrefab, transform).GetComponent<LineController>();
    }
}
