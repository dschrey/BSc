
using System.Collections.Generic;

[System.Serializable]
public class EvaluationPathData 
{
    private PathData _correctPath;
    private PathData _selectedPath;
    public List<EvaluationPathSegmentData> CorrectPathEvaluationSegments = new();  
    public List<EvaluationPathSegmentData> SelectedPathEvaluationSegments = new();


    public EvaluationPathData(PathData correctPath, PathData selectedPath)
    {
        _correctPath = correctPath;
        _selectedPath = selectedPath;

        foreach (PathSegmentData segment in correctPath.Segments)
        {
            CorrectPathEvaluationSegments.Add(new EvaluationPathSegmentData(segment));
        }
        foreach (PathSegmentData segment in _selectedPath.Segments)
        {
            SelectedPathEvaluationSegments.Add(new EvaluationPathSegmentData(segment));
        }
    }

    public PathData GetCorrectPath()
    {
        return _correctPath;
    }
    public PathData GetSelectedPath()
    {
        return _selectedPath;
    }

}