using ADC.PostNL.BuildingBlocks.AWS.SES.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace ADC.PostNL.BuildingBlocks.AWS.SES
{
    public static class AwsSesConfiguration
    {
        public static void SetUpAwsSes(this IServiceCollection services)
        {
            services.AddSingleton<IAwsSesHandler, AwsSesHandler>();
        }
    }
}
