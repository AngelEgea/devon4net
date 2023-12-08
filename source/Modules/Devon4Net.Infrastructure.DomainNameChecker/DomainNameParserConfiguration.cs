using Devon4Net.Infrastructure.Common.Handlers;
using Devon4Net.Infrastructure.DomainNameChecker.DomainParser;
using Devon4Net.Infrastructure.DomainNameChecker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devon4Net.Infrastructure.DomainNameChecker
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
