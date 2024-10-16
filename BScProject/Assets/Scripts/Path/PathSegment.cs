using System;
using UnityEngine;

public class PathSegment : MonoBehaviour 
{
    public PathSegmentData PathSegmentData;
    public event Action SegmentCompleted;
    public Objective Objective;
    public GameObject SegmentObject;
    private MovementDetection _movementDetection;
    
    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        SegmentObject = null;
        
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
        if (PathSegmentData.SegmentObjectRenderTexture != null)
        {
            PathSegmentData.SegmentObjectRenderTexture.Release();
        }
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnObjectiveCaptured()
    {
        SegmentCompleted?.Invoke();
    }
    
    private void OnExitedDectectionZone()
    {
        _movementDetection.enabled = false;
        SetObjectiveInvisible();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------------

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

    public void SpawnSegmentObjects(PathType pathType)
    {
        if (pathType != PathType.EXTENDED)
        {
            return;
        }

        SpawnObjectiveObject();
        SpawnSegmentObject();
        SpawnSegmentObstacle();
    }

    private  void SpawnObjectiveObject()
    {
        Objective.SpawnObject(PathSegmentData.ObjectiveObjectPrefab);

        ObjectRenderManager objectRenderManager = FindObjectOfType<ObjectRenderManager>();
        PathSegmentData.ObjectiveObjectRenderTexture = objectRenderManager.CreateNewRenderTexture(PathSegmentData, true);
    }

    private  void SpawnSegmentObject()
    {
        Vector3 objectSpawnpoint = transform.position + PathSegmentData.RelativeObjectPositionToObjective;
        
        SegmentObject = Instantiate(PathSegmentData.ObjectPrefab, objectSpawnpoint, PathSegmentData.ObjectRotation, transform);
        PathSegmentData.ObjectDistanceToObjective = Vector3.Distance(transform.position, objectSpawnpoint);

        ObjectRenderManager objectRenderManager = FindObjectOfType<ObjectRenderManager>();
        PathSegmentData.SegmentObjectRenderTexture = objectRenderManager.CreateNewRenderTexture(PathSegmentData);
    } 

    private  void SpawnSegmentObstacle()
    {
        Instantiate(PathSegmentData.SegmentObstaclePrefab, PathSegmentData.SegmentObstaclePrefab.transform.position, 
            PathSegmentData.SegmentObstaclePrefab.transform.rotation, transform);
    }


}