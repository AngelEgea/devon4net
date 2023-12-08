using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using Devon4Net.Infrastructure.Common;
using System.Net;

namespace Devon4Net.Infrastructure.AWS.CloudWatch.Handler
{
    public class AwsCloudWatchLogsHandler : IAwsCloudWatchLogsHandler
    {
        private AmazonCloudWatchLogsClient _amazonCloudWatchLogsClient { get; }
        private AWSCredentials _awsCredentials { get; }
        private RegionEndpoint _regionEndpoint { get; }

        private bool _disposed = false;

        public AwsCloudWatchLogsHandler(AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            _awsCredentials = awsCredentials;
            _regionEndpoint = regionEndpoint;
            _amazonCloudWatchLogsClient = CreateCloudWatchLogsClient();
        }

        public AwsCloudWatchLogsHandler(AmazonCloudWatchLogsClient amazonCloudWatchLogsClient, AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            _awsCredentials = awsCredentials;
            _regionEndpoint = regionEndpoint;
            _amazonCloudWatchLogsClient = amazonCloudWatchLogsClient;
        }

        public async Task<bool> PutLogGroupRetentionPolicy(string logGroupName, int retentionDays)
        {
            try
            {
                var request = new PutRetentionPolicyRequest
                {
                    LogGroupName = logGroupName,
                    RetentionInDays = retentionDays,
                };

                var putLogGroupRetentionResponse = await _amazonCloudWatchLogsClient.PutRetentionPolicyAsync(request);

                if (putLogGroupRetentionResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Devon4NetLogger.Information($"The retention of the log group {logGroupName} could not be adjusted (status code: {putLogGroupRetentionResponse.HttpStatusCode})");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error($"An error ocurred while trying to update log group retention of {logGroupName}");
                Devon4NetLogger.Error(ex);

                return false;
            }
        }

        public async Task<List<LogGroup>> ListLogGroups()
        {
            var listLogGroupsResponse = await _amazonCloudWatchLogsClient.DescribeLogGroupsAsync();

            if (listLogGroupsResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                return listLogGroupsResponse.LogGroups;
            }
            else
            {
                var errorMessage = $"Error trying to Describe LogGroups (status code: {listLogGroupsResponse.HttpStatusCode})";
                Devon4NetLogger.Error(errorMessage);
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        private AmazonCloudWatchLogsClient CreateCloudWatchLogsClient()
        {
            return _awsCredentials == null ? new AmazonCloudWatchLogsClient(_regionEndpoint) : new AmazonCloudWatchLogsClient(_awsCredentials, _regionEndpoint);
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
                _amazonCloudWatchLogsClient?.Dispose();
            }

            _disposed = true;
        }
    }
}
