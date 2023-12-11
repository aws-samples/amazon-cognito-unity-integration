namespace Amazon.XR.Keyboard
{
    public class NumlockKey : Key
    {
        protected override void OnPress()
        {
            Keyboard.NumlockPress();
            _text.text = (_text.text == _currentValue.Value) ? k_upperDisplay : k_numericDisplay;
        }
    }
}