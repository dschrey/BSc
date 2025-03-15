using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class Utils
{

    /// <summary>
    /// Shuffles a list based on a (optionally given) seed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Sets the layer of a gameobject and it's children.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layerIndex"></param>
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
                if (!initialized)
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
    public static void SetObjectColliders(GameObject gameObject, bool state)
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

    /// <summary>
    /// Calculates the boundaries of a given linerenderer.
    /// </summary>
    /// <param name="lineRenderer"></param>
    /// <returns></returns>
    public static Bounds CalculateLineRendererBounds(LineRenderer lineRenderer)
    {
        Bounds bounds = new(lineRenderer.GetPosition(0), Vector3.zero);
        for (int i = 1; i < lineRenderer.positionCount; i++)
        {
            bounds.Encapsulate(lineRenderer.GetPosition(i));
        }
        return bounds;
    }

    /// <summary>
    /// Sets min, max and current value for a given Slider.
    /// </summary>
    /// <param name="slider"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="currentValue"></param>
    public static void SetSliderSettings(Slider slider, float minValue, float maxValue, float currentValue)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        if (currentValue > maxValue)
            slider.value = maxValue;
        else if (currentValue < minValue)
            slider.value = minValue;
        else
            slider.value = currentValue;
    }
}