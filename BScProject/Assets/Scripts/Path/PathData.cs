using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PathData", menuName = "Path/PathData", order = 0)]
public class PathData : ScriptableObject
{
    public int PathID;
    public Sprite pathTexture;
    public bool ListSize => Test();
    public List<PathSegmentData> Segments = new();

    public bool Test()
    {
        foreach (var segment in Segments)
        {
            segment.RelativeDistanceToPreviousSegment = 4;
        }
        return true;
    }

}
