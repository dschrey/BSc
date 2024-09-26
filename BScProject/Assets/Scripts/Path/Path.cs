using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public PathData PathData { get; private set;  }
    [SerializeField] private Color _pathColor;
    [SerializeField] private GameObject _pathSegmentPrefab;
    public List<PathSegment> Segments = new();

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------


    void OnDrawGizmosSelected()
    {
        if (Segments != null && Segments.Count > 1)
        {
            Gizmos.color = _pathColor;
            Gizmos.DrawLine(transform.position, Segments[0].transform.position);
            for (int i = 0; i < Segments.Count - 1; i++)
            {
                if (Segments[i] != null && Segments[i + 1] != null)
                {
                    Gizmos.DrawLine(Segments[i].transform.position, Segments[i + 1].transform.position);
                }
            }
        }
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------------



    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------------

    public void SetupPathSegments(PathData pathData)
    {
        PathData = pathData;
        GameObject lastSegment = null;
        foreach (PathSegmentData pathSegment in pathData.Segments)
        {
            Vector3 segmentSpawnpoint;
            float segmentDistance;
            if (lastSegment == null)
            {
                // First segment
                segmentSpawnpoint = transform.position + pathSegment.RelativeSegmentPosition;
                segmentDistance = Vector3.Distance(transform.position, segmentSpawnpoint);
            }
            else
            {
                segmentSpawnpoint = lastSegment.transform.position + pathSegment.RelativeSegmentPosition;
                segmentDistance = Vector3.Distance(lastSegment.transform.position, segmentSpawnpoint);
            }

            PathSegment segment = Instantiate(_pathSegmentPrefab, segmentSpawnpoint, Quaternion.identity, transform).GetComponent<PathSegment>();
            segment.PathSegmentData = pathSegment;
            Segments.Add(segment);
            lastSegment = segment.gameObject;
            pathSegment.DistanceToPreviousSegment = segmentDistance;
            segment.PathSegmentData.DistanceToPreviousSegment = segmentDistance;
            segment.gameObject.SetActive(false);
        }
    }

}
