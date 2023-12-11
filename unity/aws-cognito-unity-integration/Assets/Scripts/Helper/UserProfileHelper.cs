using System;
using System.Threading.Tasks;
using UnityEngine;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using System.Net.Http;
using System.IO;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;

public class UserProfileHelper : MonoBehaviour
{
    public static async Task<byte[]> S3DownloadProfilePicture(string objectKey)
    {
        // Create S3 client with credentials
        AmazonS3Client s3Client = new AmazonS3Client(
            CredentialsManager.AccessKeyId,
            CredentialsManager.SecretKey,
            CredentialsManager.SessionToken,
            ExternalParameters.region
        );

        // Get object from S3
        using (GetObjectResponse response = await s3Client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = ExternalParameters.S3BucketName,
            Key = objectKey
        }))
        
        // Check if response stream is not null
        if (response.ResponseStream != null)
        {
            // Copy response stream to memory stream
            using (var memory = new MemoryStream())
            {
                Debug.Log("Copying response stream to memory stream...");
                await response.ResponseStream.CopyToAsync(memory);
                byte[] data = memory.ToArray();
                return data;
            }
        }
        else
        {
            Debug.LogError("Profile image download failed. Unable to get object from S3://" + ExternalParameters.S3BucketName + "/" + objectKey);
            return null;
        }
    }

    // This method uploads a profile picture to an AWS S3 bucket
    public static async void S3UploadProfilePicture(string localFilePath, string S3ObjectKey)
    {
        // Create an Amazon S3 client 
        AmazonS3Client s3Client = new AmazonS3Client(
            CredentialsManager.AccessKeyId,
            CredentialsManager.SecretKey,
            CredentialsManager.SessionToken,
            ExternalParameters.region
        );

        // PUT object to S3
        Debug.Log("s3 bucket name: " + ExternalParameters.S3BucketName);
        Debug.Log("s3 key: " + S3ObjectKey);
        Debug.Log("s3 FilePath: " + localFilePath);

        try
        {
            PutObjectResponse response = await s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = ExternalParameters.S3BucketName,
                Key = S3ObjectKey,
                FilePath = localFilePath
            });
            Debug.Log("S3 Put object response: " + response.HttpStatusCode);
        }
        catch (Exception e)
        {
            Debug.LogError("S3 Put object response: " + e.Message);
        }
    }

    public static async Task DynamoDBGetItem()
    {
        // Create DynamoDB client with credentials
        AmazonDynamoDBClient dynamoDBClient = new AmazonDynamoDBClient(
            CredentialsManager.AccessKeyId,
            CredentialsManager.SecretKey,
            CredentialsManager.SessionToken,
            ExternalParameters.region
        );

        // Build request for item
        var request = new GetItemRequest
        {
            TableName = ExternalParameters.DynamoDBTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"id", new AttributeValue {S = CredentialsManager.IdentityId}}
            }
        };

        try
        {
            // Get item from DynamoDB
            var response = await dynamoDBClient.GetItemAsync(request);
            string team = response.Item["team"].N;
            CredentialsManager.TeamName = team;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.LogError("Team Name Not Found - Setting Team 0");
            CredentialsManager.TeamName = "0";
        }
    }

   public static async Task DynamoDBPutItem(int? team = null)
    {
        // Create DynamoDB client with credentials
        AmazonDynamoDBClient dynamoDBClient = new AmazonDynamoDBClient(
            CredentialsManager.AccessKeyId,
            CredentialsManager.SecretKey,
            CredentialsManager.SessionToken,
            ExternalParameters.region
        );

        // Build request for item
        var request = new PutItemRequest
        {
            TableName = ExternalParameters.DynamoDBTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                {"id", new AttributeValue {S = CredentialsManager.IdentityId}},
                {"team", new AttributeValue {N = team?.ToString()}}
            }
        };

        try
        {
            // Put item in DynamoDB
            await dynamoDBClient.PutItemAsync(request);
            Debug.Log("Team name updated");
            CredentialsManager.TeamName = team?.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }


    public static async Task<string> APIGetewayIAMGetRequest(string path)
    {
        // Build request URI
        Uri requestUri = new Uri(ExternalParameters.ApiGatewayEndpoint + path);

        // Create request message with GET method
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = requestUri
        };

        // Sign the request
        SigV4Signer signer = new SigV4Signer();
        request = await signer.Sign(request, "execute-api");

        // Send the request
        using (var client = new HttpClient())
        {
            HttpResponseMessage response = await client.SendAsync(request);
            string responseStr = await response.Content.ReadAsStringAsync();
            Debug.Log("API Gateway (IAM) response: " + responseStr);
            return responseStr;
        }
    }

    public static async Task<string> APIGetewayJWTGetRequest(string path)
    {
        // Build request URI
        Uri requestUri = new Uri(ExternalParameters.ApiGatewayEndpoint + path);

        // Create request message with GET method
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = requestUri
        };

        // Add authorizer-token header
        request.Headers.Add("authorizer-token", CredentialsManager.IdToken);

        // Send the request
        using (var client = new HttpClient())
        {
            HttpResponseMessage response = await client.SendAsync(request);
            string responseStr = await response.Content.ReadAsStringAsync();
            Debug.Log("API Gateway (JWT) response: " + responseStr);
            return responseStr;
        }
    }
}
