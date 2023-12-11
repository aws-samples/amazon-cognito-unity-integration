namespace Amazon.XR.Keyboard
{
    public class BackspaceKey : Key
    {
        protected override void OnPress()
        {
            Keyboard.BackspacePress();
        }
    }
}