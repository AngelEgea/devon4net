using ADC.PostNL.BuildingBlocks.Common;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;

namespace ADC.PostNL.BuildingBlocks.AWS.Cognito.Handler
{
    public class AwsCognitoHandler : IAwsCognitoHandler
    {
        private readonly AmazonCognitoIdentityProviderClient AmazonCognitoIdentityClient;

        private bool _disposed = false;

        public AwsCognitoHandler()
        {
            AmazonCognitoIdentityClient = new AmazonCognitoIdentityProviderClient();
        }

        public AwsCognitoHandler(AWSCredentials awsCredentials)
        {
            AmazonCognitoIdentityClient = new AmazonCognitoIdentityProviderClient(awsCredentials);
        }

        public AwsCognitoHandler(RegionEndpoint awsRegion)
        {
            AmazonCognitoIdentityClient = new AmazonCognitoIdentityProviderClient(awsRegion);
        }

        public AwsCognitoHandler(AWSCredentials awsCredentials, RegionEndpoint awsRegion)
        {
            AmazonCognitoIdentityClient = new AmazonCognitoIdentityProviderClient(awsCredentials, awsRegion);
        }

        #region UserPool
        public Task<CreateUserPoolResponse> CreateUserPool(CreateUserPoolRequest userPoolRequest)
        {
            return AmazonCognitoIdentityClient.CreateUserPoolAsync(userPoolRequest);
        }

        public Task DeleteUserPool(string userPoolId)
        {
            return AmazonCognitoIdentityClient.DeleteUserPoolAsync(new DeleteUserPoolRequest
            {
                UserPoolId = userPoolId
            });
        }

        public Task<ListUserPoolsResponse> GetAllUserPools()
        {
            return AmazonCognitoIdentityClient.ListUserPoolsAsync(new ListUserPoolsRequest());
        }

        public Task<DescribeUserPoolResponse> GetUserPool(string userPoolId)
        {
            return AmazonCognitoIdentityClient.DescribeUserPoolAsync(new DescribeUserPoolRequest
            {
                UserPoolId = userPoolId
            });
        }
        #endregion

        #region AppClients
        public async Task<UserPoolClientType> CreateAppClient(CreateUserPoolClientRequest request)
        {
            var appClient = await AmazonCognitoIdentityClient.CreateUserPoolClientAsync(request).ConfigureAwait(false);

            return appClient.UserPoolClient;
        }

        public Task DeleteAppClient(string userPoolId, string appClientId)
        {
            return AmazonCognitoIdentityClient.DeleteUserPoolClientAsync(new DeleteUserPoolClientRequest
            {
                UserPoolId = userPoolId,
                ClientId = appClientId
            });
        }

        public async Task<IList<UserPoolClientDescription>> GetAllAppClients(string userPoolId)
        {
            var appClients = await AmazonCognitoIdentityClient.ListUserPoolClientsAsync(new ListUserPoolClientsRequest
            {
                UserPoolId = userPoolId
            }).ConfigureAwait(false);

            return appClients.UserPoolClients;
        }

