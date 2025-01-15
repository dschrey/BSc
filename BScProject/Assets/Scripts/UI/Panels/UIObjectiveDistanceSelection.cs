using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Class is deprecated and will be removed in the future.")]
public class PathSegmentDistanceData
{
    public PathSegmentData PathSegmentData;
    public float SelectedDistance;

    public PathSegmentDistanceData(PathSegmentData data)
    {
        PathSegmentData = data;
        SelectedDistance = -1;
    }
}
[Obsolete("Class is deprecated and will be removed in the future.")]
public class UIObjectiveDistanceSelection : MonoBehaviour
{
        
    [Header("Segment Selection")]
    [SerializeField] private Transform _segmentIndicatorParent;
    [SerializeField] private GameObject _segmentIndicatorPrefab;
    [SerializeField] private Button _buttonPrevious;
    [SerializeField] private Button _buttonNext;
    [SerializeField] private TMP_Text _textSelectedSegment;
    [SerializeField] private TMP_Text _selectionIndicator;
    [SerializeField] private TMP_Text _textSegmentDistance;

    [Header("Numpad Buttons")]
    [SerializeField] private Button _buttonZero;
    [SerializeField] private Button _buttonOne;
    [SerializeField] private Button _buttonTwo;
    [SerializeField] private Button _buttonThree;
    [SerializeField] private Button _buttonFour;
    [SerializeField] private Button _buttonFive;
    [SerializeField] private Button _buttonSix;
    [SerializeField] private Button _buttonSeven;
    [SerializeField] private Button _buttonEight;
    [SerializeField] private Button _buttonNine;
    [SerializeField] private Button _buttonRemove;
    [SerializeField] private Button _buttonClear;
    [SerializeField] private Button _buttonComma;
    private StringBuilder _inputBuffer = new();
    private bool _isDecimal = false;

    [Header("Misc")]
    [SerializeField] private Image _selectedPathImage;
    [SerializeField] private Button _confirmButton;

    private readonly List<UISegmentIndicator> _segmentIndicators = new();
    private readonly List<PathSegmentDistanceData> _segmentDistanceData = new();
    private PathSegmentDistanceData _currentSegment;
    private int _selectedSegmentID;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
    private void OnEnable() 
    {
        _buttonPrevious.onClick.AddListener(OnPreviousSegmentButtonClick);
        _buttonNext.onClick.AddListener(OnNextSegmentButtonClick);
        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        InitializeNumpad();

        if (_segmentDistanceData.Count == 0)
        {
            _selectedSegmentID = 0;

            AssessmentManager.Instance.CurrentPath.SegmentsData.ForEach(s =>
            {
                _segmentDistanceData.Add(new PathSegmentDistanceData(s));
                UISegmentIndicator segmentIndicator = Instantiate(_segmentIndicatorPrefab, _segmentIndicatorParent).GetComponent<UISegmentIndicator>();
                _segmentIndicators.Add(segmentIndicator);
            });
        }

        _confirmButton.interactable = VerifyDistanceValues();
        // _selectedPathImage.sprite = AssessmentManager.Instance.CurrentPathAssessment.SelectedPathLayoutID;

        UpdateSelectedSegment();
    }

