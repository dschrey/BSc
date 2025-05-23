using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ReturnToCenterHandler : MonoBehaviour
{
    public UnityEvent PlayerInCenter = new();
    private Coroutine _confirmCenterRoutine;
    private MovementDetection _playerMovementDetection;

    [SerializeField] private GameObject _moveToCenterHighlight;
    [SerializeField] private GameObject _inCenterHighlight;
    [SerializeField] private bool _activePrompt = false;
    public bool InCenter = false;

    void Start()
    {
        _playerMovementDetection = GetComponent<MovementDetection>();
        if (_playerMovementDetection == null)
        {
            Debug.LogError($"Could not find center movement detector");
            return;
        }
        _playerMovementDetection.PlayerEnteredDectectionZone += OnPlayerEnteredCenter;
        _playerMovementDetection.PlayerExitedDectectionZone += OnPlayerLeftCenter;
        _moveToCenterHighlight.SetActive(false);
        _inCenterHighlight.SetActive(false);
    }
    
    void OnDisable()
    {

        _playerMovementDetection.PlayerEnteredDectectionZone -= OnPlayerEnteredCenter;
        _playerMovementDetection.PlayerExitedDectectionZone -= OnPlayerLeftCenter;
    } 

    public void PromptReturnToCenter()
    {
        _activePrompt = true;
        if (InCenter)
        {
            _inCenterHighlight.SetActive(true);
            AudioManager.Instance.PlayAudio(SoundType.SoundLoadingCenter);
            _confirmCenterRoutine = StartCoroutine(ConfirmCenter());
            return;
        }
        AudioManager.Instance.PlayAudio(SoundType.InstructionReturnToCenter);
        _moveToCenterHighlight.SetActive(true);
    }

    private void OnPlayerEnteredCenter()
    {
        InCenter = true;
        if (!_activePrompt) return;
        if (_confirmCenterRoutine != null)
        {
            return;
        }

        _moveToCenterHighlight.SetActive(false);
        _inCenterHighlight.SetActive(true);
        AudioManager.Instance.PlayAudio(SoundType.SoundLoadingCenter);
        _confirmCenterRoutine = StartCoroutine(ConfirmCenter());
    }

    private void OnPlayerLeftCenter()
    {
        InCenter = false;
        if (!_activePrompt) return;
        if (_confirmCenterRoutine != null)
        {
            StopCoroutine(_confirmCenterRoutine);
            AudioManager.Instance.StopAudio();
            _confirmCenterRoutine = null;
            _inCenterHighlight.SetActive(false);
            _moveToCenterHighlight.SetActive(true);
        }
    }

    private IEnumerator ConfirmCenter()
    {
        yield return new WaitForSeconds(2);
        _activePrompt = false;
        _moveToCenterHighlight.SetActive(false);
        _inCenterHighlight.SetActive(false);
        _confirmCenterRoutine = null;
        PlayerInCenter.Invoke();
    }
}
