using System;
using System.Collections.Generic;

public class EventManager

{
    
    public static event Action<string> LoginMenuView;
    public static event Action<string> LoginMenuFacebookView;
    public static event Action<string> SignupMenuView;
    public static event Action<string> ResetPasswordMenuView;
    public static event Action<string> UserProfileView;
    // Used for passing last value to new subscribers
    public static string lastValue;
    // Global verify menu
    public static event Action<string> GlobalVerifyMenuView;
    // Global verify menu buttons 
    public static event Action<string> GlobalVerifyMenuResponseView;
    public static event Action GlobalVerifyMenuBackView;
    public static event Action GlobalVerifyMenuResendView;

    // Global verify menu - (submit, back, resend) buttons subscriptions
    // Subscription lists are created in order to remove subscriptions when panel is disabled
    public static List<Action<string>> GlobalVerifyMenuResponseViewSubscriptions = new List<Action<string>>();
    public static List<Action> GlobalVerifyMenuBackViewSubscriptions = new List<Action>();
    public static List<Action> GlobalVerifyMenuResendViewSubscriptions = new List<Action>();

    public static void LoginMenu(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => LoginMenuView?.Invoke(msg));
        lastValue = msg;
    }
    public static void LoginMenuFacebook(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => LoginMenuFacebookView?.Invoke(msg));
        lastValue = msg;
    }
    public static void SignUpMenu(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => SignupMenuView?.Invoke(msg));
        lastValue = msg;
    }
    public static void ResetPasswordMenu(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => ResetPasswordMenuView?.Invoke(msg));
        lastValue = msg;
    }
    public static void GlobalVerifyMenu(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => GlobalVerifyMenuView?.Invoke(msg));
        lastValue = msg;
    }
    public static void GlobalVerifyMenuResponse(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => GlobalVerifyMenuResponseView?.Invoke(msg));
        lastValue = msg;
    }
    public static void GlobalVerifyMenuResend()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => GlobalVerifyMenuResendView?.Invoke());
    }
    public static void GlobalVerifyMenuBack()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => GlobalVerifyMenuBackView?.Invoke());
    }
    public static void UserProfile(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => UserProfileView?.Invoke(msg));
        lastValue = msg;
    }

    // This function should be called every time Global Verify Panel is enabled
    public static void SubscribeGlobalVerifyMenu(Action<string> response, Action back, Action resend)
    {
        GlobalVerifyMenuResponseViewSubscriptions.Add(response);
        GlobalVerifyMenuResponseView += response;
        
        GlobalVerifyMenuBackViewSubscriptions.Add(back);
        GlobalVerifyMenuBackView += back;

        GlobalVerifyMenuResendViewSubscriptions.Add(resend);
        GlobalVerifyMenuResendView += resend;
    }
  
}
