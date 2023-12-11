namespace Amazon.XR.Keyboard
{
    public class SpaceKey : Key
    {
        protected override void OnPress()
        {
            Keyboard.AddText(" ");
        }
    }
}