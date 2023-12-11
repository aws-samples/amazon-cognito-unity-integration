
# Amazon Cognito & Unity Integration
Take the undifferentiated heavy lifting out of your Unity project with this sample app, delivering an authorization and authentication layer, seamless integration with your AWS backend services, and much more for your upcoming project.

## Overview

This project demonstrates the seamless integration of Unity with AWS services, showcasing the utilization of Cognito User Pool and Identity Pool for secure JWT token-based authentication. The application uses AWS Identity and Access Management (IAM) to interact with API Gateway, Lambda functions, S3, and DynamoDB.

## Project Structure

The project is organized into two main components:

1. **Backend Services (/backend):**
    - Written in .NET using AWS Cloud Development Kit (CDK).
    - Deployed using AWS CDK, creating Cognito User Pool and Identity Pool, API Gateway, Lambda functions, S3, and DynamoDB.
    - Conditions set for IAM permissions, ensuring DynamoDB and S3 access is restricted to the user's identity.

2. **Unity Application (/unity/aws-cognito-unity-integration):**
    - Unity project showcasing AWS integration.
    - The input for the script, which holds external parameters, is derived from the output of the CDK and is located at the specified path: `/unity/aws-cognito-unity-integration/Assets/Scripts/ExternalParameters.cs`.
    - You are required to download AWS SDK DLLs to be added to `/unity/aws-cognito-unity-integration/Assets/Plugins` folder.

## Prerequisites
### 1. Download the Required .NET DLLs

When using the AWS SDK for .NET and .NET Standard 2.0 for your Unity application, your application must reference the AWS SDK for .NET assemblies (DLL files) directly rather than using NuGet. For more information see

