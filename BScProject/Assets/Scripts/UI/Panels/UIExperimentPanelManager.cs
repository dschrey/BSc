using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;

// TODO REMOVE

[Obsolete("Class is deprecated and will be removed in the future.")]
public class UIExperimentPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject _experimentSetupPanel;
    [SerializeField] private GameObject _experimentRunningPanel;
    [SerializeField] private GameObject _experimentFinishedPanel;
    [SerializeField] private InputActionProperty _activationButton;
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private Transform _panelSpawnpoint;
    [SerializeField] private bool _isBillboard = true;
    [SerializeField] private float _distanceFromPlayer = 1f;
    [SerializeField] private float _panelHeight = 1.5f;
    private bool _menuVisible = true;

    // Update is called once per frame
    void Update()
    {
        if (_activationButton.action.WasPressedThisFrame())
        {
            if (ExperimentManager.Instance.ExperimentState == ExperimentState.Running)
            {
                _menuVisible = _experimentRunningPanel.activeSelf;
                _menuVisible = !_menuVisible;
                ToggleRunningPanel(_menuVisible);
            }
        }

        if (_isBillboard && _menuVisible)
        {
            transform.LookAt(_playerCamera);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);
        }
    }

    public void ShowSetupPanel()
    {
        _menuVisible = true;
        _experimentSetupPanel.SetActive(_menuVisible);
        _experimentSetupPanel.transform.localPosition = _panelSpawnpoint.position;
    }

    public void CloseSetupPanel()
    {
        _menuVisible = false;
        _experimentSetupPanel.SetActive(_menuVisible);
    }

    public void ShowFinishPanel()
    {
        _menuVisible = true;
        _experimentFinishedPanel.SetActive(_menuVisible);
        _experimentFinishedPanel.transform.position = _panelSpawnpoint.position;
    }

    public void ToggleRunningPanel(bool state)
    {
        _menuVisible = state;
        _experimentRunningPanel.SetActive(_menuVisible);
        if (state)
        {
            PositionPanel();
        }
    }

    public void PositionPanel()
    {
        LazyFollow lazyFollow = GetComponent<LazyFollow>();
        lazyFollow.enabled = false;
        XROrigin xROrigin = ExperimentManager.Instance._XROrigin.GetComponent<XROrigin>();
        Transform playerTransform = xROrigin.CameraFloorOffsetObject.transform;
        Vector3 targetPosition = playerTransform.position + playerTransform.forward * _distanceFromPlayer;
        targetPosition.y = _panelHeight;
        transform.position = targetPosition;
        lazyFollow.enabled = true;
    }

    public void ResetPanelPosition()
    {
        LazyFollow lazyFollow = GetComponent<LazyFollow>();
        lazyFollow.enabled = false;
        transform.position = _panelSpawnpoint.position;
        lazyFollow.enabled = true;
    }

}
