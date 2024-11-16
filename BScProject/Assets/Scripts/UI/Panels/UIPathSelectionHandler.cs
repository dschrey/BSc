using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPathSelectionHandler : MonoBehaviour
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private GameObject _pathSelectionPrefab;
    [SerializeField] private GameObject _selectionParent;
    private readonly List<PathSelectionOption> _pathOptions = new();

    // ---------- Unity Methods --------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _confirmButton.onClick.AddListener(OnPathSelectionConfirmed);
        _confirmButton.interactable = false;

        List<Sprite> images = Utils.Shuffle(AssessmentManager.Instance.CurrentPath.PathImageSelection);

        foreach (Sprite sprite in images)
        {
            PathSelectionOption pathOption = Instantiate(_pathSelectionPrefab, _selectionParent.transform).GetComponent<PathSelectionOption>();
            pathOption.Initialize(sprite, _selectionParent.GetComponent<ToggleGroup>());
            pathOption.PathSelectionChanged.AddListener(OnSelectedPathChanged);
            _pathOptions.Add(pathOption);
        }

    }

    private void OnDisable() 
    {
        _confirmButton.onClick.RemoveListener(OnPathSelectionConfirmed);

        foreach (PathSelectionOption pathOption in _pathOptions)
        {
            pathOption.PathSelectionChanged.RemoveListener(OnSelectedPathChanged);
            Destroy(pathOption.gameObject);
        }
        _pathOptions.Clear();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSelectedPathChanged(Sprite selectedPathImage)
    {
        if (selectedPathImage != null)
        {
            _confirmButton.interactable = true;
        }
        else
        {
            _confirmButton.interactable = false;
        }
        AssessmentManager.Instance.SetSelectedPathImage(selectedPathImage);
    }

    private void OnPathSelectionConfirmed()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------
}
