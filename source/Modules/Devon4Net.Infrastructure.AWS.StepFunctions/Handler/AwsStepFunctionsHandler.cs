using Amazon;
using Amazon.Runtime;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Devon4Net.Infrastructure.Common.Helpers;
using Devon4Net.Infrastructure.Common.Helpers.Interfaces;

namespace Devon4Net.Infrastructure.AWS.StepFunctions.Handler
{
    public class AwsStepFunctionsHandler : IAwsStepFunctionsHandler
    {
        private readonly AmazonStepFunctionsClient AmazonStepFunctionsClient;

        private readonly IJsonHelper JsonHelper;

        private bool _disposed = false;

        public AwsStepFunctionsHandler(IJsonHelper jsonHelper = null)
        {
            AmazonStepFunctionsClient = new AmazonStepFunctionsClient();
            JsonHelper = jsonHelper ?? new JsonHelper();
        }

        public AwsStepFunctionsHandler(AWSCredentials awsCredentials, IJsonHelper jsonHelper = null)
        {
            AmazonStepFunctionsClient = new AmazonStepFunctionsClient(awsCredentials);
            JsonHelper = jsonHelper ?? new JsonHelper();
        }

        public AwsStepFunctionsHandler(RegionEndpoint awsRegion, IJsonHelper jsonHelper = null)
        {
            AmazonStepFunctionsClient = new AmazonStepFunctionsClient(awsRegion);
            JsonHelper = jsonHelper ?? new JsonHelper();
        }

        public AwsStepFunctionsHandler(AWSCredentials awsCredentials, RegionEndpoint awsRegion, IJsonHelper jsonHelper = null)
        {
            AmazonStepFunctionsClient = new AmazonStepFunctionsClient(awsCredentials, awsRegion);
            JsonHelper = jsonHelper ?? new JsonHelper();
        }

        #region StateMachine
        public async Task<StartExecutionResponse> StartStateMachineExecution<T>(string stateMachineArn, T inputDataObject)
        {
            var jsonInput = await JsonHelper.Serialize(inputDataObject).ConfigureAwait(false);
            return await StartStateMachineExecution(stateMachineArn, jsonInput).ConfigureAwait(false);
        }

        public Task<StartExecutionResponse> StartStateMachineExecution(string stateMachineArn, string inputDataJson)
        {
            var startExecutionRequest = new StartExecutionRequest
            {
                Input = inputDataJson,
                Name = Guid.NewGuid().ToString("N"),
                StateMachineArn = stateMachineArn
            };

            return AmazonStepFunctionsClient.StartExecutionAsync(startExecutionRequest);
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                AmazonStepFunctionsClient?.Dispose();
            }

            _disposed = true;
        }
    }
}
