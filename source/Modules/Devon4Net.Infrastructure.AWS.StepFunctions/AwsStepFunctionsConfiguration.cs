using Devon4Net.Infrastructure.AWS.StepFunctions.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace Devon4Net.Infrastructure.AWS.StepFunctions
{
    public static class AwsStepFunctionsConfiguration
    {
        public static void SetUpAwsStepFunctions(this IServiceCollection services)
        {
            services.AddSingleton<IAwsStepFunctionsHandler, AwsStepFunctionsHandler>();
        }
    }
}
