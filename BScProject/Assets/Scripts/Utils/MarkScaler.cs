using UnityEngine;

public class MarkScaler : MonoBehaviour
{

    [Header("Optional Particles")]
    [SerializeField] private GameObject _particles;

    private void OnEnable() 
    {
        Vector3 newScale = new()
        {
            x = ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius,
            y = transform.localScale.y,
            z = ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius
        };
        transform.localScale = newScale;

        if (_particles != null)
        {
            Vector3 particleScale = new()
            {
                x = ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius,
                y = _particles.transform.localScale.y,
                z = ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius
            };
            _particles.transform.localScale = particleScale;
        }
    }
}
