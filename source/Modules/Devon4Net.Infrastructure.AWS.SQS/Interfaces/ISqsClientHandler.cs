using Amazon.SQS;
using Amazon.SQS.Model;
using Devon4Net.Infrastructure.AWS.Common.Options;
using Devon4Net.Infrastructure.AWS.SQS.Dto;

namespace Devon4Net.Infrastructure.AWS.SQS.Interfaces
{
    public interface ISqsClientHandler : IDisposable
    {
        AmazonSQSClient CreateSQSClient();
        Task<List<string>> GetSqsQueues(CancellationToken cancellationToken = default);
        Task<string> CreateSqsQueue(SqsQueueOptions sqsQueueOptions, CancellationToken cancellationToken = default);
        Task UpdateSqsQueueAttribute(string queueUrl, string attribute, string value, CancellationToken cancellationToken = default);
        Task<SqsQueueStatus> GetQueueStatus(string queueName, CancellationToken cancellationToken = default);
        Task DeleteSqsQueue(string queueUrl, bool waitForDeletion = false, CancellationToken cancellationToken = default);
        Task<List<Message>> GetSqsMessages(string queueUrl, int maxNumberOfMessagesToRetrievePerCall = 1, int ReceiveMessageWaitTimeSeconds = 0, CancellationToken cancellationToken = default);
        Task SendMessage<T>(string queueUrl, object message, bool isFifo, string accessKeyId = null, string secretAccessKey = null, CancellationToken cancellationToken = default);
        Task SendMessage<T>(string queueUrl, string message, bool isFifo, string accessKeyId = null, string secretAccessKey = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteMessage(string queueUrl, string receiptHandle, CancellationToken cancellationToken = default);
        Task<string> GetQueueArn(string queueUrl, CancellationToken cancellationToken = default);
        Task<string> GetQueueUrl(string queueName, CancellationToken cancellationToken = default);
    }
}