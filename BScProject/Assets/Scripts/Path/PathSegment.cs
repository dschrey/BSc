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

    private void OnDestroy()
    {
        // Debug.Log($"PathSegment :: OnDestroy() : Attempting release of render textures for segment (ID: {PathSegmentData.SegmentID}).");
        // if (PathSegmentData.SegmentObjectRenderTexture != null)
        // {
        //     PathSegmentData.SegmentObjectRenderTexture.Release();
        //     Debug.Log($" --- Released segment object texture.");
        // }

        // if (PathSegmentData.ObjectiveObjectRenderTexture != null)
        // {
        //     PathSegmentData.ObjectiveObjectRenderTexture.Release();
        //     Debug.Log($" --- Released objective object texture.");
        // }
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

    public void Initialize(PathSegmentData pathSegmentData, float distance)
    {
        PathSegmentData = pathSegmentData;
        PathSegmentData.DistanceToPreviousSegment = distance;
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
        SpawnObjectiveObject();
        SpawnSegmentObject();
        SpawnSegmentObstacle();
    }

    private void SpawnObjectiveObject()
    {
        Objective.SpawnObject(ResourceManager.Instance.GetObjectiveObject(PathSegmentData.ObjectiveObjectID));
    }

    private  void SpawnSegmentObject()
    {
        Vector3 objectSpawnpoint = transform.position + PathSegmentData.RelativeObjectPositionToObjective;
        
        SegmentObject = Instantiate(ResourceManager.Instance.GetSegmentObject(PathSegmentData.SegmentObjectID), objectSpawnpoint, Quaternion.identity, transform);
        PathSegmentData.ObjectDistanceToObjective = Vector3.Distance(transform.position, objectSpawnpoint);
    }

    private void SpawnSegmentObstacle()
    {
        Debug.Log($"Prefab pos {PathSegmentData.SegmentObstaclePrefab.transform.position}");
        SegmentObstacle = Instantiate(PathSegmentData.SegmentObstaclePrefab, transform);
        Debug.Log($"Spawned pos {SegmentObstacle.transform.position}");
    }

}