namespace Devon4Net.Infrastructure.DomainNameChecker.Options
{
    public class DomainNameCheckerOptions
    {
        public bool UseDomainNameChecker { get; set; }
        public bool UseBasicDomainCheck { get; set; }
        public bool UseClamAv { get; set; }
        public bool UseDomainNameParser { get; set; }
        public bool CallUrl { get; set; }
        public int HttpRequestTimeout { get; set; }
    }
}
