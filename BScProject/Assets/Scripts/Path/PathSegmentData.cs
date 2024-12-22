using UnityEngine;

[System.Serializable]
public class PathSegmentData 
{
    [Header("Segment Properties")]
    public int SegmentID;
    public Color SegmentColor;
    
    [Tooltip("Distance to segment")]
    public float DistanceFromPreviousSegment = 0f;

    [Tooltip("Angle to segment")]
    public float AngleFromPreviousSegment = 0f;

    [Header("Objective Object")]
    [Tooltip("Object that should hover on top of the objective")]
    public int ObjectiveObjectID;
    
    [Header("Landmark Object")]
    [Tooltip("Object that should serve as a landmark for each segment")]
    public int LandmarkObjectID;

    [Tooltip("Position of the landmark object relative the the objective")]
    public Vector3 RelativeObjectPositionToObjective;

    [HideInInspector]
    public float LandmarkObjectDistanceToObjective = 0;

    [Header("Segment Obstacle")]
    public GameObject SegmentObstaclePrefab;

}