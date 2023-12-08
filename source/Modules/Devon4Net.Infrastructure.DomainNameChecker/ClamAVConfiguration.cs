using ADC.PostNL.BuildingBlocks.Common.Handlers;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.ClamAVParser;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Common.AVCheck;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nClam;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker
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
