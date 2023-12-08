using ADC.PostNL.BuildingBlocks.DomainNameChecker.Common.AVCheck;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser;
using System.Net;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.Common
{
    public class DomainNameCheckerAnalysis
    {
        public List<EndPointInfo> EndPointAnalisys { get; set; }
        public AvResult AvResult { get; set; }
        public string OriginalUrl { get; set; }
        public string LastRedirectionUrl { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }

    public class EndPointInfo
    {
        public AdvancedDomainInfo AdvancedDomainInfo { get; set; }
        public bool BasicDomainValidation { get; set; }
    }
}
