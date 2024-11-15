using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public PathData PathData { get; private set;  }
    [SerializeField] private GameObject _pathSegmentPrefab;
    public List<PathSegment> Segments = new();

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    void OnDrawGizmos()
    {
        if (Segments != null && Segments.Count > 1)
        {
            Gizmos.color = PathData.PathColor;
            Vector3 position = transform.position;
            position.y = 0;
            Gizmos.DrawLine(position, Segments[0].transform.position);
            for (int i = 0; i < Segments.Count - 1; i++)
            {
                if (Segments[i] != null && Segments[i + 1] != null)
                {

                    Vector3 fromPos = Segments[i].transform.position;
                    fromPos.y = 0;
                    Vector3 toPos = Segments[i + 1].transform.position;
                    toPos.y = 0;
                    Gizmos.DrawLine(fromPos, toPos);
                }
            }
        }
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------------

    public void Initialize(PathData pathData)
    {
        PathData = pathData;
        Vector3 lastSegmentPosition = transform.position;
        foreach (PathSegmentData pathSegmentData in pathData.SegmentsData)
        {
            Vector3 segmentSpawnpoint = lastSegmentPosition + pathSegmentData.RelativeSegmentPosition;
            float segmentDistance = Vector3.Distance(lastSegmentPosition, segmentSpawnpoint);

            PathSegment segment = Instantiate(_pathSegmentPrefab, segmentSpawnpoint, Quaternion.identity, transform).GetComponent<PathSegment>();
            segment.Initialize(pathSegmentData, segmentDistance);
            
            if (pathData.Type == PathType.EXTENDED)
            {
                segment.SpawnSegmentObjects();
            }

            Segments.Add(segment);
            lastSegmentPosition = segmentSpawnpoint;
            segment.gameObject.SetActive(false);
        }
    }

    public void TogglePathPreview(bool state)
    {
        if (PathData == null)
        {
            Debug.LogWarning($"Cannot preview a path with no data.");
            return;
        }

        foreach (PathSegment segment in Segments)
        {
            segment.gameObject.SetActive(state);
        }
    }

    public PathSegment GetPathSegment(int segmentID)
    {
        return Segments.Find(s => s.PathSegmentData.SegmentID == segmentID);
    }
}
