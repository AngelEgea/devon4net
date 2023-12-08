using ADC.PostNL.BuildingBlocks.AWS.Cognito.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace ADC.PostNL.BuildingBlocks.AWS.Cognito
{
    public static class AwsCognitoConfiguration
    {
        public static void SetUpAwsCognito(this IServiceCollection services)
        {
            services.AddSingleton<IAwsCognitoHandler, AwsCognitoHandler>();
        }
    }
}
