using UnityEngine;

public class MarkScaler : MonoBehaviour
{

    [Header("Optional Particles")]
    [SerializeField] private GameObject[] _particles;

    private void OnEnable() 
    {
        Vector3 newScale = new()
        {
            x = DataManager.Instance.Settings.PlayerDetectionRadius * 2,
            y = transform.localScale.y,
            z = DataManager.Instance.Settings.PlayerDetectionRadius * 2
        };
        transform.localScale = newScale;


        foreach (GameObject particle in _particles)
        {
            Vector3 particleScale = new()
            {
                x = DataManager.Instance.Settings.PlayerDetectionRadius * 2,
                y = particle.transform.localScale.y,
                z = DataManager.Instance.Settings.PlayerDetectionRadius * 2
            };
            particle.transform.localScale = particleScale;
        }
    }
}
