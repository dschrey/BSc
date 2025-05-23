using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public enum SoundType
{
    SoundSegmentHint,
    SoundSegmentUnlocked,
    SoundLoadingCenter,
    SoundStudyStart,
    InstructionReturnToCenter,
    InstructionBackTrackRight,
    InstructionBackTrackLeft,
    InstructionUnlockSegmentEasy,
    InstructionUnlockSegmentHard,
    InstructionPathLayoutSelection,
    InstructionPathBuilding,
    InstructionLandmarkSelection,
    InstructionLandmarkPlacementLeft,
    InstructionLandmarkPlacementRight,
    InstructionSegmentHintLeft,
    InstructionSegmentHintRight,
    SoundBackground
}

[Serializable]
public class SoundEntry
{
    public SoundType SoundType;
    public AudioClip AudioClip;
}

public class AudioManager : MonoBehaviour
{

    #region Variables
    public static AudioManager Instance { get; private set; }
    private AudioSource _playerAudioSource;
    private AudioSource _backgroundAudioSource;
    [SerializeField] private List<SoundEntry> sounds;
    private Dictionary<SoundType, AudioClip> soundMap;
    private Queue<AudioClip> _audioQueue = new();
    private bool _isPlayingQueue = false;

    #endregion
    #region Unity Methods

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    void Start()
    {
        soundMap = new();
        foreach (var entry in sounds)
        {
            if (!soundMap.ContainsKey(entry.SoundType))
                soundMap.Add(entry.SoundType, entry.AudioClip);
        }

        _backgroundAudioSource = GetComponent<AudioSource>();
        if (_backgroundAudioSource == null)
        {
            Debug.LogError($"Could not find background audio source.");
        }
    }

    #endregion
    #region Class Methods

    public void GetPlayerAudioSource()
    {
        _playerAudioSource = FindObjectOfType<XROrigin>().GetComponentInChildren<AudioSource>();
        if (_playerAudioSource == null)
        {
            Debug.LogError($"Could not get player audio source.");
        }
    }

    public void PlayAudio(SoundType type, AudioSource source = null)
    {
        if (soundMap.TryGetValue(type, out var clip))
        {
            if (source != null)
            {
                source.PlayOneShot(clip);
            }
            else
            {
                _audioQueue.Enqueue(clip);
                if (!_isPlayingQueue)
                {
                    StartCoroutine(PlayQueuedAudio());
                }
            }
        }
    }

    public void StopAudio(AudioSource source = null)
    {
        if (source != null)
        {
            source.Stop();
        }
        else
        {
            StopAllCoroutines();
            _playerAudioSource.Stop();
            _audioQueue.Clear();
            _isPlayingQueue = false;
        }
    }

    private IEnumerator PlayQueuedAudio()
    {
        _isPlayingQueue = true;

        while (_audioQueue.Count > 0)
        {
            AudioClip currentClip = _audioQueue.Dequeue();
            _playerAudioSource.PlayOneShot(currentClip);
            yield return new WaitForSeconds(currentClip.length);
        }

        _isPlayingQueue = false;
    }

    public void ToggleBackgroundLoop()
    {
        if (_backgroundAudioSource.isPlaying)
        {
            _backgroundAudioSource.Stop();
            _backgroundAudioSource.clip = null;
            _backgroundAudioSource.loop = false;
        }
        else if (soundMap.TryGetValue(SoundType.SoundBackground, out var clip))
        {
            _backgroundAudioSource.clip = clip;
            _backgroundAudioSource.loop = true;
            _backgroundAudioSource.Play();
        }
    }

    #endregion

}
