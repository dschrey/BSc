using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Path", menuName = "Path/PathSet", order = 0)]
public class PathSet : ScriptableObject
{
    public string SetName;

    public List<PathData> PathList = new();

}
