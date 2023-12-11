namespace Amazon.XR.Keyboard
{
    public class ClearKey : Key
    {
        protected override void OnPress()
        {
            Keyboard.ClearPress();
        }
    }
}