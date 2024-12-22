using System;
using UnityEngine;

[Obsolete("Class is deprecated and will be removed in the future.")]
public class SegmentObjectPositionData
{
    public PathSegmentData PathSegmentData;
    public float DistanceToObjective = 0f;
    public float DistanceToRealObject = -1f;
    public Vector3 CanvasObjectPosition = Vector3.zero;
    public Vector3 WorldObjectPosition = Vector3.zero;

    public SegmentObjectPositionData(PathSegmentData data)
    {
        PathSegmentData = data;
    }
}
