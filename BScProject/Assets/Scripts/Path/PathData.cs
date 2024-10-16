using System.Collections.Generic;
using UnityEngine;

public enum PathType {DEFAULT, EXTENDED}

[CreateAssetMenu(fileName = "PathData", menuName = "Path/PathData", order = 0)]
public class PathData : ScriptableObject
{
    public int PathID;
    public PathType Type = PathType.DEFAULT;
    public Color PathColor;
    public Sprite PathImage;
    [Header("Path image selection options (at least 4)")]
    public List<Sprite> PathImageSelection;
    public List<PathSegmentData> SegmentsData = new();

    public PathSegmentData GetSegmentData(int id)
    {
        return SegmentsData.Find(s => s.SegmentID == id);
    }
}