    private void OnDisable() 
    {
        _buttonPrevious.onClick.RemoveListener(OnPreviousSegmentButtonClick);
        _buttonNext.onClick.RemoveListener(OnNextSegmentButtonClick);
        _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        TerminateNumpad();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------


    private void OnNextSegmentButtonClick()
    {
        _segmentIndicators[_selectedSegmentID].Toggle(false);
        _selectedSegmentID = (_selectedSegmentID + 1) % _segmentDistanceData.Count;
        UpdateSelectedSegment();

    }

    private void OnPreviousSegmentButtonClick()
    {
        _segmentIndicators[_selectedSegmentID].Toggle(false);
        _selectedSegmentID = (_selectedSegmentID - 1 + _segmentDistanceData.Count) % _segmentDistanceData.Count;
        UpdateSelectedSegment();

    }

    private void OnConfirmButtonClicked()
    {
        ResetNumpadInput();
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }

    private void OnButtonZeroClick()
    {
        AddNumber(0);
    }

    private void OnButtonOneClick()
    {
        AddNumber(1);
    }

    private void OnButtonTwoClick()
    {
        AddNumber(2);
    }

    private void OnButtonThreeClick()
    {
        AddNumber(3);
    }

    private void OnButtonFourClick()
    {
        AddNumber(4);
    }

    private void OnButtonFiveClick()
    {
        AddNumber(5);
    }

    private void OnButtonSixClick()
    {
        AddNumber(6);
    }

    private void OnButtonSevenClick()
    {
        AddNumber(7);
    }

    private void OnButtonEightClick()
    {
        AddNumber(8);
    }

    private void OnButtonNineClick()
    {
        AddNumber(9);
    }

    private void OnButtonCommaClick()
    {
        AddDecimalPoint();
    }

    private void OnButtonClearClick()
    {
        _inputBuffer.Clear();
        _isDecimal = false;
        _segmentIndicators[_selectedSegmentID].SetState(false);
        _currentSegment.SelectedDistance = -1;
        _textSegmentDistance.text = 0 + "m";
        _confirmButton.interactable = false;
    }

    private void OnButtonRemoveClick()
    {
        RemoveLastInput();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void InitializeNumpad()
    {
        
        _buttonComma.onClick.AddListener(OnButtonCommaClick);
        _buttonZero.onClick.AddListener(OnButtonZeroClick);
        _buttonOne.onClick.AddListener(OnButtonOneClick);
        _buttonTwo.onClick.AddListener(OnButtonTwoClick);
        _buttonThree.onClick.AddListener(OnButtonThreeClick);
        _buttonFour.onClick.AddListener(OnButtonFourClick);
        _buttonFive.onClick.AddListener(OnButtonFiveClick);
        _buttonSix.onClick.AddListener(OnButtonSixClick);
        _buttonSeven.onClick.AddListener(OnButtonSevenClick);
        _buttonEight.onClick.AddListener(OnButtonEightClick);
        _buttonNine.onClick.AddListener(OnButtonNineClick);
        _buttonClear.onClick.AddListener(OnButtonClearClick);
        _buttonRemove.onClick.AddListener(OnButtonRemoveClick);
    }

    private void TerminateNumpad()
    {
        _buttonComma.onClick.RemoveListener(OnButtonCommaClick);
        _buttonZero.onClick.RemoveListener(OnButtonZeroClick);
        _buttonOne.onClick.RemoveListener(OnButtonOneClick);
        _buttonTwo.onClick.RemoveListener(OnButtonTwoClick);
        _buttonThree.onClick.RemoveListener(OnButtonThreeClick);
        _buttonFour.onClick.RemoveListener(OnButtonFourClick);
        _buttonFive.onClick.RemoveListener(OnButtonFiveClick);
        _buttonSix.onClick.RemoveListener(OnButtonSixClick);
        _buttonSeven.onClick.RemoveListener(OnButtonSevenClick);
        _buttonEight.onClick.RemoveListener(OnButtonEightClick);
        _buttonNine.onClick.RemoveListener(OnButtonNineClick);
        _buttonClear.onClick.RemoveListener(OnButtonClearClick);
        _buttonRemove.onClick.RemoveListener(OnButtonRemoveClick);
    }

    private void NumpadInputChanged()
    {
        string input = _inputBuffer.ToString();
        if (_currentSegment == null)
        {
            ResetNumpadInput();
            return;
        }

        if (! float.TryParse(input, out float distanceValue))
        {
            Debug.LogError($"Could not parse numpad input into float.");
            return;
        }

        _textSegmentDistance.text = input + "m";
        
        _segmentIndicators[_selectedSegmentID].SetState(true);

        _currentSegment.SelectedDistance = distanceValue;
        AssessmentManager.Instance.SetSegmentObjectiveDistance(_currentSegment.PathSegmentData.SegmentID, _currentSegment.SelectedDistance);
        _confirmButton.interactable = VerifyDistanceValues();
    }

    private void UpdateSelectedSegment()
    {
        ResetNumpadInput();
        _currentSegment = _segmentDistanceData[_selectedSegmentID];
        _textSelectedSegment.color = _currentSegment.PathSegmentData.SegmentColor;
        _textSelectedSegment.text = (_selectedSegmentID + 1).ToString();
        _selectionIndicator.color = _currentSegment.PathSegmentData.SegmentColor;

        if (_currentSegment.SelectedDistance == -1)
        {
            _textSegmentDistance.text = 0 + "m";
        }
        else
        {
            _textSegmentDistance.text = _currentSegment.SelectedDistance + "m";
        }

        _segmentIndicators[_selectedSegmentID].Toggle(true);
    }

    private bool VerifyDistanceValues()
    {
        foreach (var segmentData in _segmentDistanceData)
        {
            if (segmentData.SelectedDistance == -1)
                return false;
        }
        return true;
    }

    
    private void AddNumber(int number)
    {
        if (_isDecimal)
        {
            _inputBuffer.Append(number);
        }
        else
        {
            if (_inputBuffer.Length == 1 && _inputBuffer[0] == '0') 
            {
                _inputBuffer.Clear();
            }
            _inputBuffer.Append(number);
        }

        NumpadInputChanged();
    }

    private void AddDecimalPoint()
    {
        if (!_isDecimal && _inputBuffer.Length > 0)
        {
            _inputBuffer.Append(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            _isDecimal = true;
        }
        else if (_inputBuffer.Length == 0)
        {
            _inputBuffer.Append("0" + System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            _isDecimal = true;
        }

        NumpadInputChanged();
    }

    private void RemoveLastInput()
    {
        if (_inputBuffer.Length > 0)
        {
            string decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (_inputBuffer[_inputBuffer.Length - 1].ToString() == decimalSeparator)
            {
                _isDecimal = false;
            }
            
            _inputBuffer.Remove(_inputBuffer.Length - 1, 1);
            NumpadInputChanged();
        }
    }

    private void ResetNumpadInput()
    {
        _inputBuffer.Clear();
        _isDecimal = false;
        _selectionIndicator.color = Color.white;
    }

    public void ResetPanelData()
    {
        _currentSegment = null;
        _selectedSegmentID = -1;
        _segmentIndicators.ForEach(i => Destroy(i.gameObject));
        _segmentIndicators.Clear();
        _segmentDistanceData.Clear();
        _confirmButton.interactable = false;
    }

}
