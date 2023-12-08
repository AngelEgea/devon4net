namespace Devon4Net.Infrastructure.DomainNameChecker.Common.AVCheck
{
    public interface IAVChecker
    {
        Task<AvResult> ScanStream(Stream inputStream);
        Task<AvResult> ScanFile(string filePath);
        Task<AvResult> ScanUrl(string url);
    }
}