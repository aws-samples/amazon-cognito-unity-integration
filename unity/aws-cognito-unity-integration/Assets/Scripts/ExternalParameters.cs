using System;
using Amazon;

public class ExternalParameters
{   
    // Facebook Variables are required only if you are using Facebook
    public static string FacebookAppId = ""; // eg. 987654321012345
    public static string FacebookClientToken = ""; // eg. 1234567890abcdefg0987654321
    
    // AWS variables
    public static RegionEndpoint region = RegionEndpoint.USEast1; // change this if you are in a different region
    public static Uri ApiGatewayEndpoint = new Uri("https://xxxxxx.execute-api.us-east-1.amazonaws.com"); // eg. https://123abcxyz.execute-api.us-east-1.amazonaws.com (do not put PATH)

    // Amazon Cognito Variables
    public static string identityPool = ""; // eg. us-east-1:000000-aaaa-bbbb-cccc-12345667890
    public static string userPoolId = ""; // eg. us-east-1_abcd1234
    public static string appClientId = ""; // eg. 1a2b3c4d5e6g7h8i9j

    // Amazon S3 variables
    public static string S3BucketName = ""; // eg. ...unitycognitobucket...
    public static string S3ObjectKeyPrefix = "cognito/aws-cognito-unity-integration/"; // eg. 'cognito/aws-cognito-unity-integration/' Update here requires update to IAM role

    // Amazon DynamoDB variables
    public static string DynamoDBTableName = ""; // eg. ...UnityCognitoDB....
}
