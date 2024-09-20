
using System.Collections.Generic;

public class PathAssessmentData 
{
    public PathData CorrectPath { get; }
    public PathData SelectedPath { get; set; }
    public List<PathSegmentAssessment> PathSegmentAssessments { get; }

    public PathAssessmentData(PathData correctPath)
    {
        CorrectPath = correctPath;

        foreach (PathSegmentData segment in correctPath.Segments)
        {
            PathSegmentAssessments.Add(new PathSegmentAssessment(segment));
        }
    }

}