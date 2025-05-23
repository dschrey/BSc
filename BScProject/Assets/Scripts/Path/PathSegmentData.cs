using UnityEngine;

[System.Serializable]
public class PathSegmentData 
{
    [Header("Segment Properties")]
    public int SegmentID;
    public Color SegmentColor;
    [Tooltip("Angle to segment")]
    public float AngleFromPreviousSegment = 0f;
    [Tooltip("Distance to segment")]
    public float DistanceToPreviousSegment = 0f;

    [Header("Objective Object")]
    [Tooltip("Object that should hover on top of the objective")]
    public int ObjectiveObjectID;
    
    [Header("Landmark Object")]
    [Tooltip("Object that should serve as a landmark for each segment")]
    public int LandmarkObjectID;

    [Tooltip("Position of the landmark object relative the the objective")]
    public Vector3 RelativeLandmarkPositionToObjective;
    [Tooltip("Angle from the objective to the landmark")]
    public float AngleToLandmark = 0f;
    public float LandmarkDistanceToSegment = 0;
    [Header("Segment Obstacle")]
    [Tooltip("Position of the obstacle object relative the the objective")]
    public Vector3 RelativeObstaclePositionToObjective;
    [Tooltip("Rotation of the obstacle object")]
    public Quaternion ObstacleRotation;
    [Tooltip("Local scale of the obstacle object")]
    public Vector3 Scale = new(1, 1.25f, 1);
}