using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelResetPassword : MonoBehaviour
{
    public TMP_InputField resetPasswordUsernameInput;
    public TMP_InputField resetPasswordConfirmationCodeInput;
    public Button resetPasswordConfirmationCodeRequestButton;
    public TMP_InputField resetPasswordNewPasswordInput;
    public TMP_InputField resetPasswordNewPasswordVerifyInput;
    [SerializeField] public Text resetPasswordAlertMessage;
    public Button resetPasswordSubmitButton;
    public Button resetPasswordBackButton;

    string username;
    string password;
    private void OnEnable () {
        // subscribe to Reset Password Menu event
        EventManager.ResetPasswordMenuView += ResetPasswordMenuView;
        // call function using last value
        ResetPasswordMenuView(EventManager.lastValue);

        // Setup listeners for this panel
        resetPasswordConfirmationCodeRequestButton.onClick.AddListener(ResetPasswordConfirmationCodeRequestSubmit);
        resetPasswordSubmitButton.onClick.AddListener(ResetPasswordSubmit);
        resetPasswordBackButton.onClick.AddListener(() => EventManager.LoginMenu(null));
    }
    private void OnDisable () {
        // Remove listeners for this panel
        resetPasswordConfirmationCodeRequestButton.onClick.RemoveListener(ResetPasswordConfirmationCodeRequestSubmit);
        resetPasswordSubmitButton.onClick.RemoveListener(ResetPasswordSubmit);
        resetPasswordBackButton.onClick.RemoveListener(() => EventManager.LoginMenu(null));

        // Unsubscribe to Reset Password Menu event
        EventManager.ResetPasswordMenuView -= ResetPasswordMenuView;
    }

    void ResetPasswordMenuView(string msg)
    {
        resetPasswordUsernameInput.text = username;
        resetPasswordAlertMessage.text = msg;
    }

        void ResetPasswordConfirmationCodeRequestSubmit()
    {
        resetPasswordAlertMessage.text = "Requesting new code...";
        username = resetPasswordUsernameInput.text;
        _ = Cognito.ResetPasswordConfirmationCodeRequest(username);
    }

    void ResetPasswordSubmit()
    {
        if (resetPasswordNewPasswordInput.text != resetPasswordNewPasswordVerifyInput.text)
        {
            resetPasswordAlertMessage.text = "Passwords do not match";
            return;
        }
        resetPasswordAlertMessage.text = "Resetting Password...";
        string code = resetPasswordConfirmationCodeInput.text;
        username = resetPasswordUsernameInput.text;
        password = resetPasswordNewPasswordInput.text;
        _ = Cognito.ResetPassword(username, password, code);
        resetPasswordNewPasswordInput.text = ""; // clear password input fields
        resetPasswordNewPasswordVerifyInput.text = ""; // clear password input fields
    }
}
