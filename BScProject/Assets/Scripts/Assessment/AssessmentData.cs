using System;
using System.Collections.Generic;

[Serializable]
public class AssessmentData
{
    public int AssessmentID;
    public string ParticipantName;
    public string DateTime;
    public List<PathAssessmentData> Paths;
    public bool Completed;

    public AssessmentData(int id, DateTime dateTime)
    {
        Paths = new();
        AssessmentID = id;
        DateTime = dateTime.ToString().Replace(" ", "_");
        Completed = false;
    }

    public AssessmentData(int id, string participant, DateTime dateTime)
    {
        AssessmentID = id;
        ParticipantName = participant;
        DateTime = dateTime.ToString().Replace(" ", "_");
        Paths = new();
        Completed = false;
    }

    public void AddPath(PathData pathData, int timeTaken)
    {
        PathAssessmentData pathAssessmentData = new(pathData, timeTaken);
        Paths.Add(pathAssessmentData);
    }

    public void AddPath(PathData pathData)
    {
        // TODO Handle time
        PathAssessmentData pathAssessmentData = new(pathData, 0);
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
    public int Time;
    public bool CorrentPathImageSelected;
    public List<SegmentAssessmentData> PathSegments;

    public PathAssessmentData(PathData pathData, int timeTaken)
    {
        PathSegments = new();
        PathAssessmentID = pathData.PathID;
        Type = pathData.Type;
        Length = pathData.SegmentsData.Count;
        Time = timeTaken;
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
    public float SegmentDistanceDifference;

    // Extented Path Assessment Data
    public bool CorrectObjectiveObjectSelected;
    public float SelectedObjectDistanceToObjective;
    public float CalculatedObjectDistanceToObjective;
    public float ObjectDifferenceToRealObject;
    public bool CorrectSegmentObjectSelected;

    public SegmentAssessmentData (int id)
    {
        SegmentAssessmentID = id;
    }


}




