using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelUserProfile : MonoBehaviour
{
    public Dropdown userProfileTeamName;
    public TMP_InputField userProfileFirstName;
    public TMP_InputField userProfileLastName;
    public TMP_InputField userProfileEMailInput;
    public GameObject userProfileEMailVerifyButton;
    public GameObject userProfileEMailVerified;
    public TMP_InputField userProfilePhoneInput;
    public GameObject userProfilePhoneVerifyButton;
    public GameObject userProfilePhoneVerified;
    public Toggle userProfileMFAToggle;
    [SerializeField] public Text userProfileAlertMessage;
    public Button userProfileUpdateButton;
    public Button userProfileLogoutButton;
    public Button userProfileAPIGatewayIAMButton;
    [SerializeField] public Text userProfileAPIGatewayIAMAlertMessage;
    public Button userProfileAPIGatewayJWTButton;
    [SerializeField] public Text userProfileAPIGatewayJWTAlertMessage;
    public GameObject userProfileOverlay;
    public Button userProfileOverlayButton;
    
    private void OnEnable() {
        // subscribe to User Profile event
        EventManager.UserProfileView += UserProfileView;
        // Setup listeners for this panel
        userProfileFirstName.onValueChanged.AddListener(val => CredentialsManager.Name = val);
        userProfileLastName.onValueChanged.AddListener(val => CredentialsManager.Family_name = val);
        userProfileTeamName.onValueChanged.AddListener(val => TeamNameDropdown(val));
        userProfilePhoneVerifyButton.GetComponent<Button>().onClick.AddListener(PhoneVerifySubmit);
        userProfileEMailVerifyButton.GetComponent<Button>().onClick.AddListener(EmailVerifySubmit);
        userProfileMFAToggle.onValueChanged.AddListener(val => MFAToggle(val));
        userProfileEMailInput.onValueChanged.AddListener(val => VerifyEMailCheck(val));
        userProfilePhoneInput.onValueChanged.AddListener(val => VerifyPhoneCheck(val));
        userProfileUpdateButton.onClick.AddListener(UpdateUserAttributesSubmit);
        userProfileLogoutButton.onClick.AddListener(LogoutSubmit);
        userProfileAPIGatewayIAMButton.onClick.AddListener(APIGatewayIAM);
        userProfileAPIGatewayJWTButton.onClick.AddListener(APIGatewayJWT);
        userProfileOverlayButton.onClick.AddListener(Overlay);
        // call function using last value needs to be called after listeners are setup
        UserProfileView(EventManager.lastValue);
    }

    private void OnDisable() {
        // Remove listeners for this panel
        userProfileFirstName.onValueChanged.RemoveListener(val => CredentialsManager.Name = val);
        userProfileLastName.onValueChanged.RemoveListener(val => CredentialsManager.Family_name = val);
        userProfileTeamName.onValueChanged.RemoveListener(val => TeamNameDropdown(val));
        userProfilePhoneVerifyButton.GetComponent<Button>().onClick.RemoveListener(PhoneVerifySubmit);
        userProfileEMailVerifyButton.GetComponent<Button>().onClick.RemoveListener(EmailVerifySubmit);
        userProfileMFAToggle.onValueChanged.RemoveListener(val => MFAToggle(val));
        userProfileEMailInput.onValueChanged.RemoveListener(val => VerifyEMailCheck(val));
        userProfilePhoneInput.onValueChanged.RemoveListener(val => VerifyPhoneCheck(val));
        userProfileUpdateButton.onClick.RemoveListener(UpdateUserAttributesSubmit);
        userProfileLogoutButton.onClick.RemoveListener(LogoutSubmit);
        userProfileAPIGatewayIAMButton.onClick.RemoveListener(APIGatewayIAM);
        userProfileAPIGatewayJWTButton.onClick.RemoveListener(APIGatewayJWT);
        userProfileOverlayButton.onClick.RemoveListener(Overlay);
        // Unsubscribe to User Profile event
        EventManager.UserProfileView -= UserProfileView;
    }
    private void Overlay()
    {
        if (userProfileOverlay.activeSelf)
        {
            userProfileOverlay.SetActive(false);
        }
        else
        {
            userProfileOverlay.SetActive(true);
        }
    }

    async void UserProfileView(string msg)
    {
        // refresh user attributes
        if (CredentialsManager.authType == "Cognito")
        {
            await Cognito.GetUserDetails();
            // Update user attributes
            userProfileFirstName.GetComponent<TMP_InputField>().text = CredentialsManager.Name;
            userProfileLastName.GetComponent<TMP_InputField>().text = CredentialsManager.Family_name;
            userProfileEMailInput.GetComponent<TMP_InputField>().text = CredentialsManager.Email;
            userProfilePhoneInput.GetComponent<TMP_InputField>().text = CredentialsManager.Phone_number;
            userProfileMFAToggle.isOn = CredentialsManager.MFA;
        }
        else if (CredentialsManager.authType == "Facebook")
        {
            await Cognito.LoginWithFacebook(); // exchange Facebook token for AWS token
             // Update user attributes
            userProfileFirstName.GetComponent<TMP_InputField>().text = PanelFacebookLogin.userData.name;
            userProfileLastName.GetComponent<TMP_InputField>().text = "N/A";
            userProfileEMailInput.GetComponent<TMP_InputField>().text = PanelFacebookLogin.userData.email;
            userProfilePhoneInput.GetComponent<TMP_InputField>().text = "N/A";
            userProfileMFAToggle.isOn = CredentialsManager.MFA;
        }
        
        // Show user profile screen with the message
        userProfileAlertMessage.text = msg;

        // Call AWS Services to fill data from DynamoDB
        await UserProfileHelper.DynamoDBGetItem();
        userProfileTeamName.value = int.Parse(CredentialsManager.TeamName);

        // Check if email address is verified
        userProfileEMailVerified.SetActive(CredentialsManager.Email_verified);
        userProfileEMailVerifyButton.SetActive(!CredentialsManager.Email_verified);

        // Check if phone number is verified
        userProfilePhoneVerified.SetActive(CredentialsManager.Phone_number_verified);
        userProfilePhoneVerifyButton.SetActive(!CredentialsManager.Phone_number_verified);
    }

    void PhoneVerifySubmit()
    {
        // remove common special character from phone number
        string number = userProfilePhoneInput.text; 
        number = number.Replace("-", ""); 
        number = number.Replace("(", ""); 
        number = number.Replace(")", ""); 
        number = number.Replace(" ", ""); 
        
        // check if phone number is 10 digits and add +1 if so
        if(number.Length == 10) 
        {
            number = "+1" + number; 
        } 
        // check if phone number is 11 digits and add + if so
        else if (number.Length == 11) 
        {
            number = "+" + number; 
        }
        if(number.Length != 12) 
        {
            userProfileAlertMessage.text = "Invalid Phone Number"; 
            return; 
        }
    
        CredentialsManager.Phone_number = number;
        CredentialsManager.MFA = false;
        CredentialsManager.Phone_number_verified = false;
        _ = Cognito.UpdateUserAttributePhone();
    }

    void EmailVerifySubmit()
    {
        CredentialsManager.Email = userProfileEMailInput.text;
        CredentialsManager.Email_verified = false;
        _ = Cognito.UpdateUserAttributeEMail();
    }
    void VerifyEMailCheck(string val)
    {
        if (val != CredentialsManager.Email)
        {
            userProfileEMailVerified.SetActive(false);
            userProfileEMailVerifyButton.SetActive(true);
        }
        if (val == CredentialsManager.Email && CredentialsManager.Email_verified)
        {
            userProfileEMailVerified.SetActive(true);
            userProfileEMailVerifyButton.SetActive(false);
        }
    }

    void VerifyPhoneCheck(string val)
    {
        if (val != CredentialsManager.Phone_number)
        {
            userProfilePhoneVerified.SetActive(false);
            userProfilePhoneVerifyButton.SetActive(true);
            userProfileMFAToggle.GetComponentInChildren<Text>().text = "MFA - verified phone number required";
            userProfileMFAToggle.isOn = false;
        }
        if (val == CredentialsManager.Phone_number && CredentialsManager.Phone_number_verified)
        {
            userProfilePhoneVerified.SetActive(true);
            userProfilePhoneVerifyButton.SetActive(false);
            userProfileMFAToggle.GetComponentInChildren<Text>().text = "MFA";
            userProfileMFAToggle.isOn = CredentialsManager.MFA;
        }
    }

    void UpdateUserAttributesSubmit()
    {
        _ = Cognito.UpdateUserAttributes();
        if (CredentialsManager.MFA != userProfileMFAToggle.isOn)
        {   
            Debug.Log("MFA update needed");
            CredentialsManager.MFA = userProfileMFAToggle.isOn;
            _ = Cognito.MFAUpdate();
        }
    }

    void TeamNameDropdown(int val)
    {
        if (val != int.Parse(CredentialsManager.TeamName))
        {
            _ = UserProfileHelper.DynamoDBPutItem(userProfileTeamName.value);
            EventManager.UserProfile("Team Name updated");
        }
    }

    void MFAToggle(bool val)
    {
        if (CredentialsManager.Phone_number_verified && (userProfilePhoneInput.text == CredentialsManager.Phone_number))
        {
            // do nothing
        }
        else
        {
            userProfileMFAToggle.isOn = false;
        }
    }

    void LogoutSubmit()
    {
        _ = Cognito.Logout();
        EventManager.LoginMenu(null);
    }
    async void APIGatewayIAM()
    {
        string _requestPath = "v1/iam";
        string _response = await UserProfileHelper.APIGetewayIAMGetRequest(_requestPath);
        userProfileAPIGatewayIAMAlertMessage.text = _response;
    }
    async void APIGatewayJWT()
    {
        string _requestPath = "v1/jwt";
        string _response = await UserProfileHelper.APIGetewayJWTGetRequest(_requestPath);
        userProfileAPIGatewayJWTAlertMessage.text = _response;
    }

}
