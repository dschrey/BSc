using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Flags]
public enum PathObjects
{
    None = 0,
    Hovering = 1 << 0,
    Landmarks = 1 << 1,
    Obstacles = 1 << 2
}

[CreateAssetMenu(fileName = "Path", menuName = "PathData", order = 0)]
public class PathData : ScriptableObject
{
    public int PathID = -1;
    public string PathName;
    public PathObjects PathObjects;
    [HideInInspector] public Color PathColor;
    public List<int> PathLayoutSelectionOrder = new();
    [HideInInspector] public int CorrectPathLayoutID = 0;

    public GameObject ObstaclePrefab = null;
    [Header("Fake Paths")]
    public List<float> FakePathAngles1 = new();
    public List<float> FakePathAngles2 = new();
    public List<float> FakePathAngles3 = new();
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
        if (PathLayoutSelectionOrder.Count == 0)
        {
            PathLayoutSelectionOrder = Enumerable.Range(0, 4).ToList();
            var random = new System.Random();
            PathLayoutSelectionOrder = PathLayoutSelectionOrder.OrderBy(x => random.Next()).ToList();
        }

        if (ObstaclePrefab == null)
        {
            ObstaclePrefab = Resources.Load<GameObject>("Obstacle");
        }
    }
}
