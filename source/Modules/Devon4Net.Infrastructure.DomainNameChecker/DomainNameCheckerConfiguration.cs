using ADC.PostNL.BuildingBlocks.Common.Handlers;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Handlers;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker
{
    public static class DomainNameCheckerConfiguration
    {
        public static void DomainNameCheckerSetup(this IServiceCollection services, IConfiguration configuration)
        {
            var domainNameCheckerOptions = services.GetTypedOptions<DomainNameCheckerOptions>(configuration, "DomainNameChecker");

            if (!domainNameCheckerOptions.UseDomainNameChecker)
            {
                return;
            }

            if (domainNameCheckerOptions.UseDomainNameParser)
            {
                services.SetupDomainNameParser(configuration);
            }

            if (domainNameCheckerOptions.UseClamAv)
            {
                services.SetupClamAV(configuration);
            }

            services.AddSingleton<IDomainNameCheckerHandler, DomainNameCheckerHandler>();
        }
    }
}