using System;
using System.Collections.Generic;
using Constructs;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Cognito.IdentityPool.Alpha;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.CodePipeline;

namespace CdkDotnet
{
    public class CognitoUnityIntegration : Stack
    {
        internal CognitoUnityIntegration(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // create cognito user pool
            UserPool userPool = new UserPool(this, "User-Pool", new UserPoolProps {
                SelfSignUpEnabled = true,
                Mfa = Mfa.OPTIONAL,
                UserVerification = new UserVerificationConfig {
                    EmailStyle = VerificationEmailStyle.CODE,
                },
                PasswordPolicy = new PasswordPolicy {
                    MinLength = 12,
                    RequireLowercase = true,
                    RequireUppercase = true,
                    RequireDigits = true,
                    RequireSymbols = true,
                    TempPasswordValidity = Duration.Days(3),
                },
                AutoVerify = new AutoVerifiedAttrs {
                    Email = true,
                    Phone = true
                },
                SignInAliases = new SignInAliases {
                    Username = true,
                    Email = true
                },
                AccountRecovery = AccountRecovery.EMAIL_ONLY,
                RemovalPolicy = RemovalPolicy.DESTROY, // Remove the user pool when the stack is deleted - CHANGE THIS AS NEEDED
                AdvancedSecurityMode = AdvancedSecurityMode.ENFORCED
            }
            );

            // create cognito user pool client
            UserPoolClient userPoolClient = userPool.AddClient("Client", new UserPoolClientOptions {
                OAuth = new OAuthSettings {
                    Flows = new OAuthFlows {
                        ImplicitCodeGrant = true
                    },
                    Scopes = new [] { 
                        OAuthScope.OPENID,
                        OAuthScope.EMAIL,
                        OAuthScope.PHONE,
                        OAuthScope.PROFILE,
                        OAuthScope.COGNITO_ADMIN
                     },
                },
                AuthFlows = new AuthFlow {
                    UserPassword = true,
                    UserSrp = true,
                },
                SupportedIdentityProviders = new [] { UserPoolClientIdentityProvider.COGNITO },
            }
            );

            // Set unique domain
            string stackName = Stack.Of(this).StackName.ToLower();
            string accountNumber = Stack.Of(this).Account;
            string domainPrefix = $"{stackName}-{accountNumber}";

            // Create a custom domain for the Hosted UI
            UserPoolDomain userPoolDomain = new UserPoolDomain(this, "CognitoHostedUIDomain", new UserPoolDomainProps
            {
                UserPool = userPool,
                CognitoDomain = new CognitoDomainOptions {
                    DomainPrefix = domainPrefix
                }
            });

            // create cognito identity pool with user pool authentication provider
            IdentityPool identityPool = new IdentityPool(this, "Identity-Pool", new IdentityPoolProps {
                AllowUnauthenticatedIdentities = true,
                AuthenticationProviders = new IdentityPoolAuthenticationProviders {
                    UserPools = new [] { new UserPoolAuthenticationProvider(new UserPoolAuthenticationProviderProps { UserPool = userPool, UserPoolClient = userPoolClient }) }
                }
            });

            // Create Lambda function for GET request
            Function getFunction = new Function(this, "Get-Function", new FunctionProps
            {
                Runtime = Runtime.PYTHON_3_9,
                Handler = "index.lambda_handler",
                Code = Code.FromInline(@"import boto3
                
def lambda_handler(event, context):
    return {
        'statusCode': 200,
        'body': 'Hello from Lambda!'
    }")
            });

            // Create API Gateway REST API and /prod stage
            RestApi api = new RestApi(this, "CognitoUnityIntegration", new RestApiProps
            {
                Description = "API Gateway for Cognito Unity Integration",
                EndpointTypes = new[] { EndpointType.REGIONAL },
                Deploy = false // prevent '/prod' stage from deploying
            });

            // Deploy the API v1 stage
            Deployment deployment = new Deployment(this, "Deployment", new DeploymentProps
            {
                Api = api
            });

            // Create v1 stage and associate it with the deployment
            Amazon.CDK.AWS.APIGateway.Stage stage = new Amazon.CDK.AWS.APIGateway.Stage(this, "Stage", new Amazon.CDK.AWS.APIGateway.StageProps
            {
                Deployment = deployment,
                StageName = "v1"
            });

            api.DeploymentStage = stage;

            // Define API Model - Response
            var responseModel = new Model(this, "CustomResponseModel", new ModelProps
            {
                RestApi = api,
                ModelName = "CustomResponseModel",
                ContentType = "application/json",
                Schema = new JsonSchema
                {
                    Type = JsonSchemaType.OBJECT
                }
            });

            // Create IAM role for API Gateway execution
            Role apiGatewayRole = new Role(this, "ApiGatewayRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("apigateway.amazonaws.com")
            });

            // Attach policies to the API Gateway role for invoking Lambda function
            apiGatewayRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "lambda:InvokeFunction" },
                Resources = new[] { getFunction.FunctionArn }
            }));

            // Attach policies to the API Gateway role for reading/writing CloudWatch logs for Lambda function
            apiGatewayRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "logs:CreateLogGroup", "logs:CreateLogStream", "logs:PutLogEvents" },
                Resources = new[] { "arn:aws:logs:*:*:*" }  // Replace with your actual CloudWatch log group ARN
            }));

            // Create Cognito user pool authorizer
            var userPoolAuthorizer = new CognitoUserPoolsAuthorizer(this, "CognitoAuthorizer", new CognitoUserPoolsAuthorizerProps
            {
                CognitoUserPools = new[] { userPool },
                AuthorizerName = "CognitoAuthorizer",
                IdentitySource = "method.request.header.authorizer-token" // where to look for authorization token
            });

            // Create /iam resource
            Amazon.CDK.AWS.APIGateway.Resource iamResource = api.Root.AddResource("iam");

            // Add GET method to /iam resource which triggers lambda function
            iamResource.AddMethod(
                "GET", 
                new LambdaIntegration(getFunction, new LambdaIntegrationOptions
                {
                    Proxy = false,
                    PassthroughBehavior = PassthroughBehavior.WHEN_NO_TEMPLATES,
                    IntegrationResponses = new[]
                    {
                        new IntegrationResponse
                        {
                            StatusCode = "200",
                            ResponseTemplates = new Dictionary<string, string>
                            {
                                ["application/json"] = ""
                            }
                        }
                    },
                }), 
                new MethodOptions
                    {
                        AuthorizationType = AuthorizationType.IAM,
                        MethodResponses = new[]
                        {
                            new MethodResponse
                            {
                                StatusCode = "200",
                                ResponseModels = new Dictionary<string, IModel>
                                {
                                    ["application/json"] = (IModel)responseModel
                                }
                            }
                        }
                    }
            );

            // Create /jwt resource
            Amazon.CDK.AWS.APIGateway.Resource jwtResource = api.Root.AddResource("jwt");

            // Add GET method to /jwt resource without Lambda proxy integration
            jwtResource.AddMethod(
                "GET", 
                new LambdaIntegration(getFunction, new LambdaIntegrationOptions
                    {
                        Proxy = false,
                        PassthroughBehavior = PassthroughBehavior.WHEN_NO_TEMPLATES,
                        IntegrationResponses = new[]
                        {
                            new IntegrationResponse
                            {
                                StatusCode = "200",
                                ResponseTemplates = new Dictionary<string, string>
                                {
                                    ["application/json"] = ""
                                }
                            }
                        },
                    }), 
                new MethodOptions
                {
                    Authorizer = userPoolAuthorizer,
                    AuthorizationType = AuthorizationType.COGNITO,
                    MethodResponses = new[]
                    {
                        new MethodResponse
                        {
                            StatusCode = "200",
                            ResponseModels = new Dictionary<string, IModel>
                            {
                                ["application/json"] = (IModel)responseModel
                            }
                        }
                    }
                }
            );

            // add unauthenticated role policy statement
            identityPool.UnauthenticatedRole.AddToPrincipalPolicy(new PolicyStatement(new PolicyStatementProps 
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "mobileanalytics:PutEvents", "cognito-sync:*" },
                Resources = new[] { "*" },
            }));

            // add authenticated role policy statement to allow API Gateway access
            identityPool.AuthenticatedRole.AddToPrincipalPolicy(new PolicyStatement(new PolicyStatementProps 
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "execute-api:Invoke" },
                Resources = new[] { 
                    api.ArnForExecuteApi("GET", "/iam"), 
                    api.ArnForExecuteApi("GET", "/jwt") 
                },
            }));

            // Create DynamoDB table
            Table myDynamoDBTable = new Table(this, "UnityCognitoDB", new TableProps
            {
                RemovalPolicy = RemovalPolicy.DESTROY, // Remove the DynamoDB database when the stack is deleted - CHANGE THIS AS NEEDED
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                }
            });

            // add authenticated role policy statement to allow DynamoDB access
            identityPool.AuthenticatedRole.AddToPrincipalPolicy(new PolicyStatement(new PolicyStatementProps 
            {
                Effect = Effect.ALLOW,
                Actions = new[] {
                    "dynamodb:DeleteItem",
                    "dynamodb:GetItem",
                    "dynamodb:PutItem",
                    "dynamodb:Query",
                    "dynamodb:UpdateItem"
                },
                Resources = new[] { myDynamoDBTable.TableArn },
                Conditions = new Dictionary<string, object>
                {
                    {
                        "ForAllValues:StringEquals",
                        new Dictionary<string, object>
                        {
                            {
                                "dynamodb:LeadingKeys",
                                "${cognito-identity.amazonaws.com:sub}"
                            }
                        }
                    }
                }
            }));

            // Create S3 Bucket
            Bucket s3Bucket = new Bucket(this, "UnityCognitoBucket", new BucketProps
            {
                Versioned = true, // Enable versioning if needed
                RemovalPolicy = RemovalPolicy.DESTROY // Remove the S3 bucket when the stack is deleted - CHANGE THIS AS NEEDED
            });

            // IAM Policy Statement for ListBucket with prefix condition
            identityPool.AuthenticatedRole.AddToPrincipalPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "s3:ListBucket" },
                Resources = new[] { s3Bucket.BucketArn },
                Conditions = new Dictionary<string, object>
                {
                    { "StringLike", new Dictionary<string, object> { { "s3:prefix", new[] { $"cognito/aws-cognito-unity-integration/${{cognito-identity.amazonaws.com:sub}}/*" } } } }
                }
            }));

            // IAM Policy Statement for Read, Write, and Delete Object
            identityPool.AuthenticatedRole.AddToPrincipalPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "s3:GetObject", "s3:PutObject", "s3:DeleteObject" },
                Resources = new[] { $"{s3Bucket.BucketArn}/cognito/aws-cognito-unity-integration/${{cognito-identity.amazonaws.com:sub}}/*" }
            }));

            // Output the API endpoint
            new CfnOutput(this, "ApiGatewayEndpoint", new CfnOutputProps
            {
                Value = api.Url
            });

            // output Identity Pool id
            new CfnOutput(this, "IdentityPool", new CfnOutputProps {
                Value = identityPool.IdentityPoolId
            });

            // output user pool id
            new CfnOutput(this, "UserPoolId", new CfnOutputProps {
                Value = userPool.UserPoolId
            });

            // output user pool client id
            new CfnOutput(this, "AppClientId", new CfnOutputProps {
                Value = userPoolClient.UserPoolClientId
            });

            // Export the bucket name
            new CfnOutput(this, "S3BucketName", new CfnOutputProps{
                Value = s3Bucket.BucketName
            });

            // Output the DynamoDB table name
            new CfnOutput(this, "DynamoDBTableName", new CfnOutputProps
            {
                Value = myDynamoDBTable.TableName
            });            
        }
    }
}

