using System;
using UnityEngine;

public class PathSegment : MonoBehaviour 
{
    public PathSegmentData PathSegmentData;
    public event Action SegmentCompleted;
    private Objective _objective;
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
        _objective = GetComponentInChildren<Objective>();
        if (_objective == null)
        {
            Debug.LogError($"Could not find objective for {this} path segment.");
            return;
        }
        _objective.ObjectiveCaptured += OnObjectiveCaptured;

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
        _objective.HideObjective();
    }

    public void ShowSegmentObjective()
    {
        _objective.ShowObjective();
    }

    public void PlaySegmentObjectiveHint()
    {
        _objective.ShowObjectiveHint();
    }
}