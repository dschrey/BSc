using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Class is deprecated and will be removed in the future.", true)]
public class UINumpadInputHandler : MonoBehaviour
{    
    public delegate void OnInputChanged(string newInput);
    public event OnInputChanged InputChangedEvent;
    public TMP_Text SelectionIndicator;
    
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

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _buttonComma.onClick.AddListener(HandleButtonCommaClick);
        _buttonZero.onClick.AddListener(HandleButtonZeroClick);
        _buttonOne.onClick.AddListener(HandleButtonOneClick);
        _buttonTwo.onClick.AddListener(HandleButtonTwoClick);
        _buttonThree.onClick.AddListener(HandleButtonThreeClick);
        _buttonFour.onClick.AddListener(HandleButtonFourClick);
        _buttonFive.onClick.AddListener(HandleButtonFiveClick);
        _buttonSix.onClick.AddListener(HandleButtonSixClick);
        _buttonSeven.onClick.AddListener(HandleButtonSevenClick);
        _buttonEight.onClick.AddListener(HandleButtonEightClick);
        _buttonNine.onClick.AddListener(HandleButtonNineClick);
        _buttonClear.onClick.AddListener(HandleButtonClearClick);
        _buttonRemove.onClick.AddListener(HandleButtonRemoveClick);
    }

    private void OnDisable() 
    {

        _buttonComma.onClick.RemoveListener(HandleButtonCommaClick);
        _buttonZero.onClick.RemoveListener(HandleButtonZeroClick);
        _buttonOne.onClick.RemoveListener(HandleButtonOneClick);
        _buttonTwo.onClick.RemoveListener(HandleButtonTwoClick);
        _buttonThree.onClick.RemoveListener(HandleButtonThreeClick);
        _buttonFour.onClick.RemoveListener(HandleButtonFourClick);
        _buttonFive.onClick.RemoveListener(HandleButtonFiveClick);
        _buttonSix.onClick.RemoveListener(HandleButtonSixClick);
        _buttonSeven.onClick.RemoveListener(HandleButtonSevenClick);
        _buttonEight.onClick.RemoveListener(HandleButtonEightClick);
        _buttonNine.onClick.RemoveListener(HandleButtonNineClick);
        _buttonClear.onClick.RemoveListener(HandleButtonClearClick);
        _buttonRemove.onClick.RemoveListener(HandleButtonRemoveClick);
    }
 
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void HandleButtonZeroClick()
    {
        AddNumber(0);
    }

    private void HandleButtonOneClick()
    {
        AddNumber(1);
    }

    private void HandleButtonTwoClick()
    {
        AddNumber(2);
    }

    private void HandleButtonThreeClick()
    {
        AddNumber(3);
    }

    private void HandleButtonFourClick()
    {
        AddNumber(4);
    }

    private void HandleButtonFiveClick()
    {
        AddNumber(5);
    }

    private void HandleButtonSixClick()
    {
        AddNumber(6);
    }

    private void HandleButtonSevenClick()
    {
        AddNumber(7);
    }

    private void HandleButtonEightClick()
    {
        AddNumber(8);
    }

    private void HandleButtonNineClick()
    {
        AddNumber(9);
    }

    private void HandleButtonCommaClick()
    {
        AddDecimalPoint();
    }

    private void HandleButtonClearClick()
    {
        ClearInput();
    }

    private void HandleButtonRemoveClick()
    {
        RemoveLastInput();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------


    public void AddNumber(int number)
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

        NotifyInputChanged();
    }

    public void AddDecimalPoint()
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

        NotifyInputChanged();
    }

    public void RemoveLastInput()
    {
        if (_inputBuffer.Length > 0)
        {
            string decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (_inputBuffer[_inputBuffer.Length - 1].ToString() == decimalSeparator)
            {
                _isDecimal = false;
            }
            _inputBuffer.Remove(_inputBuffer.Length - 1, 1);
            NotifyInputChanged();
        }
    }

    public void ClearInput()
    {
        _inputBuffer.Clear();
        _isDecimal = false;
        AddNumber(0);
    }

    public void ResetInput()
    {
        _inputBuffer.Clear();
        _isDecimal = false;
        SelectionIndicator.color = Color.white;
    }

    private void NotifyInputChanged()
    {
        InputChangedEvent?.Invoke(_inputBuffer.ToString());
    }

}
