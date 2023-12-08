using Amazon.StepFunctions.Model;

namespace ADC.PostNL.BuildingBlocks.AWS.StepFunctions.Handler
{
    public interface IAwsStepFunctionsHandler : IDisposable
    {
        Task<StartExecutionResponse> StartStateMachineExecution<T>(string stateMachineArn, T inputDataObject);
        Task<StartExecutionResponse> StartStateMachineExecution(string stateMachineArn, string inputDataJson);
    }
}
