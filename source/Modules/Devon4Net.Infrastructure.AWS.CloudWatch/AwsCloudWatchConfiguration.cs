using Devon4Net.Infrastructure.AWS.CloudWatch.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace Devon4Net.Infrastructure.AWS.CloudWatch
{
    public static class AwsCloudWatchConfiguration
    {
        public static void SetUpAwsCloudWatch(this IServiceCollection services)
        {
            services.AddSingleton<IAwsCloudWatchLogsHandler, AwsCloudWatchLogsHandler>();
        }
    }
}
