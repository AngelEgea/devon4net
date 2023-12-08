using Amazon.SimpleSystemsManagement.Model;

namespace Devon4Net.Infrastructure.AWS.Common.Managers.ParameterStoreManager.Interfaces
{
    public interface IAwsParameterStoreHandler : IDisposable
    {
        Task<List<ParameterMetadata>> GetAllParameters(CancellationToken cancellationToken = default);
        Task<string> GetParameterValue(string parameterName, CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetAllParametersAndValuesByPath(string path = "/", CancellationToken cancellationToken = default);
    }
}