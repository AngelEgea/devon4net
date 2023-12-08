using Amazon;
using Amazon.DynamoDBv2;
using Devon4Net.Infrastructure.AWS.Common.Options;
using Devon4Net.Infrastructure.AWS.DynamoDb.Domain.Repository;
using Devon4Net.Infrastructure.Common.Constants;
using Devon4Net.Infrastructure.Common.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devon4Net.Infrastructure.AWS.DynamoDb
{
    public static class DynamoDbConfiguration
    {
        public static void SetupDynamoDb(this IServiceCollection services, IConfiguration configuration)
        {
            var awsOptions = services.GetTypedOptions<AwsOptions>(configuration, OptionsDefinition.AwsOptions);

            if (awsOptions.UseDynamoDb)
            {
                services.AddSingleton(typeof(IDynamoDbEntityRepository<>), typeof(DynamoDbEntityRepository<>));
                services.AddSingleton<IDynamoDbTableRepository, DynamoDbTableRepository>();

                services.AddSingleton(serviceProvider =>
                {
                    var amazonDynamoDbConfig = new AmazonDynamoDBConfig
                    {
                        RegionEndpoint = serviceProvider.GetService<RegionEndpoint>()
                    };

                    if (awsOptions.DisableDynamoDbRetry)
                    {
                        amazonDynamoDbConfig.MaxErrorRetry = 0;
                    }

                    return amazonDynamoDbConfig;
                });
            }
        }
    }
}
