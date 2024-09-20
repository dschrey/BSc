using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ExperimentSettings", menuName = "ExperimentSettings")]
public class ExperimentSettings : ScriptableObject
{
    [Header("Experiment Settings")]
    [Tooltip("Iteration of the experiment")]
    public int Iterations = 3;
    [Tooltip("Paths to be played per iteration.")]
    public float PathsPerIteration = 5;
    public List<PathData> paths;

    [Header("Evaulation Settings")]
    [Tooltip("Standard: PathSelection, ItemDistance\nExtended: PathSelection, ItemPosition, ItemDistance")]
    public float MaxEvaluationSteps = 3;
    [Range(0, 1)] public float MovementSpeedStepSize = 0.25f;
    public float MovementSpeedMultiplier = 1;
    public float MinMovementSpeedMultiplier = 0.25f;
    public float MaxMovementSpeedMultiplier = 3;

}