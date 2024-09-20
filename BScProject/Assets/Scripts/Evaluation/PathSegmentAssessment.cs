using System;
using UnityEngine;

[System.Serializable]
public class PathSegmentAssessment
{
    private PathSegmentData _pathSegment;
    public float SelectedDistanceToPreviousSegment;
    public Sprite SelectedObjectiveObjectSprite;

    public PathSegmentAssessment(PathSegmentData pathSegment)
    {
        _pathSegment = pathSegment;
    }

    public PathSegmentData GetCorrectPathSegment()
    {
        return _pathSegment;
    }

    public float GetDistanceToPreviousSegment(Vector3 previousSegmentPosition, Vector3 nextSegmentPosition)
    {
        return Vector3.Distance(previousSegmentPosition, nextSegmentPosition);
    }

    public bool EvaluateObjectAssignment()
    {
        if (SelectedObjectiveObjectSprite.name == _pathSegment.ObjectiveObjectSprite.name)
        {
            return true;
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