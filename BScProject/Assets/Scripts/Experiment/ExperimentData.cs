using System;
using System.Collections.Generic;

public enum FloorType {DEFAULT, OMNIDECK}

[Serializable]
public class Trail  // Represents a path, since Path already exists.
{
    public string name;
    public FloorType floor;
    public string selectionName;
}

[Serializable]
public class ParticipantData
{
    public int id = -1;
    public Trail[] paths;
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