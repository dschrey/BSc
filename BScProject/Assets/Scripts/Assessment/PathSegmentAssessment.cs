using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathSegmentAssessment
{
    private PathSegmentData _pathSegment;
    public float SelectedDistanceToPreviousSegment;
    public RenderTexture SelectedObjectiveObjectRenderTexture;
    public float SelectedDistanceOfObjectToObjective;
    public RenderTexture SelectedSegmentObjectRenderTexture;

    public PathSegmentAssessment(PathSegmentData pathSegment)
    {
        _pathSegment = pathSegment;
    }

    public PathSegmentData GetSegmentData()
    {
        return _pathSegment;
    }

    public float GetDistanceToPreviousSegment(Vector3 previousSegmentPosition, Vector3 nextSegmentPosition)
    {
        return Vector3.Distance(previousSegmentPosition, nextSegmentPosition);
    }

    public bool EvaluateSegmentObjectAssignment()
    {
        foreach (KeyValuePair<PathSegmentData, ObjectRenderer> entry in ObjectRenderManager.Instance.ActiveRenderTextures)
        {
            if (entry.Key.SegmentID != _pathSegment.SegmentID)
                continue;

            if (entry.Value.GetRenderTexture() == _pathSegment.SegmentObjectRenderTexture)
            {
                return true;
            }
        }
        return false;
    }
    
    public bool EvaluateSegmnetDistanceAssignment()
    {
        if (SelectedDistanceToPreviousSegment == _pathSegment.DistanceToPreviousSegment)
        {
            return true;
        }
        return false;
    }

    public float GetSegmnetDistanceDifference()
    {
        return Math.Abs(_pathSegment.DistanceToPreviousSegment - SelectedDistanceToPreviousSegment);
    }

    public int GetSegmentID()
    {
        return _pathSegment.SegmentID;
    }

}