using System.Collections.Generic;
using UnityEngine;

class Utils
{
    public static List<T> Shuffle<T>(List<T> list, int seed = -1)
    {
        System.Random rng;
        if (seed == -1)
        {
           rng = new();
        }
        else
        {
            rng = new(seed);
        }

        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = rng.Next(i, list.Count);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
        return list;
    }

    public static void SetLayerRecursively(GameObject obj, int layerIndex)
    {
        obj.layer = layerIndex;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layerIndex);
        }
    }   

}