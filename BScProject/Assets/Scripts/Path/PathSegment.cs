using System;
using UnityEngine;

public class PathSegment : MonoBehaviour 
{
    public PathSegmentData PathSegmentData;
    public event Action SegmentCompleted;
    public Objective Objective;
    public GameObject SegmentObject;
    public GameObject SegmentObstacle;
    private MovementDetection _movementDetection;
    
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
        if (SegmentObject != null)
            SegmentObject.SetActive(false);
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

    public void Initialize(PathSegmentData pathSegmentData)
    {
        PathSegmentData = pathSegmentData;
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
        Debug.Log($"Spawning objects for segment (ID: {PathSegmentData.SegmentID}).");
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
        Vector3 objectSpawnpoint = transform.position + PathSegmentData.RelativeObjectPositionToObjective;
        
        GameObject prefab = ResourceManager.Instance.GetLandmarkObject(PathSegmentData.LandmarkObjectID);
        if (prefab == null)
        {
            Debug.LogWarning($"Landmark object found.");
            return;
        }

        SegmentObject = Instantiate(prefab, objectSpawnpoint, Quaternion.identity, transform);
        PathSegmentData.LandmarkObjectDistanceToObjective = Vector3.Distance(transform.position, objectSpawnpoint);
    }

    private void SpawnSegmentObstacle()
    {
        if (PathSegmentData.SegmentObstaclePrefab == null)
        {
            Debug.LogWarning($"Obstacle object found.");
            return;
        }
        SegmentObstacle = Instantiate(PathSegmentData.SegmentObstaclePrefab, transform);
    }

}