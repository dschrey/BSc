using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPathSelectionHandler : MonoBehaviour
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private GameObject _pathSelectionPrefab;
    [SerializeField] private GameObject _selectionParent;
    private List<PathSelectionOption> _pathOptions = new();

    // ---------- Unity Methods --------------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _confirmButton.onClick.AddListener(OnPathSelectionConfirmed);
        _confirmButton.interactable = false;

        foreach (Sprite spite in AssessmentManager.Instance.CurrentPath.PathImageSelection)
        {
            PathSelectionOption pathOption = Instantiate(_pathSelectionPrefab, _selectionParent.transform).GetComponent<PathSelectionOption>();
            pathOption.Initialize(spite, _selectionParent.GetComponent<ToggleGroup>());
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
        }

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

}
