using System;
using UnityEngine;
using System.Collections.Generic;
using Amazon.CognitoIdentity;
using System.Threading.Tasks;

public class CredentialsManager
{   
    // Cognito Attributes
    public static string Userid;
    public static string Email;
    public static bool Email_verified;
    public static string Name;
    public static string Family_name;
    public static string Phone_number;
    public static bool Phone_number_verified;
    public static bool MFA;

    // User Pool Tokens
    public static string Username;
    public static string IdToken;
    public static string AccessToken;
    public static string RefreshToken;
    public static int? ExpiresIn;
    
    // IAM credentials
    public static string AccessKeyId;
    public static string SecretKey;
    public static string SessionToken;
    public static System.DateTime? ExpireDate;
    public static string IdentityId;

    // Authenticaiton type
    public static string authType;

    // DynamoDB Data
    public static string TeamName;

    // Initialize the Amazon Cognito credentials provider
    public static CognitoAWSCredentials credentials = new CognitoAWSCredentials(
        ExternalParameters.identityPool, ExternalParameters.region
    );

    public static void ClearCredentials()
    {
        Userid = null;
        Username = null;
        AccessKeyId = null;
        SecretKey = null;
        SessionToken = null;
        IdToken = null;
        AccessToken = null;
        RefreshToken = null;
        ExpiresIn = null;
        Email = null;
        ExpireDate = null;
        IdentityId = null;
    }

    public static void UpdateAttributes(Dictionary<string, string> attributes)
    {
        Userid = attributes.GetValueOrDefault("sub");
        Email = attributes.GetValueOrDefault("email");
        Email_verified = Convert.ToBoolean(attributes.GetValueOrDefault("email_verified"));
        Name = attributes.GetValueOrDefault("name");
        Family_name = attributes.GetValueOrDefault("family_name");
        Phone_number = attributes.GetValueOrDefault("phone_number");
        Phone_number_verified = Convert.ToBoolean(attributes.GetValueOrDefault("phone_number_verified"));
    }

    public static async Task Get_Credentials()
    {
        var identityClient = new AmazonCognitoIdentityClient(credentials, ExternalParameters.region);

        var idRequest = new Amazon.CognitoIdentity.Model.GetIdRequest();
        idRequest.IdentityPoolId = ExternalParameters.identityPool;
        idRequest.Logins = new Dictionary<string, string> { { "cognito-idp." + ExternalParameters.region.SystemName + ".amazonaws.com/" + ExternalParameters.userPoolId, IdToken } };

        // Get identity id
        var idResponseId = await identityClient.GetIdAsync(idRequest).ConfigureAwait(false);
        if (idResponseId.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            Debug.Log(String.Format("Failed to get credentials for identity. Status code: {0} ", idResponseId.HttpStatusCode));
        }

        // Get credentials for the identity id
        var idResponseCredential = await identityClient.GetCredentialsForIdentityAsync(idResponseId.IdentityId, idRequest.Logins).ConfigureAwait(false);
        if (idResponseCredential.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            Debug.Log(String.Format("Failed to get credentials for identity. Status code: {0} ", idResponseCredential.HttpStatusCode));
        }
        AccessKeyId = idResponseCredential.Credentials.AccessKeyId;
        SecretKey = idResponseCredential.Credentials.SecretKey;
        SessionToken = idResponseCredential.Credentials.SessionToken;
        ExpireDate = idResponseCredential.Credentials.Expiration;
        IdentityId = idResponseId.IdentityId;
        authType = "Cognito";
    }
}
