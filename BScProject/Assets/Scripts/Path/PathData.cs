using System.Collections.Generic;
using UnityEngine;

public enum PathType {DEFAULT, EXTENDED}

[CreateAssetMenu(fileName = "PathData", menuName = "Path/PathData", order = 0)]
public class PathData : ScriptableObject
{
    public int PathID;
    public PathType Type = PathType.DEFAULT;
    public Sprite pathTexture;
    public List<PathSegmentData> Segments = new();

}
