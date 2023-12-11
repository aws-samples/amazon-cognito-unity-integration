using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PanelSignUp : MonoBehaviour
{
    public TMP_InputField signUpEMailInput;
    public TMP_InputField signUpUsernameInput;
    public TMP_InputField signUpPasswordInput;
    [SerializeField] public Text signUpAlertMessage;
    public Button signUpSignUpButton;
    public Button signUpBackButton;
    string username;
    string password;
    string email;
    private void OnEnable() {
        // subscribe to Sign Up Menu event
        EventManager.SignupMenuView += SignUpMenuView;
        // call function using last value
        SignUpMenuView(EventManager.lastValue);

        // Setup listeners for this panel
        signUpBackButton.onClick.AddListener(() => EventManager.LoginMenu(null));
        signUpSignUpButton.onClick.AddListener(SignupSubmit);
    }
    private void OnDisable() {
        // Remove listeners for this panel
        signUpBackButton.onClick.RemoveListener(() => EventManager.LoginMenu(null));
        signUpSignUpButton.onClick.RemoveListener(SignupSubmit);
        // Unsubscribe to Sign Up Menu event
        EventManager.SignupMenuView += SignUpMenuView;
    }

    void SignUpMenuView(string msg)
    {
        signUpAlertMessage.text = msg;
        signUpUsernameInput.text = username;
        signUpEMailInput.text = email;
    }

    void SignupSubmit()
    {
        signUpAlertMessage.text = "Creating account...";
        username = signUpUsernameInput.text;
        password = signUpPasswordInput.text;
        email = signUpEMailInput.text;
        _ = Cognito.CreateAccount(username, password, email);
        signUpPasswordInput.text = ""; // clear password input fields
    }
}
