using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
// using Newtonsoft.Json;

public class PanelFacebookLogin : MonoBehaviour
{
    [SerializeField] public Text loginFacebookAlertMessage;
    [SerializeField] public Text loginFacebookCodeDisplayCode;
    public GameObject panelFacebookLoginCodeDisplay;
    public Button loginFacebookCodeBackButton;
    public GameObject panelFacebookLoginSuccess;
    public Image panelFacebookLoginSuccessProfilePicture;
    [SerializeField] public Text panelFacebookLoginSuccessMessage;
    public Button loginFacebookSuccessContinueButton;

    public static string userAccessToken;
    public static string scope = "public_profile,email";
    public static string url = "https://graph.facebook.com/v2.6/device/login";
    public static string url_status = "https://graph.facebook.com/v2.6/device/login_status";
    public static string url_me = "https://graph.facebook.com/v2.3/me";
    public static FacebookDeviceVerificationCode code;  
    public static FacebookPollResult codePollResult;
public static string accessToken = ExternalParameters.FacebookAppId + "|" + ExternalParameters.FacebookClientToken;
    public static FacebookUserData userData;
    private void OnEnable()
    {
        loginFacebookCodeBackButton.onClick.AddListener(() => EventManager.LoginMenu(null));
        loginFacebookSuccessContinueButton.onClick.AddListener(() => EventManager.UserProfile(null));
        _ = GetCode();

        // subscribe to Login Menu Facebook View event
        EventManager.LoginMenuFacebookView += LoginMenuFacebookView;
        // call function using last value
        LoginMenuFacebookView(EventManager.lastValue);
        
    }
    private void OnDisable() {
        // Remove listeners for this panel
        loginFacebookCodeBackButton.onClick.RemoveListener(() => EventManager.LoginMenu(null));
        loginFacebookSuccessContinueButton.onClick.RemoveListener(() => EventManager.UserProfile(null));
        loginFacebookCodeDisplayCode.text = null;
        loginFacebookAlertMessage.text = null;
        // unsubscribe to Login Menu Facebook View event
        EventManager.LoginMenuFacebookView -= LoginMenuFacebookView;
    }
    void LoginMenuFacebookView(string msg)
    {
        loginFacebookCodeDisplayCode.text = msg;
        panelFacebookLoginCodeDisplay.SetActive(true);
        panelFacebookLoginSuccess.SetActive(false);        
    }
    public void LoginMenuFacebookSuccessView(string msg)
    {
        panelFacebookLoginCodeDisplay.SetActive(false);
        panelFacebookLoginSuccess.SetActive(true);
        string name = PanelFacebookLogin.userData.name;
        panelFacebookLoginSuccessMessage.text = $"{name},\nYou are now logged in.";
        CredentialsManager.authType = "Facebook";
    }
    public async Task GetCode()
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("access_token", accessToken),
            new KeyValuePair<string, string>("scope", scope)
        });

        using (var client = new HttpClient())
        {
            var response = await client.PostAsync(url, form);

            if (!response.IsSuccessStatusCode)
            {
                string _msg = "Error: " + response.ReasonPhrase;
                EventManager.LoginMenu(_msg);
            }
            else
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                code = JsonUtility.FromJson<FacebookDeviceVerificationCode>(jsonResponse);
                // EventManager.LoginMenuFacebook(code.user_code);
                loginFacebookCodeDisplayCode.text = code.user_code;
                panelFacebookLoginCodeDisplay.SetActive(true);
                _ = PollForAccessToken();
            }
        }
    }
    public async ValueTask PollForAccessToken()
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("access_token", accessToken),
            new KeyValuePair<string, string>("code", code.code)
        });

        using (var client = new HttpClient())
        {
            var response = await client.PostAsync(url_status, form);

            if (!response.IsSuccessStatusCode)
            {
                string _msg = "Error: " + response.ReasonPhrase;
                EventManager.LoginMenu(_msg);
            }
            else
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                // codePollResult = JsonConvert.DeserializeObject<FacebookPollResult>(jsonResponse);
                codePollResult = null;
                if (codePollResult.access_token != null )
                {
                    userAccessToken = codePollResult.access_token;
                    _ = GetUserData();
                }
                else if (codePollResult.error != null)
                {
                    Error error = codePollResult.error;
                    
                    if (error.error_subcode == 1349174)
                    {
                        Debug.Log("pending user action");
                        await Task.Delay((int)code.interval * 1000);
                        await PollForAccessToken();
                    }
                    else if (error.error_subcode == 1349172)
                    {
                        Debug.Log("polling too frequently");
                        await Task.Delay((int)code.interval * 1000);
                        await PollForAccessToken();
                    }
                    else if (error.error_subcode == 1349152)
                    {
                        Debug.Log("Device Code Expired");
                        EventManager.LoginMenu("Facebook device Code Expired");
                    }
                }
                else 
                {
                    Debug.Log("Something went wrong!");
                    EventManager.LoginMenu("Something went wrong!");
                }
            }
        }
    }

    public async Task GetUserData()
    {
        string fields = "name,picture,email";
        string getUrl = url_me + "?fields=" + fields + "&access_token=" + codePollResult.access_token;

        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(getUrl);

            if (!response.IsSuccessStatusCode)
            {
                string _msg = "Error: " + response.ReasonPhrase;
                EventManager.LoginMenu(_msg);
            }
            else
            {
                Debug.Log("*****GetUserData*****");
                string jsonResponse = await response.Content.ReadAsStringAsync();
                // parse json response
                // userData = JsonConvert.DeserializeObject<FacebookUserData>(jsonResponse);
                userData = null;
                string name = userData.name;
                string picture = userData.picture.data.url;
                string email = userData.email;
                Debug.Log("Name: " + name + ", Picture: " + picture + ", Email: " + email);
                DownloadProfilePicture("Panel Facebook Login/Success/User Picture");
                LoginMenuFacebookSuccessView(null);
            }
        }
    }

    public static void DownloadProfilePicture(string gameObjectName)
    {
        ImageDownloader imgDownloader = FindObjectOfType<ImageDownloader>();
        string _imageUrl = userData.picture.data.url;
        imgDownloader.StartCoroutine(imgDownloader.DownloadImage(gameObjectName, _imageUrl));
    }
}
