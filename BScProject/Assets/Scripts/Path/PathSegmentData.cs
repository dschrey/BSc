
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
    [Tooltip("Automatically created")]
    public RenderTexture ObjectiveObjectRenderTexture;

    [Header("Segment Objective Object")]
    [Tooltip("Object that should spawn on top of the objective")]
    public GameObject ObjectiveObjectPrefab;
    
    [Header("Segment Object")]
    [Tooltip("Object that should spawn with the segment")]
    public GameObject ObjectPrefab;
    [Tooltip("Automatically created")]
    public RenderTexture SegmentObjectRenderTexture;

    [Tooltip("Position of the segment object relative the the objective")]
    public Vector3 RelativeObjectPositionToObjective;

    [Tooltip("Calculated distance of the segment object to the objective")]
    public float ObjectDistanceToObjective = 0;

    [Tooltip("Rotation of the segment object")]
    public Quaternion ObjectRotation;

    [Header("Segment Obstacle")]
    public GameObject SegmentObstaclePrefab;

}