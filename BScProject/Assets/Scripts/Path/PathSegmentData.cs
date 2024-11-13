
using UnityEngine;

[System.Serializable]
public class PathSegmentData 
{
    [Header("Segment Properties")]
    public int SegmentID;
    public Color SegmentColor;

    [Header("Segment Position")]
    [Tooltip("Position of the objective relative to the previous segment")]
    public Vector3 RelativeSegmentPosition;

    [Tooltip("Calculated distance to the previous segment")]
    public float DistanceToPreviousSegment = 0;

    [Header("Objective Object")]
    [Tooltip("Object that should hover on top of the objective")]
    public int ObjectiveObjectID;
    
    [Header("Segment Object")]
    [Tooltip("Object that should serve as a landmark for each segment")]
    public int SegmentObjectID;

    [Tooltip("Position of the segment object relative the the objective")]
    public Vector3 RelativeObjectPositionToObjective;

    [Tooltip("Calculated distance of the segment object to the objective")]
    public float ObjectDistanceToObjective = 0;

    [Header("Segment Obstacle")]
    public GameObject SegmentObstaclePrefab;

}