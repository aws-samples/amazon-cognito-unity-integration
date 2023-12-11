using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoIdentity;

public class Cognito : MonoBehaviour
{
    
    private static CognitoUserPool userPool;    
    private static CognitoUser user;
    private static AuthFlowResponse authResponse;

    // Cognito Identity provider client
    private static AmazonCognitoIdentityProviderClient provider = new AmazonCognitoIdentityProviderClient
        (new Amazon.Runtime.AnonymousAWSCredentials(), ExternalParameters.region);
    
    public static async Task LoginWithFacebook()
    {   string access_token = PanelFacebookLogin.userAccessToken;
        Dictionary<string, string> Logins = new Dictionary<string, string>
            {
                { "graph.facebook.com", access_token }
            };
        try {
        CredentialsManager.credentials.AddLogin("graph.facebook.com", access_token);
        // get Identity ID from Cognito Identity Pool
        CredentialsManager.IdentityId = await CredentialsManager.credentials.GetIdentityIdAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }
        // Get Credentials for Identity
        try {

            var identityClient = new AmazonCognitoIdentityClient(CredentialsManager.credentials);
            GetCredentialsForIdentityResponse response = await identityClient.GetCredentialsForIdentityAsync(CredentialsManager.IdentityId, Logins);
            
            // Set Credentials for Unity
            CredentialsManager.AccessKeyId = response.Credentials.AccessKeyId;
            CredentialsManager.SecretKey = response.Credentials.SecretKey;
            CredentialsManager.SessionToken = response.Credentials.SessionToken;
            CredentialsManager.ExpireDate = response.Credentials.Expiration;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static async Task LoginUser(string username, string password)
    {
        CredentialsManager.Username = username;
        userPool = new CognitoUserPool(ExternalParameters.userPoolId, ExternalParameters.appClientId, provider);
        user = new CognitoUser(username, ExternalParameters.appClientId, userPool, provider);
        InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
        {
            Password = password
        };

        try
        {
            authResponse = await user.StartWithSrpAuthAsync(authRequest);
            Debug.Log("Auth Challenge name: " + authResponse.ChallengeName);
            // Check if MFA required
            if (authResponse.ChallengeName == ChallengeNameType.SMS_MFA)
            {
                MfaAuthFlow(username);
            }
            // no MFA required
            else if (authResponse.AuthenticationResult != null)
            {
                Debug.Log("Authentication Success");
                CredentialsManager.authType = "Cognito";
                updateCredentialsManager();
                // Show user profile
                EventManager.UserProfile(null);
            }
        }
        catch (Amazon.CognitoIdentityProvider.Model.NotAuthorizedException e)
        {
            Debug.LogError("Not Authorized: " + e);
            // Display error message
            EventManager.LoginMenu(e.Message);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotConfirmedException e)
        {
            Debug.LogError("User has not confirmed email address: " + e);
            // Ask user to confirm account
            ConfirmAccount(username, password);
        }
        catch (System.ArgumentNullException e)
        {
            Debug.LogError("Null value: " + e);
            // Display error message
            EventManager.LoginMenu(e.ParamName);
        }
        catch (Amazon.CognitoIdentityProvider.Model.InvalidParameterException e)
        {
            Debug.LogError("Invalid Parameter Exception: " + e);
            // Display error message
            EventManager.LoginMenu(e.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception: " + e);
            // Display error message
            EventManager.LoginMenu(e.Message);
        }
    }
    private static void MfaAuthFlow(string username)
    {
        // MFA required - Set MFA flag
        CredentialsManager.MFA = true; // Set MFA flag
        foreach (var attribute in authResponse.ChallengeParameters)
        {
            Debug.Log($"{attribute.Key}: {attribute.Value}");
        }
        // Call global verify menu
        EventManager.GlobalVerifyMenu("Code sent to:  " + authResponse.ChallengeParameters["CODE_DELIVERY_DESTINATION"]);

        // Listen for global verify menu events
        EventManager.SubscribeGlobalVerifyMenu(_globalVerifyResponse, _globalVerifyBack, _globalVerifyResend);

        void _globalVerifyBack()
        {
            EventManager.LoginMenu(null);
        }

        async void _globalVerifyResend()
        {
            EventManager.GlobalVerifyMenu("Resending MFA Code...");
            ResendConfirmationCodeRequest request = new ResendConfirmationCodeRequest
            {
                ClientId = ExternalParameters.appClientId,
                Username = username
            };
            try
            {
                ResendConfirmationCodeResponse response = await provider.ResendConfirmationCodeAsync(request);
                EventManager.GlobalVerifyMenu("Confirmation code sent to: " + response.CodeDeliveryDetails.Destination);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                string _msg = "Error sending confirmation code : " + e.Message;
                EventManager.GlobalVerifyMenu(_msg);
            }
        }

        void _globalVerifyResponse(string msg)
        {
            RespondToSmsMfaRequest mfaResponse = new RespondToSmsMfaRequest()
            {
                SessionID = authResponse.SessionID,
                MfaCode = msg
            };

            try
            {
                authResponse = user.RespondToSmsMfaAuthAsync(mfaResponse).Result;

                if (authResponse.AuthenticationResult != null)
                {
                    EventManager.GlobalVerifyMenu("MFA Auth Successful");
                    // EventManager.ClearGlobalVerifyMenuListeners();
                    CredentialsManager.authType = "Cognito";
                    updateCredentialsManager();
                    // Show user profile
                    EventManager.UserProfile(null);
                }
            }
            catch (Amazon.CognitoIdentityProvider.Model.CodeMismatchException e)
            {
                Debug.Log("MFA Auth failed:  " + e.Message);
                EventManager.GlobalVerifyMenu(e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError("MFA Auth Exception: " + e);
                EventManager.GlobalVerifyMenu(e.Message);
            }
        }
    }

    public static void ConfirmAccount(string username, string password)
    {
        EventManager.GlobalVerifyMenu("Please confirm account email");
        // Listen for global verify menu events
        EventManager.SubscribeGlobalVerifyMenu(_globalVerifyResponse, _globalVerifyBack, _globalVerifyResend);

        void _globalVerifyBack()
        {
            // EventManager.ClearGlobalVerifyMenuListeners();
            EventManager.LoginMenu(null);
        }

        async void _globalVerifyResend()
        {
            ResendConfirmationCodeRequest request = new ResendConfirmationCodeRequest
            {
                ClientId = ExternalParameters.appClientId,
                Username = username
            };
            try
            {
                ResendConfirmationCodeResponse response = await provider.ResendConfirmationCodeAsync(request);
                EventManager.GlobalVerifyMenu("Confirmation code sent to: " + response.CodeDeliveryDetails.Destination);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                string _msg = "Error sending confirmation code : " + e.Message;
                EventManager.GlobalVerifyMenu(_msg);
            }
        }

        async void _globalVerifyResponse(string msg)
        {
            ConfirmSignUpRequest request = new ConfirmSignUpRequest
            {
                ClientId = ExternalParameters.appClientId,
                ConfirmationCode = msg,
                Username = username
            };

            try
            {
                ConfirmSignUpResponse response = await provider.ConfirmSignUpAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // EventManager.ClearGlobalVerifyMenuListeners();
                    EventManager.LoginMenu("Account confirmed. Logging in...");
                    _ = LoginUser(username, password);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                string _msg = "Failed to confirm account: " + e.Message;
                EventManager.GlobalVerifyMenu(_msg);
            }
        }
    }

    public static async Task GetUserDetails()
    {
        Debug.Log("Getting user details");
        // get user details from user pool
        try
        {
            GetUserResponse userDetails = await provider.GetUserAsync(new GetUserRequest
                {
                    AccessToken = CredentialsManager.AccessToken
                    
                });

            // Get all user attributes
            var attributeValues = new Dictionary<string, string>();
            Debug.Log("User attributes are: ");
            foreach (var attribute in userDetails.UserAttributes)
            {
                Debug.Log($"{attribute.Name} : {attribute.Value}");
                attributeValues[attribute.Name] = attribute.Value;
            }
            // Update CredentialsManager with user attributes
            CredentialsManager.UpdateAttributes(attributeValues);

            // Exchange ID token for AWS credentials
            await CredentialsManager.Get_Credentials();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        // Print credentials info
        // Debug.Log("Access token: " + CredentialsManager.AccessToken);
        // Debug.Log("ID token: " + CredentialsManager.IdToken);
        // Debug.Log("Refresh token: " + CredentialsManager.RefreshToken);
        // Debug.Log("AccessKey: " + CredentialsManager.AccessKeyId);
        // Debug.Log("SecretKey: " + CredentialsManager.SecretKey);
        // Debug.Log("Session Token: " + CredentialsManager.SessionToken);
        // Debug.Log("Expires Date: " + CredentialsManager.ExpireDate);
    }

    public static async Task CreateAccount(string username, string password, string email)
    {
        SignUpRequest request = new SignUpRequest
        {
            ClientId = ExternalParameters.appClientId,
            Password = password,
            Username = username,
            UserAttributes = new List<AttributeType>
        {
            new AttributeType { Name = "email", Value = email }
        }
        };

        try
        {
            SignUpResponse response = await provider.SignUpAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                ConfirmAccount(username, password);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            string _msg = "Error creating account: " + e.Message;
            EventManager.SignUpMenu(_msg);
        }
    }

    public static async Task ResetPasswordConfirmationCodeRequest(string username)
    {
        ForgotPasswordRequest request = new ForgotPasswordRequest
        {
            ClientId = ExternalParameters.appClientId,
            Username = username
        };

        try
        {
            ForgotPasswordResponse response = await provider.ForgotPasswordAsync(request);
            string _msg = "Password reset code sent to: " + response.CodeDeliveryDetails.Destination;
            EventManager.ResetPasswordMenu(_msg);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            string _msg = "Error sending password reset code: " + e.Message;
            EventManager.ResetPasswordMenu(_msg);
        }
    }

    public static async Task ResetPassword(string username, string password, string code)
    {
        ConfirmForgotPasswordRequest request = new ConfirmForgotPasswordRequest
        {
            ClientId = ExternalParameters.appClientId,
            Username = username,
            ConfirmationCode = code,
            Password = password
        };

        try
        {
            ConfirmForgotPasswordResponse response = await provider.ConfirmForgotPasswordAsync(request);
            EventManager.LoginMenu("Password reset successfully");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            string msg = "Error resetting password: " + e.Message;
            EventManager.ResetPasswordMenu(msg);
        }
    }

    public static async Task UpdateUserAttributes()
    {
        try
        {
            var attributes = new List<AttributeType>();

            if (!string.IsNullOrEmpty(CredentialsManager.Name))
                attributes.Add(new AttributeType { Name = "name", Value = CredentialsManager.Name });

            if (!string.IsNullOrEmpty(CredentialsManager.Family_name))
                attributes.Add(new AttributeType { Name = "family_name", Value = CredentialsManager.Family_name });

            // email and phone REQUIRE verification and as such are seprate functions
            if (attributes.Count > 0)
            {
                UpdateUserAttributesResponse response = await provider.UpdateUserAttributesAsync(new UpdateUserAttributesRequest
                {
                    AccessToken = CredentialsManager.AccessToken,
                    UserAttributes = attributes
                });
                Debug.Log("UpdateUserAttributesResponse: " + response.HttpStatusCode);
                EventManager.UserProfile("Attributes Updated");
            }
            else
            {
                EventManager.UserProfile("No attribute changes");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            string _msg = "Error updating user attributes: " + e.Message;
            EventManager.UserProfile(_msg);
        }
    }

    public static async Task UpdateUserAttributePhone()
    {
        try
        {
            var attributes = new List<AttributeType>();

            if (CredentialsManager.Phone_number != null)
                attributes.Add(new AttributeType { Name = "phone_number", Value = CredentialsManager.Phone_number });

            UpdateUserAttributesResponse response = await provider.UpdateUserAttributesAsync(new UpdateUserAttributesRequest
            {
                AccessToken = CredentialsManager.AccessToken,
                UserAttributes = attributes
            });
            Debug.Log("Cognito Request UpdateUserAttribute Phone: " + response.HttpStatusCode);
            EventManager.GlobalVerifyMenu("Confirmation " + response.CodeDeliveryDetailsList[0].DeliveryMedium + " sent");

            // Listen for global verify menu events
            EventManager.SubscribeGlobalVerifyMenu(_globalVerifyResponse, _globalVerifyBack, _globalVerifyResend);

            void _globalVerifyBack()
            {
                // EventManager.ClearGlobalVerifyMenuListeners();
                EventManager.UserProfile(null);
            }

            async void _globalVerifyResend()
            {
                // resend confirmation code
                try
                {
                    UpdateUserAttributesResponse resendResponse = await provider.UpdateUserAttributesAsync(new UpdateUserAttributesRequest
                    {
                        AccessToken = CredentialsManager.AccessToken,
                        UserAttributes = attributes
                    });
                    Debug.Log("UpdateUserAttribute Phone: " + resendResponse.HttpStatusCode);
                    EventManager.GlobalVerifyMenu("Confirmation sent to " + resendResponse.CodeDeliveryDetailsList[0].Destination);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    string _msg = "Error resending confirmation code: " + e.Message;
                    EventManager.GlobalVerifyMenu(_msg);
                }
            }

            async void _globalVerifyResponse(string msg)
            {
                try
                {
                    VerifyUserAttributeResponse response = await provider.VerifyUserAttributeAsync(new VerifyUserAttributeRequest
                    {
                        AccessToken = CredentialsManager.AccessToken,
                        AttributeName = "phone_number",
                        Code = msg
                    });
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // EventManager.ClearGlobalVerifyMenuListeners();
                        Debug.Log("VerifyUserAttribute: " + response.HttpStatusCode);
                        EventManager.UserProfile("Phone number confirmed");
                    }
                }
                catch (Amazon.CognitoIdentityProvider.Model.CodeMismatchException e)
                {
                    Debug.LogError(e);
                    string _msg = "Error verifying phone number: " + e.Message;
                    EventManager.GlobalVerifyMenu(_msg);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    string _msg = "Error verifying phone number: " + e.Message;
                    EventManager.GlobalVerifyMenu(_msg);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            string msg = "Error updating phone: " + e.Message;
            EventManager.UserProfile(msg);
        }
    }

    public static async Task UpdateUserAttributeEMail()
    {
        try
        {
            var attributes = new List<AttributeType>();

            if (CredentialsManager.Email != null)
                attributes.Add(new AttributeType { Name = "email", Value = CredentialsManager.Email });

            UpdateUserAttributesResponse response = await provider.UpdateUserAttributesAsync(new UpdateUserAttributesRequest
            {
                AccessToken = CredentialsManager.AccessToken,
                UserAttributes = attributes
            });
            EventManager.GlobalVerifyMenu("Confirmation " + response.CodeDeliveryDetailsList[0].DeliveryMedium + " sent");

            // Listen for global verify menu events
            EventManager.SubscribeGlobalVerifyMenu(_globalVerifyResponse, _globalVerifyBack, _globalVerifyResend);

            void _globalVerifyBack()
            {
                EventManager.UserProfile(null);
            }
            
            async void _globalVerifyResend()
            {
                UpdateUserAttributesResponse response = await provider.UpdateUserAttributesAsync(new UpdateUserAttributesRequest
                {
                    AccessToken = CredentialsManager.AccessToken,
                    UserAttributes = attributes
                });
                EventManager.GlobalVerifyMenu("Confirmation " + response.CodeDeliveryDetailsList[0].Destination + " sent");
            }

            async void _globalVerifyResponse(string msg)
            {
                try
                {
                    VerifyUserAttributeResponse response = await provider.VerifyUserAttributeAsync(new VerifyUserAttributeRequest
                    {
                        AccessToken = CredentialsManager.AccessToken,
                        AttributeName = "email",
                        Code = msg
                    });
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // EventManager.ClearGlobalVerifyMenuListeners();
                        Debug.Log("VerifyUserAttribute: " + response.HttpStatusCode);
                        EventManager.UserProfile("Email confirmed");
                    }
                }
                catch (Amazon.CognitoIdentityProvider.Model.CodeMismatchException e)
                {
                    Debug.LogError(e);
                    string _msg = "Error: " + e.Message;
                    EventManager.GlobalVerifyMenu(_msg);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    string _msg = "Error verifying email: " + e.Message;
                    EventManager.GlobalVerifyMenu(_msg);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            string _msg = "Error updating email: " + e.Message;
            EventManager.UserProfile(_msg);
        }
    }

    public static async Task MFAUpdate()
    {
        try
        {
            SetUserMFAPreferenceResponse response = await provider.SetUserMFAPreferenceAsync(new SetUserMFAPreferenceRequest
            {
                AccessToken = CredentialsManager.AccessToken,
                SMSMfaSettings = new SMSMfaSettingsType
                {
                    Enabled = CredentialsManager.MFA
                }
            });
            Debug.Log("MFAUpdate: " + response.HttpStatusCode);
            EventManager.UserProfile("MFA updated");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static async Task RefreshSessionToken()
    {
        user.SessionTokens = new CognitoUserSession(
            CredentialsManager.IdToken,
            CredentialsManager.AccessToken,
            CredentialsManager.RefreshToken,
            DateTime.Now,
            DateTime.Now.AddDays(30));

        try
        {
            authResponse = await user.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
            {
                AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
            }).ConfigureAwait(false);

            // Refresh token successful - Update CredentialsManager
            CredentialsManager.AccessToken = authResponse.AuthenticationResult.AccessToken;
            CredentialsManager.IdToken = authResponse.AuthenticationResult.IdToken;
            CredentialsManager.RefreshToken = authResponse.AuthenticationResult.RefreshToken;
            CredentialsManager.ExpiresIn = authResponse.AuthenticationResult.ExpiresIn;

            // Exchange ID token for AWS credentials
            await CredentialsManager.Get_Credentials();

            // Print credentials info
            Debug.Log("Access token: " + CredentialsManager.AccessToken);
            Debug.Log("ID token: " + CredentialsManager.IdToken);
            Debug.Log("Refresh token: " + CredentialsManager.RefreshToken);
            Debug.Log("Expires Date: " + CredentialsManager.ExpireDate);
        }
        catch (Exception e)
        {
            EventManager.LoginMenu("Logged out: " + e.Message);
        }
    }

    public static async Task Logout()
    {
        try
        {
            CredentialsManager.ClearCredentials();
            await user.GlobalSignOutAsync();
            authResponse = null;
            EventManager.LoginMenu("Logout successful");
        }
        catch (Exception e)
        {
            string _msg = "error:  " + e.Message;
            EventManager.LoginMenu(_msg);
        }
    }
    
    private static void updateCredentialsManager()
    {
        // Update Credentials Manager with user details
        CredentialsManager.AccessToken = authResponse.AuthenticationResult.AccessToken;
        CredentialsManager.IdToken = authResponse.AuthenticationResult.IdToken;
        CredentialsManager.RefreshToken = authResponse.AuthenticationResult.RefreshToken;
        CredentialsManager.ExpiresIn = authResponse.AuthenticationResult.ExpiresIn;
    }
}
