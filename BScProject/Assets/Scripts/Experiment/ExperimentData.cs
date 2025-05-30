using System;
using System.Collections.Generic;

public enum FloorType {Default, Omnideck}

[Serializable]
public class Trail
{
    public string name;
    public FloorType floor;
    public string selectionName;
}

[Serializable]
public class ParticipantData
{
    public int id = -1;
    public List<string> pathSetOrder = new();
    public List<string> locomotionOrder = new();
}

[Serializable]
public class ParticipantDataWrapper
{
    public List<ParticipantData> participants;
}

[Serializable]
public class ExperimentData
{
    public int id = -1;
    public Dictionary<Trail, PathData> paths;

    public ExperimentData(int id)
    {
        this.id = id;
        paths = new();
    }

    public Trail GetTrailData(string trailName)
    {
        foreach (Trail trail in paths.Keys)
        {
            if (trail.selectionName != trailName)
                continue;

            return trail;
        }
        return null;
    }
    
    public PathData GetPathData(string trailName)
    {
        foreach (var path in paths)
        {
            if (path.Key.selectionName != trailName)
                continue;

            return path.Value;
        }
        return null;
    }
 
}