using System.Collections.Generic;
using UnityEngine;

public class PathAssessment 
{
    public PathData PathData { get; }
    public Sprite SelectedPathSprite { get; set; }
    public List<PathSegmentAssessment> PathSegmentAssessments { get; private set; }

    public PathAssessment(PathData correctPath)
    {
        PathData = correctPath;
        PathSegmentAssessments = new();

    foreach (PathSegmentData segment in correctPath.Segments)
        {
            PathSegmentAssessment pathSegmentAssessment = new(segment);
            PathSegmentAssessments.Add(pathSegmentAssessment);
        }
    }

    public bool EvaluateSelectedPathImage()
    {
        return PathData.PathImage == SelectedPathSprite;
    }

}

