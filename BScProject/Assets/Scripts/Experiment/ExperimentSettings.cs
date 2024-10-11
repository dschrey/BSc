using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ExperimentSettings", menuName = "ExperimentSettings")]
public class ExperimentSettings : ScriptableObject
{
    [Header("Experiment Settings")]
    [Tooltip("Size of the movement area")]
    public Vector2 MovementArea = new(10f, 6f);

    [Tooltip("Radius of the player detection colliders")]
    public float PlayerDetectionRadius = 0.35f;
    [Tooltip("Time (in seconds) it takes to reaveal an objective")]
    public float ObjectiveRevealTime = 1.5f;

    [Tooltip("Paths to be played per iteration.")]
    public List<PathData> paths;

    [Header("Evaulation Settings")]
    [Tooltip("Standard: PathSelection, ItemDistance\nExtended: PathSelection, ItemPosition, ItemDistance")]
    [Range(0, 1)] public float MovementSpeedStepSize = 0.25f;
    public float MovementSpeedMultiplier = 1;
    public float MinMovementSpeedMultiplier = 0.25f;
    public float MaxMovementSpeedMultiplier = 3;
    public int CompletedAssessments = 0;

}

public class ExperimentSettingsData 
{
    public int CompletedAssessments;
    public float PlayerDetectionRadius;
    public float ObjectiveRevealTime;
    public float MovementSpeedMultiplier;
}