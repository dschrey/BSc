using System;
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
            position.y = 1;
            Gizmos.DrawLine(position, Segments[0].transform.position);
            for (int i = 0; i < Segments.Count - 1; i++)
            {
                if (Segments[i] != null && Segments[i + 1] != null)
                {

                    Vector3 fromPos = Segments[i].transform.position;
                    fromPos.y = 1;
                    Vector3 toPos = Segments[i + 1].transform.position;
                    toPos.y = 1;
                    Gizmos.DrawLine(fromPos, toPos);
                }
            }
        }
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------------

    public void SetupPath(PathData pathData)
    {
        PathData = pathData;
        GameObject lastSegment = null;
        foreach (PathSegmentData pathSegmentData in pathData.SegmentsData)
        {
            Vector3 segmentSpawnpoint;
            float segmentDistance;
            if (lastSegment == null)
            {
                // First segment
                segmentSpawnpoint = transform.position + pathSegmentData.RelativeSegmentPosition;
                segmentDistance = Vector3.Distance(transform.position, segmentSpawnpoint);
            }
            else
            {
                segmentSpawnpoint = lastSegment.transform.position + pathSegmentData.RelativeSegmentPosition;
                segmentDistance = Vector3.Distance(lastSegment.transform.position, segmentSpawnpoint);
            }

            PathSegment segment = Instantiate(_pathSegmentPrefab, segmentSpawnpoint, Quaternion.identity, transform).GetComponent<PathSegment>();
            segment.PathSegmentData = pathSegmentData;
            pathSegmentData.DistanceToPreviousSegment = segmentDistance;
            segment.PathSegmentData.DistanceToPreviousSegment = segmentDistance;
            segment.SpawnSegmentObjects(pathData.Type);
            Segments.Add(segment);
            lastSegment = segment.gameObject;
            segment.gameObject.SetActive(false);
        }
    }

    internal static object Combine(string persistentDataPath, string v)
    {
        throw new NotImplementedException();
    }
}
