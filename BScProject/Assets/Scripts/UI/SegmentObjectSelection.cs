using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegmentObjectSelection : MonoBehaviour
{
    [SerializeField] private Image _segmentLabelImage;
    [SerializeField] private TMP_Text _segmentLabelText;
    [SerializeField] private Button _buttonPrevious;
    [SerializeField] private Button _buttonNext;
    [SerializeField] private Image _selectedObjectImage;
    public event Action<int> SelectedObjectChanged;
    private List<Sprite> _objectChoises = new();
    private int _segmentID;
    private int _selectedSpriteID;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _buttonPrevious.onClick.AddListener(OnPreviousButtonClick);
        _buttonNext.onClick.AddListener(OnNextButtonClick);

        _selectedSpriteID = -1;
        _selectedObjectImage = null;
        _objectChoises.Clear();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnNextButtonClick()
    {
        _selectedSpriteID = (_selectedSpriteID + 1) % _objectChoises.Count;
        UpdateObjectImage();
    }

    private void OnPreviousButtonClick()
    {
        _selectedSpriteID = (_selectedSpriteID - 1 + _objectChoises.Count) % _objectChoises.Count;
        UpdateObjectImage();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void SetSegmentLabel(int segmentID, Color segmentColor)
    {
        _segmentID = segmentID;
        _segmentLabelImage.color = segmentColor;
        if (_segmentLabelText != null)
        {
            _segmentLabelText.color = segmentColor;
            _segmentLabelText.text = (segmentID + 1).ToString();
        }
    }

    public void AddObjectChoise(Sprite choise)
    {
        _objectChoises.Add(choise);
    }

    private void UpdateObjectImage()
    {
        _selectedObjectImage.sprite = _objectChoises[_selectedSpriteID];
        SelectedObjectChanged?.Invoke(_segmentID);
    }

    public Sprite GetSelectedSprite()
    {
        return _selectedObjectImage.sprite;
    }
}
