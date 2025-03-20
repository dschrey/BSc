using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PathSegment : MonoBehaviour
{
    public PathSegmentData PathSegmentData;
    public event Action SegmentCompleted;
    private MovementDetection _playerMovementDetection;
    private Coroutine _collectionCoroutine;
    private bool _objectiveCaptured = false;

    [Header("Objects")]
    [SerializeField] private Transform _objectiveObjectSpawnpoint;
    public GameObject LandmarkObject;
    public GameObject SegmentObstacle;
    private GameObject _obstaclePrefab = null;

    [Header("Particles")]
    public GameObject ObjectiveObject;
    public GameObject CapturedParticles;
    public GameObject HintParticles;
    public GameObject LockedParticles;

    [Header("Audio")]
    private AudioSource _audioSource;
    [SerializeField] private AudioClip Hint;
    [SerializeField] private AudioClip Captured;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        _playerMovementDetection = GetComponent<MovementDetection>();
        if (_playerMovementDetection == null)
        {
            Debug.LogError($"Could not find objective for {this} path segment.");
            return;
        }
        _playerMovementDetection.PlayerEnteredDectectionZone += OnPlayerEnterObjective;
        _playerMovementDetection.PlayerExitedDectectionZone += OnPlayerExitObjective;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = PathSegmentData.SegmentColor;
        Gizmos.DrawWireSphere(transform.position, DataManager.Instance.Settings.PlayerDetectionRadius);
    }

    void OnDisable()
    {
        _playerMovementDetection.PlayerEnteredDectectionZone -= OnPlayerEnterObjective;
        _playerMovementDetection.PlayerExitedDectectionZone -= OnPlayerExitObjective;
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------------

    private void OnObjectiveCaptured()
    {
        if (LandmarkObject != null)
            LandmarkObject.SetActive(false);
        if (SegmentObstacle != null)
            SegmentObstacle.SetActive(false);
        SegmentCompleted?.Invoke();
    }

    private void OnPlayerEnterObjective()
    {
        if (_objectiveCaptured)
        {
            return;
        }
        StartCapturingObjective();
    }

    private void OnPlayerExitObjective()
    {
        if (_objectiveCaptured)
        {
            return;
        }
        StopCapturingObjective();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------------

    public void Initialize(PathSegmentData pathSegmentData, GameObject obstaclePrefab, PathObjects pathObjects)
    {
        PathSegmentData = pathSegmentData;
        _obstaclePrefab = obstaclePrefab;
        SpawnSegmentObjects(pathObjects);
    }

    private void StartCapturingObjective()
    {
        if (_collectionCoroutine != null)
        {
            return;
        }
        _collectionCoroutine = StartCoroutine(CaptureObjective());
    }

    private void StopCapturingObjective()
    {
        if (_collectionCoroutine != null)
        {
            StopCoroutine(_collectionCoroutine);
            _collectionCoroutine = null;
        }
    }

    private IEnumerator CaptureObjective()
    {
        yield return new WaitForSeconds(DataManager.Instance.Settings.ObjectiveRevealTime);
        SetObjectiveCaptured();
    }

    private void SetObjectiveCaptured()
    {
        Debug.Log($"Captured Segment {PathSegmentData.SegmentID}!");
        _objectiveCaptured = true;
        LockedParticles.SetActive(false);
        CapturedParticles.SetActive(true);
        PlayAudio(Captured);
        OnObjectiveCaptured();
    }

    public void SetObjectiveInvisible()
    {
        LockedParticles.SetActive(false);
        if (ObjectiveObject != null)
            ObjectiveObject.SetActive(false);
    }

    public void ShowSegmentObjective()
    {
        LockedParticles.SetActive(true);
        if (ObjectiveObject != null)
            ObjectiveObject.SetActive(true); ;
    }

    public void PlaySegmentObjectiveHint()
    {
        StartCoroutine(CoroutineObjectiveHint());
    }

    private IEnumerator CoroutineObjectiveHint()
    {
        HintParticles.SetActive(true);
        PlayAudio(Hint);
        yield return new WaitForSeconds(2f);
        HintParticles.SetActive(false);
    }

    private void SpawnSegmentObjects(PathObjects pathObjects)
    {
        if ((pathObjects & PathObjects.Hovering) != 0)
        {
            SpawnHoverObject();
        }

        if ((pathObjects & PathObjects.Landmarks) != 0)
        {
            SpawnLandmarkObject();
        }

        if ((pathObjects & PathObjects.Obstacles) != 0)
        {
            SpawnSegmentObstacle();
        }

    }

    private void SpawnHoverObject()
    {
        GameObject prefab = ResourceManager.Instance.GetHoverObject(PathSegmentData.ObjectiveObjectID);
        if (prefab == null)
        {
            Debug.LogWarning($"Objective object found.");
            return;
        }
        ObjectiveObject = Instantiate(prefab, _objectiveObjectSpawnpoint);
    }

    private void SpawnLandmarkObject()
    {

        float angleInRadians = PathSegmentData.AngleToLandmark * Mathf.Deg2Rad;
        Vector3 relativePosition = new(
            PathSegmentData.LandmarkObjectDistanceToObjective * Mathf.Cos(angleInRadians),
            0,
            PathSegmentData.LandmarkObjectDistanceToObjective * Mathf.Sin(angleInRadians)
        );
        Vector3 objectSpawnpoint = transform.position + relativePosition;

        GameObject prefab = ResourceManager.Instance.GetLandmarkObject(PathSegmentData.LandmarkObjectID);
        if (prefab == null)
        {
            Debug.LogWarning($"Landmark object found.");
            return;
        }

        LandmarkObject = Instantiate(prefab, objectSpawnpoint, Quaternion.identity, transform);
    }

    private void SpawnSegmentObstacle()
    {
        if (!PathSegmentData.ShowObstacle) return;
        if (_obstaclePrefab == null)
        {
            Debug.LogError($"Obstacle prefab is null.");
            return;
        }
        SegmentObstacle = Instantiate(_obstaclePrefab, transform);
        Vector3 objectSpawnpoint = transform.position + PathSegmentData.RelativeObstaclePositionToObjective;
        objectSpawnpoint.y = SegmentObstacle.transform.position.y;
        SegmentObstacle.transform.SetPositionAndRotation(objectSpawnpoint, PathSegmentData.ObstacleRotation);
        SegmentObstacle.transform.localScale = PathSegmentData.Scale;
    }

    private void PlayAudio(AudioClip clip)
    {
        if (_audioSource == null)
        {
            Debug.LogError($"Audio source for segment {PathSegmentData.SegmentID} was not found.");
            return;
        }
        _audioSource.PlayOneShot(clip);
    }

    public void PlayHintAudio() => PlayAudio(Hint);

}