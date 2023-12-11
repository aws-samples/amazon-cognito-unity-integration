using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelLogin : MonoBehaviour
{
    // Login Menu
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public Button loginPasswordShowHideButton;
    public GameObject passwordShowText;
    public GameObject passwordHideText;
    [SerializeField] public Text loginAlertMessage;
    public Button loginLoginButton;
    public Button loginSignUpButton;
    public Button loginForgotPasswordButton;
    public Button loginLoginWithFacebookButton;
    private bool isPasswordVisible = false;
    private void OnEnable() {
        // subscribe to Login Menu View event
        EventManager.LoginMenuView += LoginMenuView;
        // call function using last value
        LoginMenuView(EventManager.lastValue);
        // show hide password
        loginPasswordShowHideButton.onClick.AddListener(PasswordShowHide);
        // Setup listeners for this panel
        loginSignUpButton.onClick.AddListener(() => EventManager.SignUpMenu(null));
        loginLoginButton.onClick.AddListener(LoginSubmit);
        loginForgotPasswordButton.onClick.AddListener(() => EventManager.ResetPasswordMenu(null));
        loginLoginWithFacebookButton.onClick.AddListener(() => EventManager.LoginMenuFacebook(null));
    }
    private void OnDisable() {
        // Remove listeners for this panel
        loginPasswordShowHideButton.onClick.RemoveListener(PasswordShowHide);
        loginSignUpButton.onClick.RemoveListener(() => EventManager.SignUpMenu(null));
        loginLoginButton.onClick.RemoveListener(LoginSubmit);
        loginForgotPasswordButton.onClick.RemoveListener(() => EventManager.ResetPasswordMenu(null));
        loginLoginWithFacebookButton.onClick.RemoveListener(() => EventManager.LoginMenuFacebook(null));
        // Unsubscribe to Login Menu View event
        EventManager.LoginMenuView -= LoginMenuView;
    }

    void PasswordShowHide()
    {
        // show or hide password
        isPasswordVisible = !isPasswordVisible;
        passwordShowText.SetActive(!isPasswordVisible);
        passwordHideText.SetActive(isPasswordVisible);
        loginPasswordInput.contentType = isPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

        // Force the input field to refresh its visual representation
        string currentText = loginPasswordInput.text;
        loginPasswordInput.text = "";
        loginPasswordInput.text = currentText;
    }

    void LoginSubmit()
    {
        loginAlertMessage.text = "Connecting...";
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;
        _ = Cognito.LoginUser(username, password);
        loginPasswordInput.text = ""; // clear password input fields
    }

    void LoginMenuView(string msg)
    {
        loginAlertMessage.text = msg;
    }

}
