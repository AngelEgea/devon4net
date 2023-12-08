using Amazon.CloudWatchLogs.Model;

namespace Devon4Net.Infrastructure.AWS.CloudWatch.Handler
{
    public interface IAwsCloudWatchLogsHandler : IDisposable
    {
        Task<bool> PutLogGroupRetentionPolicy(string logGroupName, int retentionDays);
        Task<List<LogGroup>> ListLogGroups();
    }
}
