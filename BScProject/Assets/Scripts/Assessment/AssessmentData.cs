using System;
using System.Collections.Generic;

[Serializable]
public class AssessmentData
{
    public int AssessmentID;
    public DateTime DateTime;
    public List<PathAssessmentData> Paths;
    public bool Completed;

    public AssessmentData(int id, DateTime dateTime)
    {
        Paths = new();
        AssessmentID = id;
        DateTime = dateTime;
        Completed = false;
    }

    public void AddPath(PathData pathData)
    {
        PathAssessmentData pathAssessmentData = new(pathData);
        Paths.Add(pathAssessmentData);
    }

    public PathAssessmentData GetPath(int pathID)
    {
        return Paths.Find(p => p.PathAssessmentID == pathID);
    }
}

[Serializable]
public class PathAssessmentData
{
    public int PathAssessmentID;
    public PathType Type;
    public int Length;
    public bool CorrentPathImageSelected;
    public List<SegmentAssessmentData> PathSegments;

    public PathAssessmentData(PathData pathData)
    {
        PathSegments = new();
        PathAssessmentID = pathData.PathID;
        Type = pathData.Type;
        Length = pathData.SegmentsData.Count;
        foreach (PathSegmentData pathSegment in pathData.SegmentsData)
        {
            SegmentAssessmentData segmentAssessmentData = new(pathSegment.SegmentID);
            PathSegments.Add(segmentAssessmentData);
        }
    }

    public SegmentAssessmentData GetSegment(int segmentAssessmentID)
    {
        return PathSegments.Find(s => s.SegmentAssessmentID == segmentAssessmentID);
    }

}

[Serializable]
public class SegmentAssessmentData
{
    public int SegmentAssessmentID;
    public float SelectedDistanceToPreviousSegment;
    public float CalculatedDistanceToPreviousSegment;
    public bool CorrectObjectiveObjectSelected;

    public float SelectedObjectDistanceToObjective;
    public float CalculatedObjectDistanceToObjective;
    public bool CorrectSegmentObjectSelected;

    public SegmentAssessmentData (int id)
    {
        SegmentAssessmentID = id;
    }
}




