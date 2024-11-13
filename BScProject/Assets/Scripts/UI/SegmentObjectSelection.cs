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
    [SerializeField] private RawImage _selectedObjectImage;
    [SerializeField] private Texture _emptyTexture;
    public event Action<int> SelectedObjectChanged;
    public List<RenderTexture> _objectRenderChoises = new();
    private int _segmentID;
    private int _selectedRenderTextureID = -1;
    public bool ObjectSelected => _selectedRenderTextureID != -1;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _buttonPrevious.onClick.AddListener(OnPreviousButtonClick);
        _buttonNext.onClick.AddListener(OnNextButtonClick);

        _selectedRenderTextureID = -1;
        _selectedObjectImage.texture = _emptyTexture;
        _objectRenderChoises.Clear();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnNextButtonClick()
    {
        _selectedRenderTextureID = (_selectedRenderTextureID + 1) % _objectRenderChoises.Count;
        UpdateObjectImage();
    }

    private void OnPreviousButtonClick()
    {
        _selectedRenderTextureID = (_selectedRenderTextureID - 1 + _objectRenderChoises.Count) % _objectRenderChoises.Count;
        UpdateObjectImage();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int segmentID, Color segmentColor)
    {
        _segmentID = segmentID;
        _segmentLabelImage.color = segmentColor;
        if (_segmentLabelText != null)
        {
            _segmentLabelText.color = segmentColor;
            _segmentLabelText.text = (segmentID + 1).ToString();
        }
    }

    public void AddObjectChoices(List<RenderTexture> textures)
    {
        _objectRenderChoises.AddRange(textures);
    }

    private void UpdateObjectImage()
    {
        _selectedObjectImage.texture = _objectRenderChoises[_selectedRenderTextureID];
        SelectedObjectChanged?.Invoke(_segmentID);
    }

    public RenderTexture GetSelectedRenderTexture()
    {
        return _objectRenderChoises[_selectedRenderTextureID];
    }
}
