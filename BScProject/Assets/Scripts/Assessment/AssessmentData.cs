using System;
using System.Collections.Generic;

[Serializable]
public class AssessmentData
{
    public int ID;
    public DateTime DateTime;
    public List<PathAssessmentData> Paths;

    public AssessmentData(int id, DateTime dateTime)
    {
        ID = id;
        DateTime = dateTime;
    }
}

[Serializable]
public class PathAssessmentData
{
    public int ID;
    public PathType Type;
    public int Length;
    public bool CorrentPathImageSelected;
    public List<SegmentAssessmentData> PathSegments;

}

[Serializable]
public class SegmentAssessmentData
{
    public int ID;
    public float SelectedObjectiveDistance;
    public float CalculatedObjectiveDistance;
    public bool CorrectObjectiveObjectSelected;

    public float SelectedObjectDistance;
    public float CorrectObjectDistance;
    public bool CorrectObjectSelected;
}




