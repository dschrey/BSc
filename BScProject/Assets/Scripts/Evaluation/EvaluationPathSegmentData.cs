using UnityEngine;

[System.Serializable]
public class EvaluationPathSegmentData
{
    private PathSegmentData _pathSegment;
    public float defaultModeDistance;
    public float extendedModeDistance;

    public EvaluationPathSegmentData(PathSegmentData pathSegment)
    {
        _pathSegment = pathSegment;
    }

    public PathSegmentData GetCorrectPathSegment()
    {
        return _pathSegment;
    }

    public float GetDistanceToPreviousSegment(Vector3 previousSegmentPosition, Vector3 nextSegmentPosition)
    {
        return Vector3.Distance(previousSegmentPosition, nextSegmentPosition);
    }

}