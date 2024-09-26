using System;
using UnityEngine;

public class PathSegment : MonoBehaviour 
{
    public PathSegmentData PathSegmentData;
    public event Action SegmentCompleted;
    public Objective Objective { get; private set; }
    public GameObject SegmentObject { get; private set; }
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

        SpawnSegmentObject();

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

    private void SpawnSegmentObject()
    {
        Path path = transform.parent.GetComponent<Path>();
        if (path.PathData.Type != PathType.EXTENDED)
        {
            return;
        }


        Vector3 objectSpawnpoint = transform.position + PathSegmentData.RelativeObjectPositionToObjective;
        
        SegmentObject = Instantiate(PathSegmentData.ObjectPrefab, objectSpawnpoint, PathSegmentData.ObjectRotation, transform);
        PathSegmentData.ObjectDistanceToObjective = Vector3.Distance(transform.position, objectSpawnpoint);
    } 
}