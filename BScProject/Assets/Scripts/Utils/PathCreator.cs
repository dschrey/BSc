using System;
using System.Collections.Generic;
using UnityEngine;




public class PathCreator : MonoBehaviour
{
    [SerializeField] private Transform _pathSpawn;
    [SerializeField] private GameObject _pathPrefabObject;
    public PathData pathData;
    private Path _createdPath = null;
    [Header("Segments"), Range(0, 10), SerializeField]
    private int _numSegments = 0;

    // public List<SegmentAttributes> SegmentsData = new();


    private void Start()
    {
        // pathData.SegmentsData.ForEach(data => SegmentsData.Add(data));
        // _createdPath = Instantiate(_pathPrefabObject, _pathSpawn.transform).GetComponent<Path>();
        // _createdPath.Initialize(_createdPath.PathData);
    }


    private void OnValidate()
    {
      
    }


    private void Update()
    {
        // foreach (var segment in SegmentsData)
        // {
        //     segment.SegmentID = CalculateSegmentID();
        //     UpdatePosition();
        // }
        // pathData.SegmentsData = SegmentsData;
    }


    private void UpdatePosition()
    {
        Vector3 lastSegmentPosition = Vector3.zero;
        // foreach (var segment in SegmentsData)
        // {
        //     Vector3 segmentSpawnpoint = lastSegmentPosition + pathSegmentData.RelativeSegmentPosition;
        //     float segmentDistance = Vector3.Distance(lastSegmentPosition, segmentSpawnpoint);

        //     PathSegment segment = Instantiate(_pathSegmentPrefab, segmentSpawnpoint, Quaternion.identity, transform).GetComponent<PathSegment>();
        //     segment.Initialize(pathSegmentData, segmentDistance);

        //     if (pathData.Type == PathType.EXTENDED)
        //     {
        //         segment.SpawnSegmentObjects();
        //     }

        //     Segments.Add(segment);
        //     lastSegmentPosition = segmentSpawnpoint;
        //     segment.gameObject.SetActive(false);
        // }
    }

}
