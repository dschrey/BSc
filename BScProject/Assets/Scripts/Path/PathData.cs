using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Flags]
public enum PathObjects
{
    None = 0,
    Hover = 1 << 0,
    Landmarks = 1 << 1,
    Obstacles = 1 << 2
}

public enum PathDifficulty
{
    Easy,    // Current segment highlights stay visible
    Hard    // Current segment highlights stay disappears on move
}


[CreateAssetMenu(fileName = "Path", menuName = "Path/PathData", order = 0)]
public class PathData : ScriptableObject
{
    public int PathID = -1;
    public string PathName;
    public PathObjects PathObjects;
    public PathDifficulty PathDifficulty;
    [HideInInspector] public Color PathColor;
    public List<int> PathLayoutDisplayOrder = new();
    [HideInInspector] public int CorrectPathLayoutID = 0;
    [HideInInspector] public GameObject ObstaclePrefab = null;
    [Header("Fake Paths")]
    public List<float> FakePathAngles1 = new();
    public List<float> FakePathAngles2 = new();
    public List<float> FakePathAngles3 = new();
    [Header("Path Segments")]
    public List<PathSegmentData> SegmentsData = new();
    public event Action Update;
    public PathSegmentData GetSegmentData(int id)
    {
        return SegmentsData.Find(s => s.SegmentID == id);
    }

    private readonly List<Color> _colors = new()
    {
        Color.green,
        Color.blue,
        Color.red,
        Color.yellow,
        Color.magenta,
        Color.cyan
    };

    private void OnValidate()
    {
        int count = 0;
        int color = 0;
        foreach (var segment in SegmentsData)
        {
            segment.SegmentID = count;
            if (color == _colors.Count) color = 0;
            segment.SegmentColor = _colors[color];
            count++;
            color++;
        }

        // #if UNITY_EDITOR
        // if (!Application.isPlaying) return;
        //     Update.Invoke();
        // #endif
    }

    private void OnEnable()
    {
        // Initialze path layout display order randomly on creation
        if (PathLayoutDisplayOrder.Count == 0)
        {
            PathLayoutDisplayOrder = Enumerable.Range(0, 4).ToList();
            var random = new System.Random();
            PathLayoutDisplayOrder = PathLayoutDisplayOrder.OrderBy(x => random.Next()).ToList();
        }

        if (PathDifficulty == PathDifficulty.Hard && ((PathObjects & PathObjects.Obstacles) != 0) && ObstaclePrefab == null)
        {
            ObstaclePrefab = Resources.Load<GameObject>("Obstacle");
        }
    }
}
