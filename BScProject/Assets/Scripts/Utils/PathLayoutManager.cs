using System.Collections.Generic;
using UnityEngine;


public class PathLayoutManager : MonoBehaviour
{

    public static PathLayoutManager Instance { get; private set; }

    [Header("Correct Path Layout")]

    public List<PathLayoutCreator> PathLayouts = new();


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------


    public void PreparePathPreviews(PathData pathData)
    {
        if (PathLayouts.Count < 4)
        {
            Debug.LogError($"Path layout creators are missing!");
            return;
        }
        PathLayouts[0].CreatePathLayout(pathData.SegmentsData);
        PathLayouts[1].CreatePathLayout(pathData.SegmentsData, pathData.FakePathAngles1);
        PathLayouts[2].CreatePathLayout(pathData.SegmentsData, pathData.FakePathAngles2);
        PathLayouts[3].CreatePathLayout(pathData.SegmentsData, pathData.FakePathAngles3);
    }

    public PathLayoutCreator GetPathLayout(int layoutToFind)
    {
        return PathLayouts.Find(x => x.PathLayoutID == layoutToFind);
    }
}
