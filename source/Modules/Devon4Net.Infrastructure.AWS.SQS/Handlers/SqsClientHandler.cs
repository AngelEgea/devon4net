using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Devon4Net.Infrastructure.AWS.Common.Options;
using Devon4Net.Infrastructure.AWS.SQS.Dto;
using Devon4Net.Infrastructure.AWS.SQS.Helper;
using Devon4Net.Infrastructure.AWS.SQS.Interfaces;
using Devon4Net.Infrastructure.Common;
using System.Net;
using System.Text.Json;

namespace Devon4Net.Infrastructure.AWS.SQS.Handlers
{
    public class SqsClientHandler : ISqsClientHandler
    {
        private AWSCredentials AWSCredentials { get; }
        private RegionEndpoint RegionEndpoint { get; }
        private AmazonSQSClient AmazonSQSClient { get; }
        private bool _disposed = false;

        public SqsClientHandler(AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            AWSCredentials = awsCredentials;
            RegionEndpoint = regionEndpoint;
            AmazonSQSClient = CreateSQSClient();
        }

        public SqsClientHandler(AmazonSQSClient amazonSQSClient, AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            AWSCredentials = awsCredentials;
            RegionEndpoint = regionEndpoint;
            AmazonSQSClient = amazonSQSClient;
        }

        public AmazonSQSClient CreateSQSClient()
        {
            var sqsConfig = new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint
            };

            return new AmazonSQSClient(AWSCredentials, sqsConfig);
        }

