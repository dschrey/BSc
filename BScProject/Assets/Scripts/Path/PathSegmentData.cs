
using UnityEngine;

[System.Serializable]
public class PathSegmentData 
{
    [Header("Segment Properties")]
    public int SegmentID;
    public Color SegmentColor;
    public Sprite ObjectiveObjectSprite;
    public Sprite SegmentObjectSprite;

    [Header("Segment Position")]
    [Tooltip("Position of the objective relative to the previous segment")]
    public Vector3 RelativeSegmentPosition;
    [Tooltip("Calculated distance to the previous segment")]
    public float DistanceToPreviousSegment = 0;

    [Header("Segment Object")]
    public GameObject ObjectPrefab;

    [Tooltip("Position of the segment object relative the the objective")]
    public Vector3 RelativeObjectPositionToObjective;
    [Tooltip("Calculated distance of the segment object to the objective")]

    public float ObjectDistanceToObjective = 0;

    [Tooltip("Rotation of the segment object")]
    public Quaternion ObjectRotation;
}