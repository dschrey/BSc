using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource)), RequireComponent(typeof(CapsuleCollider))]
public class Objective : MonoBehaviour
{
    public GameObject ObjectiveObject;
    public GameObject CapturedParticles;
    public GameObject HintParticles;
    public GameObject LockedParticles;
    public AudioClip HintAudio;
    public AudioClip CapturedAudio;
    public event Action ObjectiveCaptured;
    private Coroutine _collectionCoroutine;
    [SerializeField] private Transform _objectiveObjectSpawnpoint;
    private AudioSource _audioSource;
    private CapsuleCollider _collider;
    private bool _objectiveCaptured = false;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<CapsuleCollider>();
        _collider.radius = ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, ExperimentManager.Instance.ExperimentSettings.PlayerDetectionRadius);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnTriggerEnter(Collider collider)
    {
        if (_objectiveCaptured)
        {
            return;
        }

        if (collider.CompareTag("Player"))
        {
            StartCollectingObjective();
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (_objectiveCaptured)
        {
            return;
        }

        if (collider.CompareTag("Player"))
        {
            StopCapturingObjective();
        }        
    }

    // ---------- Start Methods ------------------------------------------------------------------------------------------------------------------------------

    private void StartCollectingObjective()
    {
        if (_collectionCoroutine != null)
        {
            return;
        }

        Debug.Log($"Objective.cs :: StartCollectingItem() : Started collecting {this}");
        
        _collectionCoroutine = StartCoroutine(CaptureObjective());
    }

    private void StopCapturingObjective()
    {
        if (_collectionCoroutine != null)
        {
            Debug.Log($"Objective.cs :: StopCollectingItem() : Stopped collecting {this}");
            StopCoroutine(_collectionCoroutine);
            _collectionCoroutine = null;
        }
    }

    private IEnumerator CaptureObjective()
    {
        yield return new WaitForSeconds(ExperimentManager.Instance.ExperimentSettings.ObjectiveRevealTime);
        SetObjectiveCaptured();
    }

    private void SetObjectiveCaptured()
    {
        Debug.Log($"Objective.cs :: SetObjectiveCaptured() : Captured {this}!");
        _objectiveCaptured = true;
        LockedParticles.SetActive(false);
        CapturedParticles.SetActive(true);
        PlayAudio(CapturedAudio);
        ObjectiveCaptured?.Invoke();
    }

    public void HideObjective()
    {
        LockedParticles.SetActive(false);
    }

    public void ShowObjective()
    {
        LockedParticles.SetActive(true);
    }

    public void ShowObjectiveHint()
    {
        StartCoroutine(CoroutineObjectiveHint());
    }

    private IEnumerator CoroutineObjectiveHint()
    {
        HintParticles.SetActive(true);
        PlayAudio(HintAudio);
        yield return new WaitForSeconds(2f);
        HintParticles.SetActive(false);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (_audioSource == null)
        {
            Debug.LogError($"Objective :: PlayAudio() : Audio source for {this} was not found.");
            return;    
        }
        _audioSource.PlayOneShot(clip);
    }

    public void SpawnObject(GameObject prefab)
    {
        ObjectiveObject = Instantiate(prefab, _objectiveObjectSpawnpoint);
    }
}
