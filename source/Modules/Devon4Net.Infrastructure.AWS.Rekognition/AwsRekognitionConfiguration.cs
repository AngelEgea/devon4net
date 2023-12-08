using ADC.PostNL.BuildingBlocks.AWS.Rekognition.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace ADC.PostNL.BuildingBlocks.AWS.Rekognition
{
    public static class AwsRekognitionConfiguration
    {
        public static void SetUpAwsRekognition(this IServiceCollection services)
        {
            services.AddSingleton<IAwsRekognitionHandler, AwsRekognitionHandler>();
        }
    }
}
