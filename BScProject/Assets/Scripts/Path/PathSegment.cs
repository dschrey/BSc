using System;
using UnityEngine;

public class PathSegment : MonoBehaviour 
{
    public PathSegmentData PathSegmentData;
    public event Action SegmentCompleted;
    public Objective Objective;
    public GameObject LandmarkObject;
    public GameObject SegmentObstacle;
    private MovementDetection _movementDetection;
    private GameObject _obstaclePrefab = null;
    
    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {        
        _movementDetection = GetComponent<MovementDetection>();
        if (_movementDetection == null)
        {
            Debug.LogError($"Could not find objective for {this} path segment.");
            return;
        }
        _movementDetection.ExitedDectectionZone += OnExitedDectectionZone;
        Objective = GetComponentInChildren<Objective>();
        if (Objective == null)
        {
            Debug.LogError($"Could not find objective for {this} path segment.");
            return;
        }
        Objective.ObjectiveCaptured += OnObjectiveCaptured;

    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnObjectiveCaptured()
    {
        if (LandmarkObject != null)
            LandmarkObject.SetActive(false);
        if (SegmentObstacle != null)
            SegmentObstacle.SetActive(false);
        SegmentCompleted?.Invoke();
    }
    
    private void OnExitedDectectionZone()
    {
        _movementDetection.enabled = false;
        SetObjectiveInvisible();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------------

    public void Initialize(PathSegmentData pathSegmentData, GameObject obstaclePrefab)
    {
        PathSegmentData = pathSegmentData;
        _obstaclePrefab = obstaclePrefab;
    }

    public void SetObjectiveInvisible()
    {
        Objective.HideObjective();
    }

    public void ShowSegmentObjective()
    {
        Objective.ShowObjective();
    }

    public void PlaySegmentObjectiveHint()
    {
        Objective.ShowObjectiveHint();
    }

    public void SpawnSegmentObjects()
    {
        SpawnHoverObject();
        SpawnLandmarkObject();
        SpawnSegmentObstacle();
    }

    private void SpawnHoverObject()
    {
        GameObject prefab = ResourceManager.Instance.GetHoverObject(PathSegmentData.ObjectiveObjectID);
        if (prefab == null)
        {
            Debug.LogWarning($"Objective object found.");
            return;
        }
        Objective.SpawnObject(prefab);
    }

    private  void SpawnLandmarkObject()
    {

        float angleInRadians = PathSegmentData.AngleToLandmark * Mathf.Deg2Rad;
        Vector3 relativePosition = new (
            PathSegmentData.LandmarkObjectDistanceToObjective * Mathf.Cos(angleInRadians),
            0,
            PathSegmentData.LandmarkObjectDistanceToObjective * Mathf.Sin(angleInRadians)
        );
        Vector3 objectSpawnpoint = transform.position + relativePosition;
        
        GameObject prefab = ResourceManager.Instance.GetLandmarkObject(PathSegmentData.LandmarkObjectID);
        if (prefab == null)
        {
            Debug.LogWarning($"Landmark object found.");
            return;
        }

        LandmarkObject = Instantiate(prefab, objectSpawnpoint, Quaternion.identity, transform);
    }

    private void SpawnSegmentObstacle()
    {
        if (!PathSegmentData.ShowObstacle) return;
        if (_obstaclePrefab == null)
        {
            Debug.LogError($"Obstacle prefab is null.");
            return;
        }
        SegmentObstacle = Instantiate(_obstaclePrefab, transform);
        Vector3 objectSpawnpoint = transform.position + PathSegmentData.RelativeObstaclePositionToObjective;
        objectSpawnpoint.y = SegmentObstacle.transform.position.y;
        SegmentObstacle.transform.SetPositionAndRotation(objectSpawnpoint, PathSegmentData.ObstacleRotation);
        SegmentObstacle.transform.localScale = PathSegmentData.Scale;
    }

}