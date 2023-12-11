using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PanelGlobalVerify : MonoBehaviour
{
    public TMP_InputField globalVerifyCodeInput;
    [SerializeField] public Text globalVerifyAlertMessage;
    public Button globalVerifySubmitButton;
    public Button globalVerifyResendButton;
    public Button globalVerifyBackButton;
    private void OnEnable() {
        // subscribe to Global Verify Menu View event
        EventManager.GlobalVerifyMenuView += GlobalVerifyMenuView;
        // call function using last value
        GlobalVerifyMenuView(EventManager.lastValue);

        // Setup listeners for this panel
        globalVerifySubmitButton.onClick.AddListener(GlobalVerifySubmit);
        globalVerifyResendButton.onClick.AddListener(() => EventManager.GlobalVerifyMenuResend());
        globalVerifyBackButton.onClick.AddListener(() => EventManager.GlobalVerifyMenuBack());
    }
    private void OnDisable() {
        // Remove listeners for this panel
        globalVerifySubmitButton.onClick.RemoveListener(GlobalVerifySubmit);
        globalVerifyResendButton.onClick.RemoveListener(() => EventManager.GlobalVerifyMenuResend());
        globalVerifyBackButton.onClick.RemoveListener(() => EventManager.GlobalVerifyMenuBack());
        // Remove all listeners for this panel
        ClearGlobalVerifyMenuListeners();
        // Unsubscription to Global Verify Menu View event
        EventManager.GlobalVerifyMenuView -= GlobalVerifyMenuView;
    }

    void GlobalVerifyMenuView(string msg)
    {
        // Clear code input field of previous attempt
        globalVerifyCodeInput.text = "";
        globalVerifyAlertMessage.text = msg;
    }
        void GlobalVerifySubmit()
    {
        string msg = globalVerifyCodeInput.text;
        Debug.Log("Global verify submit");
        EventManager.GlobalVerifyMenuResponse(msg);
    }

    // Remove subscription to submit button, back button and resend button
    static void ClearGlobalVerifyMenuListeners()
    {
        ClearGlobalVerifyMenuResponseView();
        ClearGlobalVerifyMenuBackView();
        ClearGlobalVerifyMenuResendView();
    }
    
    static void ClearGlobalVerifyMenuResponseView()
    {
        foreach (var listener in EventManager.GlobalVerifyMenuResponseViewSubscriptions)
        {
            EventManager.GlobalVerifyMenuResponseView -= listener;
        }
        EventManager.GlobalVerifyMenuResponseViewSubscriptions.Clear();
    }

    static void ClearGlobalVerifyMenuBackView()
    {
        foreach (var listener in EventManager.GlobalVerifyMenuBackViewSubscriptions)
        {
            EventManager.GlobalVerifyMenuBackView -= listener;
        }
        EventManager.GlobalVerifyMenuBackViewSubscriptions.Clear();
    }

    static void ClearGlobalVerifyMenuResendView()
    {
        foreach (var listener in EventManager.GlobalVerifyMenuResendViewSubscriptions)
        {
            EventManager.GlobalVerifyMenuResendView -= listener;
        }
        EventManager.GlobalVerifyMenuResponseViewSubscriptions.Clear();
    }
}
