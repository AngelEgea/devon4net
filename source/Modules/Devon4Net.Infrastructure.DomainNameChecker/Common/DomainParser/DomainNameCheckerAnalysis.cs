using Devon4Net.Infrastructure.DomainNameChecker.Common.AVCheck;
using Devon4Net.Infrastructure.DomainNameChecker.DomainParser;
using System.Net;

namespace Devon4Net.Infrastructure.DomainNameChecker.Common.DomainParser
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
