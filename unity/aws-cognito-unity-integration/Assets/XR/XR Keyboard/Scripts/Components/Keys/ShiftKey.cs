namespace Amazon.XR.Keyboard
{
    public class ShiftKey : Key
    {
        protected override void Awake()
        {
            base.Awake();
            Keyboard.OnNumlockPress += HandleNumlockPress;
        }

        protected override void OnPress()
        {
            Keyboard.ShiftPress();
        }

        private void HandleNumlockPress(ShiftState state)
        {
            SetDisplayText(state);
        }

        protected override void SetDisplayText(ShiftState state)
        {
            switch (state)
            {
                case ShiftState.NUMERIC:
                    _text.text = k_symbolicDisplay;
                    break;
                case ShiftState.SYMBOLIC:
                    _text.text = k_numericDisplay;
                    break;
                default:
                    _text.text = _currentValue.Value;
                    break;
            }
        }
    }
}