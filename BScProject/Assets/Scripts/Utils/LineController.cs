using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineController : MonoBehaviour
{

    [SerializeField] private LineRenderer _lineRenderer;
    private List<Transform> _points = new();


    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _points.Count; i++)
        {
            _lineRenderer.SetPosition(i, _points[i].position);
        }
    }

    public void SetLinePoints(List<Transform> points)
    {
        _points.Clear();
        _points = points;
    }

    public void ResetPointList()
    {
        _points.Clear();
    }

    public LineRenderer GetLineRenderer()
    {
        return _lineRenderer;
    }

    public void SetColor(Color color)
    {
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
    }
}
