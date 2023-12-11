using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// key
//// typing
////// letter
////// symbol
//// control

namespace Amazon.XR.Keyboard
{
    [RequireComponent(typeof(Button))]
    public class Key : MonoBehaviour
    {
        [SerializeField] protected KeyCode _primaryKey;
        protected Button _button;
        protected TMP_Text _text;

        protected const string k_upperDisplay = "ABC";
        protected const string k_numericDisplay = "123";
        protected const string k_symbolicDisplay = "#+=";

        protected KeyValuePair<KeyCode, string> _currentValue;
        public KeyValuePair<KeyCode, string> CurrentValue
        {
            get { return _currentValue; }
        }

        protected virtual void Awake()
        {
            _button = this.GetComponent<Button>();
            _text = this.GetComponentInChildren<TMP_Text>();
            _button.onClick.AddListener(OnPress);

            SetDisplayText();
        }

        protected virtual void SetValue(KeyCode key)
        {
            _currentValue = new KeyValuePair<KeyCode, string>(key, Utils.GetKeyValue(key));
        }

        protected virtual void SetDisplayText()
        {
            if (string.IsNullOrEmpty(_currentValue.Value))
            {
                SetValue(_primaryKey);
            }

            _text.text = _currentValue.Value;
        }

        protected virtual void SetDisplayText(ShiftState state) { }
        protected virtual void OnPress() { }
    }
}