        public async Task<List<string>> GetSqsQueues(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await AmazonSQSClient.ListQueuesAsync(string.Empty, cancellationToken).ConfigureAwait(false);
                return result.QueueUrls;
            }
            catch (Exception ex)
            {
                LogSqsException(ex);
                throw;
            }
        }

        public async Task<string> CreateSqsQueue(SqsQueueOptions sqsQueueOptions, CancellationToken cancellationToken = default)
        {
            var queueAttrs = new Dictionary<string, string>();

            try
            {
                queueAttrs.Add(QueueAttributeName.FifoQueue, sqsQueueOptions.UseFifo.ToString().ToLowerInvariant());
                queueAttrs.Add(QueueAttributeName.DelaySeconds, sqsQueueOptions.DelaySeconds.ToString().ToLowerInvariant());
                queueAttrs.Add(QueueAttributeName.MaximumMessageSize, sqsQueueOptions.MaximumMessageSize.ToString().ToLowerInvariant());
                queueAttrs.Add(QueueAttributeName.ReceiveMessageWaitTimeSeconds, sqsQueueOptions.ReceiveMessageWaitTimeSeconds.ToString().ToLowerInvariant());

                if (sqsQueueOptions.RedrivePolicy != null && !string.IsNullOrEmpty(sqsQueueOptions.RedrivePolicy.DeadLetterQueueUrl) && sqsQueueOptions.RedrivePolicy.MaxReceiveCount > 0)
                {
                    queueAttrs.Add(QueueAttributeName.RedrivePolicy, await GetRedrivePolicy(sqsQueueOptions.RedrivePolicy, cancellationToken).ConfigureAwait(false));
                }

                var responseCreate = await AmazonSQSClient.CreateQueueAsync(new CreateQueueRequest { QueueName = sqsQueueOptions.QueueName, Attributes = queueAttrs }, cancellationToken).ConfigureAwait(false);
                return responseCreate.QueueUrl;
            }
            catch (QueueNameExistsException ex)
            {
                Devon4NetLogger.Error($"Error creating the queue: {sqsQueueOptions.QueueName}. The queue name already exists");
                LogSqsException(ex);
                throw;
            }
            catch (QueueDeletedRecentlyException ex)
            {
                Devon4NetLogger.Error($"Error creating the queue: {sqsQueueOptions.QueueName}. The queue has been deleted recently");
                LogSqsException(ex);
                throw;
            }
        }

        public async Task<SqsQueueStatus> GetQueueStatus(string queueName, CancellationToken cancellationToken = default)
        {
            CheckQueueName(queueName);
            var queueUrl = await GetQueueUrl(queueName, cancellationToken).ConfigureAwait(false);

            try
            {
                var attributes = new List<string> { QueueAttributeName.ApproximateNumberOfMessages, QueueAttributeName.ApproximateNumberOfMessagesNotVisible, QueueAttributeName.LastModifiedTimestamp, QueueAttributeName.ApproximateNumberOfMessagesDelayed };
                var response = await AmazonSQSClient.GetQueueAttributesAsync(new GetQueueAttributesRequest(queueUrl, attributes), cancellationToken).ConfigureAwait(false);

                return new SqsQueueStatus
                {
                    IsHealthy = response.HttpStatusCode == HttpStatusCode.OK,
                    QueueName = queueName,
                    ApproximateNumberOfMessages = response.ApproximateNumberOfMessages,
                    ApproximateNumberOfMessagesNotVisible = response.ApproximateNumberOfMessagesNotVisible,
                    LastModifiedTimestamp = response.LastModifiedTimestamp,
                    ApproximateNumberOfMessagesDelayed = response.ApproximateNumberOfMessagesDelayed,
                    IsFifo = response.FifoQueue
                };
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error($"Failed to GetNumberOfMessages for queue {queueName}: {ex.Message}");
                LogSqsException(ex);
                throw;
            }
        }

        public async Task<List<Message>> GetSqsMessages(string queueUrl, int maxNumberOfMessagesToRetrievePerCall = 1, int ReceiveMessageWaitTimeSeconds = 0, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await AmazonSQSClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = maxNumberOfMessagesToRetrievePerCall,
                    WaitTimeSeconds = ReceiveMessageWaitTimeSeconds,
                    AttributeNames = new List<string> { QueueAttributeName.All },
                    MessageAttributeNames = new List<string> { QueueAttributeName.All }
                }, cancellationToken).ConfigureAwait(false);

                return result?.Messages;
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error($"Failed to GetSqsMessages for queue {queueUrl}: {ex.Message}");
                LogSqsException(ex);
                throw;
            }
        }

        public async Task SendMessage<T>(string queueUrl, string message, bool isFifo, string accessKeyId = null, string secretAccessKey = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = message,
                    MessageAttributes = SqsMessageTypeAttributehelper.CreateAttributes<T>()
                };

                if (isFifo)
                {
                    sendMessageRequest.MessageGroupId = typeof(T).Name;
                    sendMessageRequest.MessageDeduplicationId = Guid.NewGuid().ToString();
                }

                var sqsClient = string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(secretAccessKey) ?
                    AmazonSQSClient :
                    new AmazonSQSClient(accessKeyId, secretAccessKey);

                await sqsClient.SendMessageAsync(sendMessageRequest, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error($"Failed to PostMessagesAsync for queue {queueUrl}: {ex.Message}");
                LogSqsException(ex);
                throw;
            }
        }

        public Task SendMessage<T>(string queueUrl, object message, bool isFifo, string accessKeyId = null, string secretAccessKey = null, CancellationToken cancellationToken = default)
        {
            return SendMessage<T>(queueUrl, JsonSerializer.Serialize(message), isFifo, accessKeyId, secretAccessKey, cancellationToken);
        }

        public async Task<bool> DeleteMessage(string queueUrl, string receiptHandle, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await AmazonSQSClient.DeleteMessageAsync(queueUrl, receiptHandle, cancellationToken).ConfigureAwait(false);
                return result.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error($"Failed to DeleteMessage for queue {queueUrl}: {ex.Message}");
                LogSqsException(ex);
                throw;
            }
        }

        public async Task DeleteSqsQueue(string queueUrl, bool waitForDeletion = false, CancellationToken cancellationToken = default)
        {
            try
            {
                await AmazonSQSClient.DeleteQueueAsync(queueUrl, cancellationToken).ConfigureAwait(false);

                if (!waitForDeletion) return;

                var queueExists = true;

                do
                {
                    var queues = await GetSqsQueues(cancellationToken).ConfigureAwait(false);
                    queueExists = queues.Any(q => q == queueUrl);
                    await Task.Delay(1000, cancellationToken);
                } while (queueExists);
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error($"Error deleting the queue: {queueUrl}");
                LogSqsException(ex);
            }
        }

        public async Task UpdateSqsQueueAttribute(string queueUrl, string attribute, string value, CancellationToken cancellationToken = default)
        {
            try
            {
                await AmazonSQSClient.SetQueueAttributesAsync(queueUrl, new Dictionary<string, string> { { attribute, value } }, cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidAttributeNameException ex)
            {
                Devon4NetLogger.Error($"Error updating the attribute: {attribute}. The queue has been deleted recently name");
                LogSqsException(ex);
            }
        }

        public async Task<string> GetQueueArn(string queueUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var responseGetAtt = await AmazonSQSClient.GetQueueAttributesAsync(queueUrl, new List<string> { QueueAttributeName.QueueArn }, cancellationToken).ConfigureAwait(false);
                return responseGetAtt.QueueARN;
            }
            catch (InvalidAttributeNameException ex)
            {
                Devon4NetLogger.Error($"Error getting the queue arn: {queueUrl}");
                LogSqsException(ex);
            }

            return null;
        }

        public async Task<string> GetQueueUrl(string queueName, CancellationToken cancellationToken = default)
        {
            CheckQueueName(queueName);

            try
            {
                var response = await AmazonSQSClient.GetQueueUrlAsync(queueName, cancellationToken).ConfigureAwait(false);
                return response.QueueUrl;
            }
            catch (QueueDoesNotExistException ex)
            {
                LogSqsException(ex);
                throw new InvalidOperationException($"Could not retrieve the URL for the queue {queueName} as it does not exist or check if you do not have access to the queue", ex);
            }
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
                AmazonSQSClient?.Dispose();
            }

            _disposed = true;
        }

        private static void LogSqsException(Exception exception)
        {
            var message = exception?.Message;
            var innerException = exception?.InnerException;
            Devon4NetLogger.Error($"Error performing the SQS action:{message} {innerException}");
        }

        private static void CheckQueueName(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                Devon4NetLogger.Error("Queue name can not be null or empty");
                throw new ArgumentException("Queue name can not be null or empty");
            }
        }

        private async Task<string> GetRedrivePolicy(RedrivePolicyOptions redrivePolicyOptions, CancellationToken cancellationToken = default)
        {
            return JsonSerializer.Serialize(new RedrivePolicy
            {
                DeadLetterQueueUrl = await GetQueueArn(redrivePolicyOptions.DeadLetterQueueUrl, cancellationToken).ConfigureAwait(false),
                MaxReceiveCount = redrivePolicyOptions.MaxReceiveCount
            });
        }
    }
}
