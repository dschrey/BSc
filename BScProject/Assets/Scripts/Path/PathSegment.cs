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
        Debug.Log($"PathSegment :: SpawnSegmentObjects : Spawning objects for segment (ID: {PathSegmentData.SegmentID}).");
        SpawnHoverObject();
        SpawnLandmarkObject();
        SpawnSegmentObstacle();
    }

    private void SpawnHoverObject()
    {
        Objective.SpawnObject(ResourceManager.Instance.GetHoverObject(PathSegmentData.ObjectiveObjectID));
    }

    private  void SpawnLandmarkObject()
    {
        Vector3 objectSpawnpoint = transform.position + PathSegmentData.RelativeObjectPositionToObjective;
        
        SegmentObject = Instantiate(ResourceManager.Instance.GetLandmarkObject(PathSegmentData.LandmarkObjectID), objectSpawnpoint, Quaternion.identity, transform);
        PathSegmentData.LandmarkObjectDistanceToObjective = Vector3.Distance(transform.position, objectSpawnpoint);
        Debug.Log($"Spawned at {SegmentObject.transform.localPosition}");
    }

    private void SpawnSegmentObstacle()
    {
        SegmentObstacle = Instantiate(PathSegmentData.SegmentObstaclePrefab, transform);
    }

}