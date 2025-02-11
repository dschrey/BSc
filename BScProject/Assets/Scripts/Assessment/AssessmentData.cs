using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssessmentData
{
    public int AssessmentID;
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

    public void AddPath(PathData pathData, FloorType floor)
    {
        PathAssessmentData pathAssessmentData = new(pathData, floor);
        Paths.Add(pathAssessmentData);
        Debug.Log($"Added path (ID: {pathData.PathID}) to assessment {AssessmentID}\n\tPath count: {Paths.Count}");
    }

    public PathAssessmentData GetPath(int pathID)
    {
        return Paths.Find(p => p.PathID == pathID);
    }

    public PathAssessmentData GetPath(string pathName)
    {
        return Paths.Find(p => p.Name == pathName);
    }

    public void SetPathTime(string pathName, string timeTaken)
    {
        GetPath(pathName).Time = timeTaken;
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
    public int SelectedPathLayout;
    public int ActualPathLayout;
    public bool CorrentPathLayoutSelected;
    public List<SegmentAssessmentData> PathSegments;

    public PathAssessmentData(PathData pathData, FloorType floor)
    {
        PathSegments = new();
        Name = pathData.PathName;
        PathID = pathData.PathID;
        FloorType = floor;
        Length = pathData.SegmentsData.Count;
        ActualPathLayout = pathData.PathLayoutID;

        foreach (PathSegmentData pathSegment in pathData.SegmentsData)
        {
            PathSegments.Add(new(pathSegment.SegmentID, pathSegment.DistanceFromPreviousSegment,
                pathSegment.LandmarkObjectDistanceToObjective, pathSegment.ObjectiveObjectID, pathSegment.LandmarkObjectID));
        }
    }

    public void SetPathLayout(int layoutID)
    {
        SelectedPathLayout = layoutID;
        CorrentPathLayoutSelected = SelectedPathLayout == ActualPathLayout;
        Debug.Log($"Path ({PathID} - {Name}): Correct layout -> {CorrentPathLayoutSelected}");
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

    public int SelectedHoverObjectID;
    public int ActualHoverObjectID;
    public bool CorrectHoverObjectSelected;

    public float SelectedLandmarkDistanceToObjective;
    public float ActualLandmarkDistanceToObjective;
    public float LandmarkDifferenceToRealObject;
    public float LandmarkDistanceError;

    public int SelectedLandmarkObjectID;
    public int ActualLandmarkObjectID;
    public bool CorrectLandmarkObjectSelected;

    public SegmentAssessmentData(int segmentID, float distanceFromPreviousSegment, float landmarkObjectDistanceToObjective,
        int hoverObjectID, int landmarkObjectID)
    {
        SegmentID = segmentID;
        ActualDistanceToPreviousSegment = distanceFromPreviousSegment;
        ActualLandmarkDistanceToObjective = landmarkObjectDistanceToObjective;
        ActualHoverObjectID = hoverObjectID;
        ActualLandmarkObjectID = landmarkObjectID;
    }

    public void SetObjectiveDistance(float distance)
    {
        SelectedDistanceToPreviousSegment = distance;
        SegmentDistanceError = Math.Abs(ActualDistanceToPreviousSegment - SelectedDistanceToPreviousSegment);
        Debug.LogWarning($"Objective: Set distance: {distance}: Actual distance: {ActualDistanceToPreviousSegment}\nError: {LandmarkDistanceError}");

    }

    public void SetLandmarkDistances(float distanceToObjective, float differenceToRealObject)
    {
        SelectedLandmarkDistanceToObjective = distanceToObjective;
        LandmarkDifferenceToRealObject = differenceToRealObject;
        LandmarkDistanceError = Math.Abs(ActualLandmarkDistanceToObjective - SelectedLandmarkDistanceToObjective);
        Debug.LogWarning($"Landmark: Set distance: {distanceToObjective}: Actual distance: {ActualLandmarkDistanceToObjective}\nError: {LandmarkDistanceError}");
    }

    public void SetHoverObject(int objectID)
    {
        SelectedHoverObjectID = objectID;
        CorrectHoverObjectSelected = SelectedHoverObjectID == ActualHoverObjectID;
        Debug.Log($"Segment ({SegmentID}): Hover correct -> {CorrectHoverObjectSelected}");
    }

    public void SetLandmarkObject(int objectID)
    {
        SelectedLandmarkObjectID = objectID;
        CorrectLandmarkObjectSelected = SelectedLandmarkObjectID == ActualLandmarkObjectID;
        Debug.Log($"Segment ({SegmentID}): Landmark correct -> {CorrectLandmarkObjectSelected}");
    }

}




