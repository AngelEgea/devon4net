using Amazon;
using Amazon.Runtime;
using Devon4Net.Infrastructure.AWS.Common.Managers.ParameterStoreManager.Handlers;
using Devon4Net.Infrastructure.AWS.Common.Managers.ParameterStoreManager.Interfaces;
using Devon4Net.Infrastructure.Common;
using Devon4Net.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Devon4Net.Infrastructure.AWS.Common.Managers.ParameterStoreManager
{
    public class AwsParameterStoreConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private bool _disposed;
        private IAwsParameterStoreHandler _awsParameterStoreHandler { get; set; }

        public AwsParameterStoreConfigurationProvider(AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            _awsParameterStoreHandler = new AwsParameterStoreHandler(awsCredentials, regionEndpoint);
            _disposed = false;
        }
        public override void Load()
        {
            base.Load();
            Data = GetAwsParameterStoreData(default).ConfigureAwait(false).GetAwaiter().GetResult();
        }

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
                _awsParameterStoreHandler.Dispose();
            }
            _disposed = true;
        }

        private async Task<Dictionary<string, string>> GetAwsParameterStoreData(CancellationToken cancellationToken)
        {
            var parameters = await _awsParameterStoreHandler.GetAllParametersAndValuesByPath(cancellationToken: cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<string, string>();

            foreach (var parameter in parameters)
            {
                var parameterName = parameter.Key;
                var parameterValue = parameter.Value;

                try
                {
                    foreach (var pair in ConfigurationProviderHelper.JsonToStringDictionary(parameterValue, parameterName))
                    {
                        result.Add(pair.Key, pair.Value);
                    }
                }
                catch (JsonException ex)
                {
                    Devon4NetLogger.Warning($"The parameter {parameterName} could not be parsed as a JSON, it will be added to the options as a plain string");
                    Devon4NetLogger.Warning(ex);
                    result.Add(parameterName, parameterValue);
                }
            }

            return result;
        }
    }
}
