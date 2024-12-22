using System;

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
        return SelectedSegmentObjectID == _pathSegment.LandmarkObjectID;
    }

    public bool AssessObjectiveObject()
    {
        return SelectedObjectiveObjectID == _pathSegment.ObjectiveObjectID;
    }

    public int GetSegmentID()
    {
        return _pathSegment.SegmentID;
    }

}