- [Special considerations for Unity support](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/unity-special.html)
- [Obtaining assemblies for the AWS SDK for .NET](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-obtain-assemblies.html#download-zip-files)

You can opt to obtain the .dll from nuget.org by downloading the corresponding package. After successful download, rename the file extension from .nupkg to .zip, extract the DLL from the designated path located in lib/netstandard2.0.

Make sure to add the following DLLs to the `/unity/aws-cognito-unity-integration/Assets/Plugins` folder in your Unity project:

- Amazon.Extensions.CognitoAuthentication.dll
- AWSSDK.APIGateway.dll
- AWSSDK.CognitoIdentity.dll
- AWSSDK.CognitoIdentityProvider.dll
- AWSSDK.Core.dll
- AWSSDK.DynamoDBv2.dll
- AWSSDK.S3.dll
- AWSSDK.SecurityToken.dll
- Microsoft.Bcl.AsyncInterfaces.dll
- System.Runtime.CompilerServices.Unsafe.dll


### 2. AWS Account

Prerequisites:

- AWS account.
- SES configured for sending email. (User creation requires email verification)
- Local environment with this reposotory and AWS CLI.
- Optional: AWS Pinpoint and SNS configured for SMS MFA verification.

## Getting Started

1. **Backend Deployment:**
    - Navigate to the `/backend` folder.
    - Deploy CDK using `dotnet build src` followed by `cdk deploy`.
    - Retrieve output variables: `ApiGatewayEndpoint`, `IdentityPool`, `UserPoolId`,  `AppClientId`, `S3BucketName`, `DynamoDBTableName` for next step.

2. **Unity Configuration:**
    - Include the necessary DLLs in the `/unity/aws-cognito-unity-integration/Assets/Plugins` folder.
    - Modify the `ExternalParameters.cs` file by incorporating the output variables obtained from the CDK deployment. The file is located at `/unity/aws-cognito-unity-integration/Assets/Scripts/ExternalParameters.cs`.
    - Utilize Unity Hub to open the project located in `/unity/aws-cognito-unity-integration`. (The project has been tested on version 2022.3.12f1.)
    - Open your Unity Editor and load the sample scene by navigating to `Assets/Scenes/SampleScene.unity.`

3. **Optional MFA Setup:**
    - If using SMS MFA, setup SNS and AWS Pinpoint for sending SMS messages. (This Unity application supports sending to 10+1 digit phone number. Modification may be necessary to support other countries or specific requirements.)

## Usage

1. **User Account Creation:**
    - Sign up in the Unity application.
    - Verify email address (Requires SES for email verification).

2. **Sign In:**
    - After email confirmation, you will automatically be logged into your profile page. 
    - MFA verification will require you to input SMS code during every signin event. 

3. **Other Features:**
    - To reset your password, you need to request a verification code via email before submitting a new password.
    -  When signing up, you must verify your email address as part of the registration process.
    - If you want to change your email address, you must verify the new address before it can be updated.
    - Enabling Multi-Factor Authentication (MFA) is only possible after verifying your phone number through SMS.
    - Changing your phone number or email address requires re-verification.
    - Passwords must meet certain criteria (minimum length of 12, include lowercase and uppercase letters, digits, and a special character).

## Beyond Sample App

-  As you add/remove .DLLs, be sure to update link.xml file located in `/unity/aws-cognito-unity-integration/Assets/link.xml`. This file helps Unity understand which parts to use.

- The _ThirdParty folder contains XR Interaction Toolkit files. It's advisable to update these files to a version that supports or matches the Unity version you are running.

## Security Considerations

When implementing the Unity AWS integration, it's essential to prioritize security considerations. Here are some key points to keep in mind:

### API Gateway Proxy Recommendation:

- It is recommended to utilize API Gateway as a proxy before accessing AWS services. This provides an additional layer of security and control over the communication between Unity and AWS services.

### Direct Access to AWS Services:

- While services such as S3 and DynamoDB can be accessed directly and securely using the appropriate IAM roles, caution should be exercised.
- Limit access permissions to the minimum required for your Unity application, and thoroughly understand the scope of permissions granted.

### IAM Role Limitations:

- Ensure IAM roles associated with your Unity application have only the necessary permissions and are not overly permissive.
- Regularly review and update IAM roles to align with the principle of least privilege.

### Network Security:

- Implement secure communication channels between Unity and AWS services. Use secure protocols and consider encryption where applicable.
- Regularly review network configurations and access controls to identify and mitigate potential vulnerabilities.

### Logging and Monitoring:

- Enable logging and monitoring for both Unity and AWS services to detect and respond to any suspicious activities promptly.
- Regularly review logs and monitor AWS CloudWatch metrics to maintain visibility into the system's security posture.

### Regular Audits:

- Conduct regular security audits to identify and address potential vulnerabilities in the Unity application and associated AWS resources.
- Stay informed about security best practices and updates from both Unity and AWS, applying patches and updates promptly.

### User Authentication and Authorization:

- Implement robust user authentication mechanisms, such as Cognito User Pool, and maintain proper authorization to access AWS resources.
- Regularly review user accounts and permissions to prevent unauthorized access.

These considerations are meant to guide you in creating a secure integration between Unity and AWS. Regularly reassess and update security measures to adapt to evolving threats and maintain ongoing integrity of your application.

## Cleanup

To efficiently tear down the resources created by this integration, run the command `cdk destroy` in the `/backend` directory:

Note: If you wish to retain specific resources and prevent them from being deleted during cleanup, you should first update the CDK file located at `backend/src/CdkDotnet/CdkDotnetStack.cs` Locate the resources you want to retain and update their RemovalPolicy to RETAIN. This will result in the persistence of these resources even after running the cleanup command.

Please exercise caution when modifying the RemovalPolicy, as retaining resources unnecessarily may lead to unwanted costs or resource accumulation. Always review and validate your changes before applying them.

## Contribution
We welcome contributions from the community to help improve and enhance this open-source Unity VR project. If you would like to contribute, please follow these guidelines:

Fork the repository and make your changes in a new branch.
Submit a pull request with a detailed description of your changes.
Your changes will be reviewed, and if approved, will be merged into the main branch.

## License
This sample code is made available under the MIT-0 license. See the LICENSE file.
