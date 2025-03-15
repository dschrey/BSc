using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ExperimentSettings", menuName = "ExperimentSettings")]
public class Settings : ScriptableObject
{
    [Header("Experiment Settings")]
    [Tooltip("Size of the movement area")]
    public Vector2 MovementArea = new(10f, 6f);

    [Tooltip("Radius of the player detection colliders")]
    public float PlayerDetectionRadius = 0.35f;
    public float DefaultPlayerDetectionRadius = 0.35f;

    [Tooltip("Time (in seconds) it takes to reaveal an objective")]
    public float ObjectiveRevealTime = 1.5f;
    public float DefaultObjectiveRevealTime = 1.5f;
    [Tooltip("Time (in seconds) it takes to reaveal an objective")]
    public float PathGuidanceCooldown = 5f;
    public float DefaultPathGuidanceCooldown = 5f;

    [Tooltip("Distance of each segment to the previous (in meters) in the path preview.")]
    public float SegmentLength = 2.0f;
    public float DefaultSegmentLength = 2.0f;

    [Header("Assessment Settings")]
    [Tooltip("Standard: PathSelection, ItemDistance\nExtended: PathSelection, ItemPosition, ItemDistance")]
    [Range(0, 1)] public float MovementSpeedStepSize = 0.25f;
    public float MovementSpeedMultiplier = 1;
    public float MinMovementSpeedMultiplier = 0.25f;
    public float MaxMovementSpeedMultiplier = 3;
    public int CompletedExperiments = 0;
    public float TransitionDuration = 0.75f;
    public float DefaultTransitionDuration = 0.75f;

}

public class ExperimentSettingsData 
{
    public int CompletedAssessments;
    public float PlayerDetectionRadius;
    public float ObjectiveRevealTime;
    public float MovementSpeedMultiplier;
    public float TransitionDuration;
    public float SegmentLength;
}