using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Devon4Net.Infrastructure.AWS.Common.Extensions
{
    public static class AwsParametersExtension
    {
        public static void GetAwsPlainParameter<T>(this IServiceCollection services, IConfiguration configuration, string sectionName) where T : class, new()
        {
            services.AddSingleton(JsonSerializer.Deserialize<T>(configuration.GetSection(sectionName).Value));
        }
    }
}
