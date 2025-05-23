using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PathSegment : MonoBehaviour
{
    #region Variables
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
    public GameObject HoverObject;
    public GameObject CapturedParticles;
    public GameObject HintParticles;
    public GameObject LockedParticles;

    [Header("Audio")]
    private AudioSource _audioSource;
    [SerializeField] private AudioClip Hint;
    [SerializeField] private AudioClip Captured;

    private bool _isCapturing = false;

    #endregion
    #region Unity Methods

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

    #endregion
    #region Listener Methods

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
        if (PathManager.Instance.GetCurrentSegmentID() -1 != PathSegmentData.SegmentID)
        {
            return;
        }

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

    #endregion
    #region Class Methods

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
        _isCapturing = true;
        _collectionCoroutine = StartCoroutine(CaptureObjective());
    }

    private void StopCapturingObjective()
    {
        if (_collectionCoroutine != null)
        {
            StopCoroutine(_collectionCoroutine);
            _isCapturing = false;
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
        _objectiveCaptured = true;
        _isCapturing = false;
        LockedParticles.SetActive(false);
        HintParticles.SetActive(false);
        CapturedParticles.SetActive(true);
        AudioManager.Instance.PlayAudio(SoundType.SoundSegmentUnlocked, _audioSource);
        OnObjectiveCaptured();
    }

    public void SetSegmentInvisible()
    {
        HintParticles.SetActive(false);
        LockedParticles.SetActive(false);
        if (HoverObject != null)
            HoverObject.SetActive(false);
    }

    public void ShowSegmentObjective()
    {
        LockedParticles.SetActive(true);
        if (HoverObject != null)
            HoverObject.SetActive(true);
    }

    public void ShowSegmentHint()
    {
        if (_objectiveCaptured) return;
        if (_isCapturing) return;
        StartCoroutine(CoroutineObjectiveHint());
    }

    private IEnumerator CoroutineObjectiveHint()
    {
        HintParticles.SetActive(true);
        AudioManager.Instance.PlayAudio(SoundType.SoundSegmentHint, _audioSource);
        yield return new WaitForSeconds(2f);
        HintParticles.SetActive(false);
    }

    private void SpawnSegmentObjects(PathObjects pathObjects)
    {
        if ((pathObjects & PathObjects.Hover) != 0)
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
            Debug.LogWarning($"Hover object not found.");
            return;
        }
        HoverObject = Instantiate(prefab, _objectiveObjectSpawnpoint);
    }

    private void SpawnLandmarkObject()
    {

        float angleInRadians = PathSegmentData.AngleToLandmark * Mathf.Deg2Rad;
        Vector3 relativePosition = new(
            PathSegmentData.LandmarkDistanceToSegment * Mathf.Sin(angleInRadians),
            0,
            PathSegmentData.LandmarkDistanceToSegment * Mathf.Cos(angleInRadians)
        );
        Vector3 objectSpawnpoint = transform.position + relativePosition;

        GameObject prefab = ResourceManager.Instance.GetLandmarkObject(PathSegmentData.LandmarkObjectID);
        if (prefab == null)
        {
            Debug.LogWarning($"Landmark object not found.");
            return;
        }

        LandmarkObject = Instantiate(prefab, objectSpawnpoint, Quaternion.identity, transform);
    }

    private void SpawnSegmentObstacle()
    {
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

    /// <summary>
    /// Set segment visual based on type.
    /// 1: Locked Visual;
    /// 2: Hint Visual;
    /// 3: Unlocked Visual;
    /// Any: All
    /// </summary>
    /// <param name="type"></param>
    /// <param name="state"></param>
    public void SetParticleVisuals(int type, bool state)
    {
        switch (type)
        {
            case 1:
                LockedParticles.SetActive(state);
                break;
            case 2:
                HintParticles.SetActive(state);
                break;
            case 3:
                CapturedParticles.SetActive(state);
                break;
            default:
                CapturedParticles.SetActive(state);
                HintParticles.SetActive(state);
                LockedParticles.SetActive(state);
                break;
        }
    }

    /// <summary>
    /// Set segment visual based on type.
    /// 1: Landmark;
    /// 2: Hover;
    /// 3: Obstacle;
    /// Any: All
    /// </summary>
    /// <param name="type"></param>
    /// <param name="state"></param>
    public void SetObjectsVisuals(int type, bool state)
    {
        switch (type)
        {
            case 1:
                if (LandmarkObject != null)
                    LandmarkObject.SetActive(state);
                break;
            case 2:
                if (HoverObject != null)
                    HoverObject.SetActive(state);
                break;
            case 3:
                if (SegmentObstacle != null)
                    SegmentObstacle.SetActive(state);
                break;
            default:
                if (HoverObject != null)
                    HoverObject.SetActive(state);
                if (LandmarkObject != null)
                    LandmarkObject.SetActive(state);
                if (SegmentObstacle != null)
                    SegmentObstacle.SetActive(state);
                break;
        }
    }
    
    #endregion
}