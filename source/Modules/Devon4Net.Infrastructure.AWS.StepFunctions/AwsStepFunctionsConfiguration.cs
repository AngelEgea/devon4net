using ADC.PostNL.BuildingBlocks.AWS.StepFunctions.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace ADC.PostNL.BuildingBlocks.AWS.StepFunctions
{
    public static class AwsStepFunctionsConfiguration
    {
        public static void SetUpAwsStepFunctions(this IServiceCollection services)
        {
            services.AddSingleton<IAwsStepFunctionsHandler, AwsStepFunctionsHandler>();
        }
    }
}
