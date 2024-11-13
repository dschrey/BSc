using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathSegmentAssessment
{
    private PathSegmentData _pathSegment;
    public float SelectedDistanceToPreviousSegment;
    public int SelectedObjectiveObjectID;
    public float SelectedDistanceOfObjectToObjective;
    public float SelectedDistanceOfObjectToRealObject;
    public int SelectedSegmentObjectID;

    public PathSegmentAssessment(PathSegmentData pathSegment)
    {
        _pathSegment = pathSegment;
    }

    public PathSegmentData GetSegmentData()
    {
        return _pathSegment;
    }

    public bool AssessSegmentObject()
    {
        return SelectedSegmentObjectID == _pathSegment.SegmentObjectID;
    }

    public bool AssessObjectiveObject()
    {
        return SelectedObjectiveObjectID == _pathSegment.ObjectiveObjectID;
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