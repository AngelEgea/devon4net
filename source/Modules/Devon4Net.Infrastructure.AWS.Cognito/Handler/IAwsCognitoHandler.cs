using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;

namespace ADC.PostNL.BuildingBlocks.AWS.Cognito.Handler
{
    public interface IAwsCognitoHandler : IDisposable
    {
        //UserPools
        Task<CreateUserPoolResponse> CreateUserPool(CreateUserPoolRequest userPoolRequest);
        Task DeleteUserPool(string userPoolId);
        Task<ListUserPoolsResponse> GetAllUserPools();
        Task<DescribeUserPoolResponse> GetUserPool(string userPoolId);

        //AppClients
        Task<UserPoolClientType> CreateAppClient(CreateUserPoolClientRequest request);
        Task DeleteAppClient(string userPoolId, string appClientId);
        Task<IList<UserPoolClientDescription>> GetAllAppClients(string userPoolId);
        Task<UserPoolClientType> GetAppClient(string userPoolId, string appClientId);

        //Users
        Task CreateUser(string userPoolId, string userName);
        Task SetUserPassword(string userPoolId, string userName, string userPassword);
        Task DeleteUser(string userPoolId, string userName);
        Task<IList<UserType>> GetAllUsers(string userPoolId);
        Task<AdminGetUserResponse> GetUser(string userPoolId, string userName);
        Task<bool> DisableUser(string userPoolId, string userName);
        Task<bool> EnableUser(string userPoolId, string userName);

        //Auth_Flow
        Task<AdminInitiateAuthResponse> AdminInitiateAuth(string userPoolId, string appClientId, string userName, string userPassword);
        Task<AuthFlowResponse> GetAccessToken(string userPoolId, string appClientId, string appClientSecret, string userName, string userPassword);
        Task<AuthFlowResponse> RefreshTokens(string userPoolId, string appClientId, string userName, string refreshToken, string appClientSecret = null);
    }
}
