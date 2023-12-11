using UnityEngine;

public class UIManager : MonoBehaviour

{
    // Menu Category
    public GameObject panelLogin;
    public GameObject panelFacebookLogin;
    public GameObject panelSignup;
    public GameObject panelResetPassword;
    public GameObject panelUserProfile;
    public GameObject panelGlobalVerify;
    public GameObject keyboard;

    void Start()
    {
        // Suscribe to events. Events are used to move between menus
        SubscribeToEvents();

        // Trigger login menu event
        EventManager.LoginMenu(null);
    }
    void SubscribeToEvents()
    {
        EventManager.LoginMenuView += LoginMenuView;
        EventManager.LoginMenuFacebookView += LoginMenuFacebookView;
        EventManager.SignupMenuView += SignUpMenuView;
        EventManager.ResetPasswordMenuView += ResetPasswordMenuView;
        EventManager.UserProfileView += UserProfileView;
        EventManager.GlobalVerifyMenuView += GlobalVerifyMenuView;
    }

    void DisableAllViews()
    {
        panelLogin.SetActive(false);
        panelFacebookLogin.SetActive(false);
        panelSignup.SetActive(false);
        panelUserProfile.SetActive(false);
        panelResetPassword.SetActive(false);
        panelGlobalVerify.SetActive(false);
        keyboard.SetActive(false);
    }

    void LoginMenuView(string msg)
    {
        // check if gameobject is active
        if (!panelLogin.activeSelf)
        {
            // if not active, enable it
            DisableAllViews();
            panelLogin.SetActive(true);
        }
    }

    void LoginMenuFacebookView(string msg)
    {
        // check if gameobject is active
        if (!panelFacebookLogin.activeSelf)
        {
            // if not active, enable it
            DisableAllViews();
            panelFacebookLogin.SetActive(true);
        }
    }
    
    void SignUpMenuView(string msg)
    {
        // check if gameobject is active
        if (!panelSignup.activeSelf)
        {
            // if not active, enable it
            DisableAllViews();
            panelSignup.SetActive(true);
        }
    }

    void ResetPasswordMenuView(string msg)
    {
        // check if gameobject is active
        if (!panelResetPassword.activeSelf)
        {
            // if not active, enable it
            DisableAllViews();
            panelResetPassword.SetActive(true);
        }
    }

    void UserProfileView(string msg)
    {
        // check if gameobject is active
        if (!panelUserProfile.activeSelf)
        {
            // if not active, enable it
            DisableAllViews();
            panelUserProfile.SetActive(true);
        }
    }

    void GlobalVerifyMenuView(string msg)
    {
        // check if gameobject is active
        if (!panelGlobalVerify.activeSelf)
        {
            // if not active, enable it
            DisableAllViews();
            panelGlobalVerify.SetActive(true);
        } 
    }
}