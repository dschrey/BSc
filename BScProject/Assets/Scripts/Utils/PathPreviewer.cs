using System.Collections.Generic;
using UnityEngine;


public class PathPreviewer : MonoBehaviour
{
    [SerializeField] private Transform _pathSpawn;
    [SerializeField] private GameObject _pathPrefabObject;
    [SerializeField] private PathLayoutManager _pathLayoutManager;
    
    [Header("Path Data")]
    public PathData pathData;
    private Path _createdPath = null;
    public bool UpdateObjects = false;
    public bool Refresh = false;
    public bool UpdateFakePaths = false;
    public List<PathSegmentData> SegmentsData = new();


    private void Start()
    {
        pathData.SegmentsData.ForEach(data => SegmentsData.Add(data));
        pathData.Update += OnInspectorUpdate;
        if (InitializePath()) UpdateObjects = true;
    }

    private void Update()
    {
        if (UpdateFakePaths)
        {
            UpdateFakePaths = false;
            _pathLayoutManager.CreatePreview(pathData);
            return;
        }
        if (Refresh)
        {
            Refresh = false;
            Destroy(_createdPath.gameObject);
            InitializePath();
            return;
        }

        if (pathData == null)
        {
            if (_createdPath != null) Destroy(_createdPath.gameObject);
            SegmentsData.Clear();
            return;
        }

        if (pathData != null && _createdPath == null)
        {
            pathData.SegmentsData.ForEach(data => SegmentsData.Add(data));
            pathData.Update += OnInspectorUpdate;
        }
        if (! UpdateObjects) return;

        if (SegmentsData.Count != pathData.SegmentsData.Count)
        {
            UpdateObjects = false;
            Destroy(_createdPath.gameObject);
            if (InitializePath()) UpdateObjects = true;
            return;
        }
        
        foreach (PathSegment segment in _createdPath.Segments)
        {
            UpdateSegmentPosition(segment);
            UpdateLandmarkPosition(segment);
            UpdateObstaclePosition(segment);
        }
    }

    private void UpdateObstaclePosition(PathSegment segment)
    {
        GameObject obstacle = segment.SegmentObstacle;
        if (obstacle == null) return;
        Vector3 relativePosition = obstacle.transform.position - segment.transform.position;
        relativePosition.y = obstacle.transform.localScale.y / 2;
        segment.PathSegmentData.RelativeObstaclePositionToObjective = relativePosition;
        segment.PathSegmentData.Scale = obstacle.transform.localScale;
    }

    private void UpdateLandmarkPosition(PathSegment segment)
    {
        GameObject landmark = segment.LandmarkObject;
        if (landmark == null) return;
        Vector3 relativePosition = landmark.transform.position - segment.transform.position;
        segment.PathSegmentData.RelativeLandmarkPositionToObjective = relativePosition;
    }

    private bool InitializePath()
    {
        _createdPath = Instantiate(_pathPrefabObject, _pathSpawn.transform).GetComponent<Path>();
        _createdPath.Initialize(pathData);
        foreach (PathSegment segment in _createdPath.Segments)
        {
            segment.gameObject.SetActive(true);
        }
        return true;
    }


    private void UpdateSegmentPosition(PathSegment segment)
    {
        int segmentID = segment.PathSegmentData.SegmentID;
        int previousSegmenID = segmentID - 1;
        Vector3 direction = Vector3.zero;
        float distance;
        if (segmentID == 0)
        {
            distance = Vector3.Distance(segment.transform.position, _pathSpawn.position);
            direction = segment.transform.position - _pathSpawn.position;

        }
        else
        {
            PathSegment previousSegment = _createdPath.GetPathSegment(previousSegmenID);
            distance = Vector3.Distance(segment.transform.position, previousSegment.transform.position);
            direction = segment.transform.position - previousSegment.transform.position;
        }
        PathSegmentData pathSegmentData = pathData.GetSegmentData(segmentID);
        pathSegmentData.DistanceToPreviousSegment = distance;
        
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        pathSegmentData.AngleFromPreviousSegment = angle;
    }

    private void OnInspectorUpdate()
    {
        #if UNITY_EDITOR
            if (!Application.isPlaying) return;
        #endif

        if (UpdateObjects) return;
        Destroy(_createdPath.gameObject);
        InitializePath();
    }
}
