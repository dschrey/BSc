using System.Collections.Generic;
using UnityEngine;

public class PathAssessment 
{
    public PathData PathData { get; private set; }

    // TODO fix everything related to the selected path layout
    public int SelectedPathLayoutID;
    public List<PathSegmentAssessment> PathSegmentAssessments { get; private set; }

    public PathAssessment(PathData correctPath)
    {
        PathData = correctPath;
        PathSegmentAssessments = new();

    foreach (PathSegmentData segment in correctPath.SegmentsData)
        {
            PathSegmentAssessment pathSegmentAssessment = new(segment);
            PathSegmentAssessments.Add(pathSegmentAssessment);
        }
    }

    public bool EvaluateSelectedPathImage()
    {
        return PathData.PathLayoutID == SelectedPathLayoutID;
    }

}

