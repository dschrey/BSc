using UnityEngine;

public class MarkScaler : MonoBehaviour
{

    [Header("Optional Particles")]
    [SerializeField] private GameObject[] _particles;

    private void OnEnable() 
    {
        Vector3 newScale = new()
        {
            x = ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius,
            y = transform.localScale.y,
            z = ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius
        };
        transform.localScale = newScale;


        foreach (GameObject particle in _particles)
        {
            Vector3 particleScale = new()
            {
                x = 2 *ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius,
                y = particle.transform.localScale.y,
                z = 2 *ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius
            };
            particle.transform.localScale = particleScale;
        }
    }
}
