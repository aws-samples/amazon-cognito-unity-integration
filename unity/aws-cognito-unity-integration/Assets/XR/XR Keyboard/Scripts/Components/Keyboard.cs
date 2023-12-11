using UnityEngine;

namespace Amazon.XR.Keyboard
{
    public class Keyboard : MonoBehaviour
    {
        public static System.Action<string> OnTextUpdate;
        public static System.Action<ShiftState> OnShiftPress;
        public static System.Action<ShiftState> OnNumlockPress;

        private static string _inputValue;
        public static string InputValue
        {
            get { return _inputValue; }
        }

        private static ShiftState _state;

        private void Awake()
        {
            if (this.gameObject.activeSelf)
            {
                Deactivate();
            }
        }

        public void Activate()
        {
            _inputValue = "";
            this.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            this.gameObject.SetActive(false);
            _inputValue = "";
        }

        public static void AddText(string value)
        {
            _inputValue += value;
            OnTextUpdate?.Invoke(_inputValue);
        }

        public static void ShiftPress()
        {
            switch (_state)
            {
                case ShiftState.LOWER:
                    _state = ShiftState.UPPER;
                    break;
                case ShiftState.UPPER:
                    _state = ShiftState.LOWER;
                    break;
                case ShiftState.NUMERIC:
                    _state = ShiftState.SYMBOLIC;
                    break;
                case ShiftState.SYMBOLIC:
                    _state = ShiftState.NUMERIC;
                    break;
            }
            OnShiftPress?.Invoke(_state);
        }

        public static void NumlockPress()
        {
            switch (_state)
            {
                case ShiftState.LOWER:
                case ShiftState.UPPER:
                    _state = ShiftState.NUMERIC;
                    break;
                case ShiftState.NUMERIC:
                case ShiftState.SYMBOLIC:
                    _state = ShiftState.LOWER;
                    break;
            }
            OnNumlockPress?.Invoke(_state);
        }

        public static void BackspacePress()
        {
            if (string.IsNullOrEmpty(_inputValue))
                return;

            _inputValue = _inputValue.Remove(_inputValue.Length - 1);
            OnTextUpdate?.Invoke(_inputValue);
        }

        public static void ClearPress()
        {
            _inputValue = "";
            OnTextUpdate?.Invoke(_inputValue);
        }
    }
}