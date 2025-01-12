﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Devon4Net.Infrastructure.AWS.DynamoDb.Common;
using Devon4Net.Infrastructure.Common.Helpers;
using System.Runtime.CompilerServices;

namespace Devon4Net.Infrastructure.AWS.DynamoDb.Domain.Repository
{
    public class DynamoDbEntityRepository<T> : DynamoDbBaseRepository, IDynamoDbEntityRepository<T> where T : class
    {
        public DynamoDbEntityRepository(AWSCredentials awsCredentials, AmazonDynamoDBConfig amazonDynamoDBConfig, JsonHelper jsonHelper = null) : base(awsCredentials, amazonDynamoDBConfig, jsonHelper)
        {
        }

        public async Task Create(T entity, bool ignoreNullValues = false, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputGenericEntity(entity);
                await DynamoDBContext.SaveAsync(entity, new DynamoDBOperationConfig
                {
                    IgnoreNullValues = ignoreNullValues
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task Create(IEnumerable<T> entities, bool ignoreNullValues = false, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputGenericEntityList(entities);
                var batchWrite = DynamoDBContext.CreateBatchWrite<T>(new DynamoDBOperationConfig
                {
                    IgnoreNullValues = ignoreNullValues
                });
                batchWrite.AddPutItems(entities);
                await batchWrite.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task Update(T entity, bool ignoreNullValues = false, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputGenericEntity(entity);
                await DynamoDBContext.SaveAsync(entity, new DynamoDBOperationConfig
                {
                    IgnoreNullValues = ignoreNullValues
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task<IList<T>> Get(string paginationToken = null, CancellationToken cancellationToken = default)
        {
            var table = DynamoDBContext.GetTargetTable<T>();
            var scanOps = new ScanOperationConfig();

            if (!string.IsNullOrEmpty(paginationToken))
            {
                scanOps.PaginationToken = paginationToken;
            }

            var results = table.Scan(scanOps);
            List<Document> data = await results.GetNextSetAsync(cancellationToken).ConfigureAwait(false);

            return DynamoDBContext.FromDocuments<T>(data).ToList();
        }

        public async Task<IList<T>> Get(List<ScanCondition> searchCriteria, ConditionalOperatorValues conditionalOperator = ConditionalOperatorValues.And, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckScanConditions(searchCriteria);
                return await DynamoDBContext.ScanAsync<T>(searchCriteria, new DynamoDBOperationConfig
                {
                    ConditionalOperator = conditionalOperator
                }).GetRemainingAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public Task<IList<T>> Get(DynamoSearchCriteria searchCriteria, ConditionalOperatorValues conditionalOperator = ConditionalOperatorValues.And, CancellationToken cancellationToken = default)
        {
            return Get(searchCriteria.GetScanConditionList(), conditionalOperator, cancellationToken);
        }

        public async IAsyncEnumerable<T> Get(string id, string indexName, List<ScanCondition> searchCriteria = null, ConditionalOperatorValues conditionalOperator = ConditionalOperatorValues.And, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var indexQueryResult = DynamoDBContext.QueryAsync<T>(id, new DynamoDBOperationConfig
            {
                IndexName = indexName,
                QueryFilter = searchCriteria,
                ConditionalOperator = conditionalOperator
            });

            List<T> results;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                results = await indexQueryResult.GetNextSetAsync(CancellationToken.None).ConfigureAwait(false);

                foreach (var result in results)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        yield break;
                    }

                    yield return result;
                }
            } while (!indexQueryResult.IsDone);
        }

        public async IAsyncEnumerable<T> GetEnum(List<ScanCondition> searchCriteria = null, ConditionalOperatorValues conditionalOperator = ConditionalOperatorValues.And, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var indexQueryResult = DynamoDBContext.ScanAsync<T>(searchCriteria, new DynamoDBOperationConfig
            {
                ConditionalOperator = conditionalOperator
            });

            List<T> results;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                results = await indexQueryResult.GetNextSetAsync(CancellationToken.None).ConfigureAwait(false);

                foreach (var result in results)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        yield break;
                    }

                    yield return result;
                }
            } while (results.Count > 0);
        }

        public Task<T> GetById(int id, CancellationToken cancellationToken = default)
        {
            return Get(id, cancellationToken);
        }

        public Task<T> GetById(float id, CancellationToken cancellationToken = default)
        {
            return Get(id, cancellationToken);
        }

        public async Task<T> GetById(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputIdString(id);

                return await Get((object)id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task<T> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputIdGuid(id);

                return await Get(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task<T> GetById(DateTime id, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputIdDateTime(id);

                return await Get(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task<T> GetById(object id, CancellationToken cancellationToken)
        {
            try
            {
                CheckInputIdObject(id);

                return await Get(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public Task DeleteById(int id, CancellationToken cancellationToken = default)
        {
            return Delete(id, cancellationToken);
        }

        public Task DeleteById(float id, CancellationToken cancellationToken = default)
        {
            return Delete(id, cancellationToken);
        }

        public async Task DeleteById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputIdGuid(id);

                await Delete(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task DeleteById(DateTime id, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputIdDateTime(id);

                await Delete(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task DeleteById(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputIdString(id);

                await Delete(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task DeleteById(object id, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckInputIdObject(id);

                await Delete(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task Delete(List<T> entityList, CancellationToken cancellationToken = default)
        {
            try
            {
                var batch = DynamoDBContext.CreateBatchWrite<T>();
                batch.AddDeleteItems(entityList);
                await batch.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        public async Task Put(IEnumerable<T> entityList, CancellationToken cancellationToken = default)
        {
            try
            {
                var batch = DynamoDBContext.CreateBatchWrite<T>();
                batch.AddPutItems(entityList);
                await batch.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogDynamoException(ref ex);
                throw;
            }
        }

        #region private methods
        private Task<T> Get(object id, CancellationToken cancellationToken)
        {
            return DynamoDBContext.LoadAsync<T>(id, cancellationToken);
        }

        private Task Delete(object id, CancellationToken cancellationToken)
        {
            return DynamoDBContext.DeleteAsync<T>(id, cancellationToken);
        }

        private static void CheckScanConditions(List<ScanCondition> searchCriteria)
        {
            if (searchCriteria == null || searchCriteria.Count == 0)
            {
                throw new ArgumentNullException(nameof(searchCriteria));
            }
        }

        private static void CheckInputIdString(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
        }

        private static void CheckInputIdGuid(Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                throw new ArgumentNullException(nameof(id));
            }
        }

        private static void CheckInputIdDateTime(DateTime id)
        {
            if (id == default)
            {
                throw new ArgumentNullException(nameof(id));
            }
        }

        private static void CheckInputIdObject(object id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
        }

        private static void CheckInputGenericEntity(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
        }

        private static void CheckInputGenericEntityList(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
        }
        #endregion
    }
}
