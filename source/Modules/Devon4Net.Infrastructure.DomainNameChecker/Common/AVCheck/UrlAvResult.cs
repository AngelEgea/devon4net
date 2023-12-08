using ADC.PostNL.BuildingBlocks.DomainNameChecker.Common.AVCheck;
using System.Net;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.Common
{
    public class UrlAvResult
    {
        public string Url { get; set; }
        public AvResult AvScanResult { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
