using Devon4Net.Infrastructure.AWS.Common.Options;
using Microsoft.Extensions.Options;

namespace Devon4Net.Infrastructure.AWS.Common.Validators
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly ApiKeysOptions _apiKeyOptions;

        public ApiKeyValidator(IOptions<ApiKeysOptions> apiKeyOptions)
        {
            _apiKeyOptions = apiKeyOptions.Value;
        }

        public bool ValidateApiToken(string apiToken)
        {
            return _apiKeyOptions?.ApiKeys?.Contains(apiToken) == true;
        }
    }
}
