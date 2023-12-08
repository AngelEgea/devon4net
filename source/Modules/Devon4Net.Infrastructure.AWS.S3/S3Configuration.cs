using Devon4Net.Infrastructure.AWS.S3.Handlers;
using Devon4Net.Infrastructure.AWS.S3.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Devon4Net.Infrastructure.AWS.S3
{
    public static class S3Configuration
    {
        public static void SetupS3(this IServiceCollection services)
        {
            services.AddSingleton<IAwsS3Handler, AwsS3Handler>();
        }
    }
}
