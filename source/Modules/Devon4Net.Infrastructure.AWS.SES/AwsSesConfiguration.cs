using Devon4Net.Infrastructure.AWS.SES.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace Devon4Net.Infrastructure.AWS.SES
{
    public static class AwsSesConfiguration
    {
        public static void SetUpAwsSes(this IServiceCollection services)
        {
            services.AddSingleton<IAwsSesHandler, AwsSesHandler>();
        }
    }
}
