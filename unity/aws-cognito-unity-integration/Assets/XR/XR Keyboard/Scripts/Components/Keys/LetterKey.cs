using UnityEngine;
using System.Collections.Generic;

namespace Amazon.XR.Keyboard
{
    public class LetterKey : Key
    {
        [SerializeField] protected KeyCode _numericKey;
        [SerializeField] protected KeyCode _symbolicKey;


        protected override void Awake()
        {
            base.Awake();

            Keyboard.OnShiftPress += HandleShiftPress;
            Keyboard.OnNumlockPress += HandleNumlockPress;
        }

        private void SetUpperValue(KeyCode key)
        {
            _currentValue = new KeyValuePair<KeyCode, string>(key, Utils.GetKeyValue(key).ToUpper());
        }

        protected override void SetDisplayText(ShiftState state)
        {
            base.SetDisplayText(state);

            switch (state)
            {
                case ShiftState.LOWER:
                    SetValue(_primaryKey);
                    break;
                case ShiftState.UPPER:
                    SetUpperValue(_primaryKey);
                    break;
                case ShiftState.NUMERIC:
                    SetValue(_numericKey);
                    break;
                case ShiftState.SYMBOLIC:
                    SetValue(_symbolicKey);
                    break;
            }

            _text.text = _currentValue.Value;
        }

        protected override void OnPress()
        {
            Keyboard.AddText(_currentValue.Value);
        }

        private void HandleShiftPress(ShiftState state)
        {
            SetDisplayText(state);
        }

        private void HandleNumlockPress(ShiftState state)
        {
            SetDisplayText(state);
        }
    }
}