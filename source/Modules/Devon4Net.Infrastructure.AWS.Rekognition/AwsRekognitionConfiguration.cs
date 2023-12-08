using Devon4Net.Infrastructure.AWS.Rekognition.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace Devon4Net.Infrastructure.AWS.Rekognition
{
    public static class AwsRekognitionConfiguration
    {
        public static void SetUpAwsRekognition(this IServiceCollection services)
        {
            services.AddSingleton<IAwsRekognitionHandler, AwsRekognitionHandler>();
        }
    }
}
