using Devon4Net.Infrastructure.Common.Handlers;
using Devon4Net.Infrastructure.DomainNameChecker.ClamAVParser;
using Devon4Net.Infrastructure.DomainNameChecker.Common.AVCheck;
using Devon4Net.Infrastructure.DomainNameChecker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nClam;

namespace Devon4Net.Infrastructure.DomainNameChecker
{
    public static class ClamAVConfiguration
    {
        public static void SetupClamAV(this IServiceCollection services, IConfiguration configuration)
        {
            var clamAVOptions = services.GetTypedOptions<ClamAVOptions>(configuration, "ClamAV");
            if (clamAVOptions == null || clamAVOptions?.UseClamAV != true) return;

            services.AddTransient(_ => new ClamClient(clamAVOptions.Url, clamAVOptions.Port));
            services.AddTransient<IAVChecker, ClamAVParserService>();
        }
    }
}
