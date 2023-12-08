using ADC.PostNL.BuildingBlocks.AWS.CloudWatch.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace ADC.PostNL.BuildingBlocks.AWS.CloudWatch
{
    public static class AwsCloudWatchConfiguration
    {
        public static void SetUpAwsCloudWatch(this IServiceCollection services)
        {
            services.AddSingleton<IAwsCloudWatchLogsHandler, AwsCloudWatchLogsHandler>();
        }
    }
}
