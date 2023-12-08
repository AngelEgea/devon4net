namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.Options
{
    public class DomainNameParserOptions
    {
        public bool UseDomainNameParser { get; set; }
        public AdvancedDomaincheck AdvancedDomainCheck { get; set; }
    }

    public class AdvancedDomaincheck
    {
        public bool UseEmbebedTopLevelDomainList { get; set; }
        public LocalTopLevelDomainList LocalTopLevelDomainList { get; set; }
        public Onlinetopleveldomainlist OnlineTopLevelDomainList { get; set; }
        public List<string> ExcludedDomainList { get; set; }
    }

    public class LocalTopLevelDomainList
    {
        public bool UseLocalTopLevelDomainList { get; set; }
        public string LocalTopLevelDomainListPath { get; set; }
    }

    public class Onlinetopleveldomainlist
    {
        public bool UseOnlineTopLevelDomainList { get; set; }
        public string TopLevelDomainListUrl { get; set; }
    }
}
