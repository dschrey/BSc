using UnityEngine;

public class MarkScaler : MonoBehaviour
{

    [Header("Optional Particles")]
    [SerializeField] private GameObject[] _particles;

    private void OnEnable() 
    {
        Vector3 newScale = new()
        {
            x = DataManager.Instance.Settings.PlayerDetectionRadius,
            y = transform.localScale.y,
            z = DataManager.Instance.Settings.PlayerDetectionRadius
        };
        transform.localScale = newScale;


        foreach (GameObject particle in _particles)
        {
            Vector3 particleScale = new()
            {
                x = 2 *DataManager.Instance.Settings.PlayerDetectionRadius,
                y = particle.transform.localScale.y,
                z = 2 *DataManager.Instance.Settings.PlayerDetectionRadius
            };
            particle.transform.localScale = particleScale;
        }
    }
}
