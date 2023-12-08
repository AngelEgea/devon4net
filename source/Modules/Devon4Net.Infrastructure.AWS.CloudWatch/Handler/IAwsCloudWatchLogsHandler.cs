using Amazon.CloudWatchLogs.Model;

namespace ADC.PostNL.BuildingBlocks.AWS.CloudWatch.Handler
{
    public interface IAwsCloudWatchLogsHandler : IDisposable
    {
        Task<bool> PutLogGroupRetentionPolicy(string logGroupName, int retentionDays);
        Task<List<LogGroup>> ListLogGroups();
    }
}
