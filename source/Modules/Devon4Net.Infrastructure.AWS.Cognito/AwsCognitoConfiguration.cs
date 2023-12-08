using Devon4Net.Infrastructure.AWS.Cognito.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace Devon4Net.Infrastructure.AWS.Cognito
{
    public static class AwsCognitoConfiguration
    {
        public static void SetUpAwsCognito(this IServiceCollection services)
        {
            services.AddSingleton<IAwsCognitoHandler, AwsCognitoHandler>();
        }
    }
}
