namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser
{
    public class AdvancedDomainInfo
    {
        public bool IsValid { get; set; }
        public bool IsExcluded { get; set; }
        public string TLD { get; set; }
        public string SLD { get; set; }
        public string SubDomain { get; set; }
        public TopLevelDomainRule TopLevelDomainRule { get; set; }
    }
}
