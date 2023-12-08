using Devon4Net.Infrastructure.DomainNameChecker.Common.DomainParser;

namespace Devon4Net.Infrastructure.DomainNameChecker.Handlers
{
    public interface IDomainNameCheckerHandler
    {
        Task<DomainNameCheckerAnalysis> ParseDomain(string url);
    }
}
