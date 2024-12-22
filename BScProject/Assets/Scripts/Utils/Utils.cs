using System.Collections.Generic;
using Unity.VisualScripting;
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

    /// <summary>
    /// Calculates the boundary that encapsules a single objects
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public static Bounds CalculateObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    /// <summary>
    /// Calculates the boundary that encapsules multiple objects
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public static Bounds CalculateObjectBounds(List<GameObject> objects)
    {
        Bounds combinedBounds = new(Vector3.zero, Vector3.zero);
        bool initialized = false;

        foreach (GameObject obj in objects)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                if (! initialized)
                {
                    combinedBounds = renderer.bounds;
                    initialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }
        }
        return combinedBounds;
    }

    /// <summary>
    /// Adjusts the camera to the bounds and looks at the center.
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="objectBounds"></param>
    public static void AdjustCameraToBounds(Camera camera, Bounds objectBounds)
    {
        camera.orthographicSize = Mathf.Max(objectBounds.size.y, objectBounds.size.x) / 2f + 0.05f;

        camera.transform.position = new Vector3(objectBounds.center.x, objectBounds.center.y, camera.transform.position.z);
        camera.transform.LookAt(objectBounds.center);
    }

    /// <summary>
    /// Adjusts the camera to capture the all objects passed.
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="objects"></param>
    public static void AdjustCameraToObjects(Camera camera, List<GameObject> objects)
    {
        Bounds combinedBounds = CalculateObjectBounds(objects);
        camera.orthographicSize = Mathf.Max(combinedBounds.size.x, combinedBounds.size.z) / 2f + 0.05f;
        camera.transform.position = new Vector3(combinedBounds.center.x, camera.transform.position.y, combinedBounds.center.z);
    }

    /// <summary>
    /// Iterates through the children of the gameObject and sets their colliders to given state.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="state"></param>
    public static void ToggleObjectColliders(GameObject gameObject, bool state)
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = state;
        }
    }

    /// <summary>
    /// Iterates through the children of the gameObject and sets their renderer to given state.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="state"></param>
    public static void ToggleObjectRenderers(GameObject gameObject, bool state)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = state;
        }
    }
}