        public async Task<UserPoolClientType> GetAppClient(string userPoolId, string appClientId)
        {
            try
            {
                var appClient = await AmazonCognitoIdentityClient.DescribeUserPoolClientAsync(new DescribeUserPoolClientRequest
                {
                    UserPoolId = userPoolId,
                    ClientId = appClientId
                }).ConfigureAwait(false);

                return appClient.UserPoolClient;
            }
            catch (ResourceNotFoundException ex)
            {
                PostNLLogger.Warning($"AppClient not found from AwsCognitoHandler. Message: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Users
        public Task CreateUser(string userPoolId, string userName)
        {
            return AmazonCognitoIdentityClient.AdminCreateUserAsync(new AdminCreateUserRequest
            {
                UserPoolId = userPoolId,
                Username = userName
            });
        }

        public Task SetUserPassword(string userPoolId, string userName, string userPassword)
        {
            return AmazonCognitoIdentityClient.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
            {
                UserPoolId = userPoolId,
                Username = userName,
                Password = userPassword,
                Permanent = true
            });
        }

        public Task DeleteUser(string userPoolId, string userName)
        {
            return AmazonCognitoIdentityClient.AdminDeleteUserAsync(new AdminDeleteUserRequest
            {
                UserPoolId = userPoolId,
                Username = userName
            });
        }

        public async Task<IList<UserType>> GetAllUsers(string userPoolId)
        {
            var users = await AmazonCognitoIdentityClient.ListUsersAsync(new ListUsersRequest
            {
                UserPoolId = userPoolId,
            }).ConfigureAwait(false);

            return users.Users;
        }

        public Task<AdminGetUserResponse> GetUser(string userPoolId, string userName)
        {
            try
            {
                return AmazonCognitoIdentityClient.AdminGetUserAsync(new AdminGetUserRequest
                {
                    UserPoolId = userPoolId,
                    Username = userName
                });
            }
            catch (UserNotFoundException ex)
            {
                PostNLLogger.Warning($"User not found from AwsCognitoHandler. Message: {ex.Message}");
                return Task.FromResult<AdminGetUserResponse>(null);
            }
        }

        public async Task<bool> DisableUser(string userPoolId, string userName)
        {
            var response = await AmazonCognitoIdentityClient.AdminDisableUserAsync(new AdminDisableUserRequest
            {
                UserPoolId = userPoolId,
                Username = userName
            }).ConfigureAwait(false);

            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> EnableUser(string userPoolId, string userName)
        {
            var response = await AmazonCognitoIdentityClient.AdminEnableUserAsync(new AdminEnableUserRequest
            {
                UserPoolId = userPoolId,
                Username = userName
            }).ConfigureAwait(false);

            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        #endregion

        #region Auth
        public Task<AdminInitiateAuthResponse> AdminInitiateAuth(string userPoolId, string appClientId, string userName, string userPassword)
        {
            var authFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH;
            var authParameters = new Dictionary<string, string>()
            {
                {"USERNAME", userName },
                {"PASSWORD", userPassword }
            };

            return AdminAuth(userPoolId, appClientId, authFlow, authParameters);
        }

        public Task<AuthFlowResponse> GetAccessToken(string userPoolId, string appClientId, string appClientSecret, string userName, string userPassword)
        {
            CognitoUserPool userPool = new(userPoolId, appClientId, AmazonCognitoIdentityClient, appClientSecret);
            CognitoUser user = new(userName, appClientId, userPool, AmazonCognitoIdentityClient, appClientSecret);
            InitiateSrpAuthRequest authRequest = new() { Password = userPassword };

            return user.StartWithSrpAuthAsync(authRequest);
        }

        public Task<AuthFlowResponse> RefreshTokens(string userPoolId, string appClientId, string userName, string refreshToken, string appClientSecret = null)
        {
            CognitoUserPool userPool = new(userPoolId, appClientId, AmazonCognitoIdentityClient, appClientSecret);
            CognitoUser user = new(userName, appClientId, userPool, AmazonCognitoIdentityClient, appClientSecret)
            {
                SessionTokens = new CognitoUserSession(null, null, refreshToken, DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
            };

            InitiateRefreshTokenAuthRequest refreshRequest = new()
            {
                AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
            };

            return user.StartWithRefreshTokenAuthAsync(refreshRequest);
        }

        private Task<AdminInitiateAuthResponse> AdminAuth(string userPoolId, string appClientId, AuthFlowType authFlow, Dictionary<string, string> authParameters)
        {
            var adminInitiateAuthRequest = new AdminInitiateAuthRequest
            {
                UserPoolId = userPoolId,
                ClientId = appClientId,
                AuthFlow = authFlow,
                AuthParameters = authParameters
            };

            return AmazonCognitoIdentityClient.AdminInitiateAuthAsync(adminInitiateAuthRequest);
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                AmazonCognitoIdentityClient.Dispose();
            }

            _disposed = true;
        }
    }
}
