
using UnityEngine;

[System.Serializable]
public class PathSegmentData 
{
    public int SegmentID;
    public Vector3 relativeSegmentPosition;
    public float RelativeDistanceToPreviousSegment = 0;
}