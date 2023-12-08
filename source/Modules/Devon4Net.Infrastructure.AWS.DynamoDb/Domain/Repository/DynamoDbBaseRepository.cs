using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Devon4Net.Infrastructure.AWS.DynamoDb.Constants;
using Devon4Net.Infrastructure.Common;
using Devon4Net.Infrastructure.Common.Helpers;

namespace Devon4Net.Infrastructure.AWS.DynamoDb.Domain.Repository
{
    public class DynamoDbBaseRepository : IDynamoDbBaseRepository
    {
        protected IDynamoDBContext DynamoDBContext { get; }
        protected IAmazonDynamoDB AmazonDynamoDBClient { get; }
        protected JsonHelper JsonHelper { get; }

        private bool _disposed = false;

        public DynamoDbBaseRepository(AWSCredentials awsCredentials, AmazonDynamoDBConfig amazonDynamoDBConfig, JsonHelper jsonHelper = null)
        {
            AmazonDynamoDBClient = new AmazonDynamoDBClient(awsCredentials, amazonDynamoDBConfig);
            DynamoDBContext = new DynamoDBContext(AmazonDynamoDBClient);
            JsonHelper = jsonHelper ?? new JsonHelper();
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
                DynamoDBContext?.Dispose();
                AmazonDynamoDBClient?.Dispose();
            }

            _disposed = true;
        }

        protected static void LogDynamoException(ref Exception exception)
        {
            var message = exception?.Message;
            var innerException = exception?.InnerException;
            Devon4NetLogger.Error($"Error performing the DynamoDB action:{message} {innerException}");
        }

        protected List<T> TransformData<T>(List<Document> documents)
        {
            var objToCast = documents.ConvertAll(d => d[DynamoDbGeneralObjectStorageAttributes.AttributeValue].AsString());
            return JsonHelper.Deserialize<T>(objToCast);
        }

        #region tables
        public async Task<bool> TableExists(string tableName)
        {
            var request = new ListTablesRequest
            {
                Limit = 10, // Page size.
                ExclusiveStartTableName = null
            };

            var response = await AmazonDynamoDBClient.ListTablesAsync(request).ConfigureAwait(false);
            var result = response.TableNames;
            var tableExists = result.Find(x => x == tableName) != null;
            return tableExists;
        }

        public async Task<CreateTableResponse> CreateTable(string tableName, List<KeySchemaElement> keySchema, List<AttributeDefinition> attributes, long readCapacityUnits = 5, long writeCapacityUnits = 5, bool streamEnabled = true, StreamViewType streamViewType = default)
        {
            try
            {
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    KeySchema = keySchema,
                    AttributeDefinitions = attributes,
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = readCapacityUnits,
                        WriteCapacityUnits = writeCapacityUnits
                    },
                    StreamSpecification = new StreamSpecification
                    {
                        StreamEnabled = streamEnabled,
                        StreamViewType = streamViewType ?? StreamViewType.NEW_AND_OLD_IMAGES
                    }
                };

                return await AmazonDynamoDBClient.CreateTableAsync(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        #endregion
    }
}
