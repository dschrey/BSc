using UnityEngine;

[RequireComponent(typeof(Renderer), typeof(BoxCollider))]
public class Border : MonoBehaviour
{
    private Renderer quadRenderer;

    private void Start()
    {
        quadRenderer = GetComponent<Renderer>();
        quadRenderer.enabled = false;

        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            quadRenderer.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            quadRenderer.enabled = false;
        }
    }
}
