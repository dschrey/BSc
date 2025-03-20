using System;
using System.Collections;
using UnityEngine;

[Obsolete]
[RequireComponent(typeof(AudioSource))]
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
    private bool _objectiveCaptured = false;
    // private MovementDetection _playerMovementDetection;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        // _playerMovementDetection = gameObject.AddComponent<MovementDetection>();
        // _playerMovementDetection.PlayerEnteredDectectionZone += OnPlayerEnterObjective;
        // _playerMovementDetection.PlayerExitedDectectionZone += OnPlayerExitObjective;

    }

    void OnDisable()
    {
        // _playerMovementDetection.PlayerEnteredDectectionZone -= OnPlayerEnterObjective;
        // _playerMovementDetection.PlayerExitedDectectionZone -= OnPlayerExitObjective;
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------------ 

    // ---------- Start Methods ------------------------------------------------------------------------------------------------------------------------------


    private IEnumerator CaptureObjective()
    {
        yield return new WaitForSeconds(DataManager.Instance.Settings.ObjectiveRevealTime);
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
        if (ObjectiveObject != null)
            ObjectiveObject.SetActive(false);
    }

    public void ShowObjective()
    {
        LockedParticles.SetActive(true);
        if (ObjectiveObject != null)
            ObjectiveObject.SetActive(true);
    }

    public void ShowObjectiveHint()
    {
        StartCoroutine(CoroutineObjectiveHint());
    }

    private IEnumerator CoroutineObjectiveHint()
    {
        HintParticles.SetActive(true);
        PlayHindAudio();
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
    
    public void PlayHindAudio() => PlayAudio(HintAudio);
}
