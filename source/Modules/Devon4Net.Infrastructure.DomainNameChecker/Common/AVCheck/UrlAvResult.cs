using System.Net;

namespace Devon4Net.Infrastructure.DomainNameChecker.Common.AVCheck
{
    public class UrlAvResult
    {
        public string Url { get; set; }
        public AvResult AvScanResult { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
