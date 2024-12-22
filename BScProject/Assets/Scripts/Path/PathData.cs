using System.Collections.Generic;
using UnityEngine;

public enum PathType {DEFAULT, EXTENDED}

[CreateAssetMenu(fileName = "PathData", menuName = "Path/PathData", order = 0)]
public class PathData : ScriptableObject
{
    public int PathID;
    public PathType Type = PathType.DEFAULT;
    public Color PathColor;
    public List<int> PathLayoutSelectionOrder = new();
    [HideInInspector] public int PathLayoutID = 0;
    [Header("Fake Paths")]
    public List<float> FakePathAngles1 = new();
    public List<float> FakePathAngles2 = new();
    public List<float> FakePathAngles3 = new();
    public List<PathSegmentData> SegmentsData = new();
    public PathSegmentData GetSegmentData(int id)
    {
        return SegmentsData.Find(s => s.SegmentID == id);
    }
}
