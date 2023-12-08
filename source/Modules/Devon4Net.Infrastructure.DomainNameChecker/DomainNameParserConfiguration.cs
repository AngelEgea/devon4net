using ADC.PostNL.BuildingBlocks.Common.Handlers;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker
{
    public static class DomainNameParserConfiguration
    {
        public static void SetupDomainNameParser(this IServiceCollection services, IConfiguration configuration)
        {
            var domainNameCheckerOptions = services.GetTypedOptions<DomainNameParserOptions>(configuration, "DomainNameParser");
            if (domainNameCheckerOptions == null || domainNameCheckerOptions?.UseDomainNameParser != true) return;

            services.AddSingleton(new TopLevelDomainList(domainNameCheckerOptions));
            services.AddTransient<DomainNameParser>();
        }
    }
}
