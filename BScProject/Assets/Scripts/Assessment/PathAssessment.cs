using System;
using System.Collections.Generic;

// TODO Remove class
[Obsolete("Class is deprecated and will be removed in the future", true)]
public class PathAssessment
{
    public PathData PathData { get; private set; }
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
        return PathData.CorrectPathLayoutID == SelectedPathLayoutID;
    }

}

