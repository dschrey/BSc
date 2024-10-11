using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineController : MonoBehaviour
{

    private LineRenderer _lineRenderer;
    private List<Transform> _points = new();


    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _points.Count;  i++)
        {
            _lineRenderer.SetPosition(i, _points[i].position);
        }
    }

    public void SetLinePoints(List<Transform> points)
    {
        _points = points;
    }

    public void ResetLinePoints()
    {
        _lineRenderer.positionCount = 0;
    }
}
