using ADC.PostNL.BuildingBlocks.DomainNameChecker.Common;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.Handlers
{
    public interface IDomainNameCheckerHandler
    {
        Task<DomainNameCheckerAnalysis> ParseDomain(string url);
    }
}
