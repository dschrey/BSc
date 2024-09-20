
using UnityEngine;

[System.Serializable]
public class PathSegmentData 
{
    public int SegmentID;
    public Color SegmentColor;
    public Sprite ObjectiveObjectSprite;
    public Sprite SegmentObjectSprite;
    public Vector3 relativeSegmentPosition;
    public float DistanceToPreviousSegment = 0;
}