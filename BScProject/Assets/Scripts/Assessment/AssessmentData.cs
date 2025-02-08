using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssessmentData
{
    public int AssessmentID;
    // public string ParticipantName;
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

    public void AddPath(PathData pathData, FloorType floor, string timeTaken)
    {
        Debug.Log($"Added path (ID: {pathData.PathID}) to assessment {AssessmentID}");
        PathAssessmentData pathAssessmentData = new(pathData, floor, timeTaken);
        Paths.Add(pathAssessmentData);
    }

    public PathAssessmentData GetPath(int pathID)
    {
        return Paths.Find(p => p.PathID == pathID);
    }
}

[Serializable]
public class PathAssessmentData
{
    public int PathID;
    public string Name;
    public FloorType FloorType;
    public int Length;
    public string Time;
    public bool CorrentPathLayoutSelected;
    public List<SegmentAssessmentData> PathSegments;

    public PathAssessmentData(PathData pathData, FloorType floor, string timeTaken)
    {
        PathSegments = new();
        Name = pathData.name;
        PathID = pathData.PathID;
        FloorType = floor;
        Length = pathData.SegmentsData.Count;
        Time = timeTaken;

        foreach (PathSegmentData pathSegment in pathData.SegmentsData)
        {
            PathSegments.Add(new(pathSegment.SegmentID));
        }
    }

    public SegmentAssessmentData GetSegment(int segmentID)
    {
        return PathSegments.Find(s => s.SegmentID == segmentID);
    }

}

[Serializable]
public class SegmentAssessmentData
{
    public int SegmentID;
    public float SelectedDistanceToPreviousSegment;
    public float ActualDistanceToPreviousSegment;
    public float SegmentDistanceError;
    public bool CorrectHoverObjectSelected;
    public float SelectedLandmarkDistanceToObjective;
    public float ActualLandmarkDistanceToObjective;
    public float LandmarkDifferenceToRealObject;
    public bool CorrectLandmarkObjectSelected;

    public SegmentAssessmentData (int id)
    {
        SegmentID = id;
    }


}




