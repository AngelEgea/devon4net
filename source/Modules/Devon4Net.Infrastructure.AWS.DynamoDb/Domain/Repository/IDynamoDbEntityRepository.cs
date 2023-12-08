using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Devon4Net.Infrastructure.AWS.DynamoDb.Common;

namespace Devon4Net.Infrastructure.AWS.DynamoDb.Domain.Repository
{
    public interface IDynamoDbEntityRepository<T> : IDynamoDbBaseRepository where T : class
    {
        Task Create(T entity, bool ignoreNullValues = false, CancellationToken cancellationToken = default);
        Task Create(IEnumerable<T> entities, bool ignoreNullValues = false, CancellationToken cancellationToken = default);
        Task Update(T entity, bool ignoreNullValues = false, CancellationToken cancellationToken = default);
        Task<IList<T>> Get(string paginationToken = null, CancellationToken cancellationToken = default);
        Task<IList<T>> Get(List<ScanCondition> searchCriteria, ConditionalOperatorValues conditionalOperator = ConditionalOperatorValues.And, CancellationToken cancellationToken = default);
        Task<IList<T>> Get(DynamoSearchCriteria searchCriteria, ConditionalOperatorValues conditionalOperator = ConditionalOperatorValues.And, CancellationToken cancellationToken = default);
        public IAsyncEnumerable<T> Get(string id, string indexName, List<ScanCondition> searchCriteria = null, ConditionalOperatorValues conditionalOperator = ConditionalOperatorValues.And, CancellationToken cancellationToken = default);
        public IAsyncEnumerable<T> GetEnum(List<ScanCondition> searchCriteria = null, ConditionalOperatorValues conditionalOperator = ConditionalOperatorValues.And, CancellationToken cancellationToken = default);
        Task<T> GetById(int id, CancellationToken cancellationToken = default);
        Task<T> GetById(float id, CancellationToken cancellationToken = default);
        Task<T> GetById(string id, CancellationToken cancellationToken = default);
        Task<T> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<T> GetById(DateTime id, CancellationToken cancellationToken = default);
        Task<T> GetById(object id, CancellationToken cancellationToken);
        Task DeleteById(int id, CancellationToken cancellationToken = default);
        Task DeleteById(float id, CancellationToken cancellationToken = default);
        Task DeleteById(Guid id, CancellationToken cancellationToken = default);
        Task DeleteById(DateTime id, CancellationToken cancellationToken = default);
        Task DeleteById(string id, CancellationToken cancellationToken = default);
        Task DeleteById(object id, CancellationToken cancellationToken = default);
        Task Delete(List<T> entityList, CancellationToken cancellationToken = default);
        Task Put(IEnumerable<T> entityList, CancellationToken cancellationToken = default);
    }
}