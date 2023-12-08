using ADC.PostNL.BuildingBlocks.Common;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Common;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Common.AVCheck;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Options;
using Microsoft.Extensions.Options;
using System.Net;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.Handlers
{
    public class DomainNameCheckerHandler : IDomainNameCheckerHandler
    {
        private DomainNameCheckerOptions DomainNameCheckerHandlerOptions { get; }
        private DomainNameParser DomainNameParser { get; }
        private IAVChecker ClamAVParserService { get; }

        public DomainNameCheckerHandler(IOptions<DomainNameCheckerOptions> domainNameCheckerHandlerOptions)
        {
            DomainNameCheckerHandlerOptions = domainNameCheckerHandlerOptions.Value;
            DomainNameParser = null;
            ClamAVParserService = null;
        }

        public DomainNameCheckerHandler(IOptions<DomainNameCheckerOptions> domainNameCheckerHandlerOptions, DomainNameParser domainNameParser, IAVChecker clamAVParserService)
        {
            DomainNameCheckerHandlerOptions = domainNameCheckerHandlerOptions.Value;
            DomainNameParser = domainNameParser;
            ClamAVParserService = clamAVParserService;
        }

        public DomainNameCheckerHandler(IOptions<DomainNameCheckerOptions> domainNameCheckerHandlerOptions, IAVChecker clamAVParserService)
        {
            DomainNameCheckerHandlerOptions = domainNameCheckerHandlerOptions.Value;
            DomainNameParser = null;
            ClamAVParserService = clamAVParserService;
        }

        public DomainNameCheckerHandler(IOptions<DomainNameCheckerOptions> domainNameCheckerHandlerOptions, DomainNameParser domainNameParser)
        {
            DomainNameCheckerHandlerOptions = domainNameCheckerHandlerOptions.Value;
            DomainNameParser = domainNameParser;
            ClamAVParserService = null;
        }

        public async Task<DomainNameCheckerAnalysis> ParseDomain(string url)
        {
            var result = new DomainNameCheckerAnalysis
            {
                OriginalUrl = url,
                LastRedirectionUrl = url,
                EndPointAnalisys = new List<EndPointInfo>(),
                AvResult = new AvResult { Result = ScanResult.Unknown }
            };

            var domainInfo = GeEndPointInfoAnalisys(url);
            result.EndPointAnalisys.Add(domainInfo);

            if (domainInfo.AdvancedDomainInfo.IsValid && !domainInfo.AdvancedDomainInfo.IsExcluded)
            {
                if (DomainNameCheckerHandlerOptions.CallUrl)
                {
                    var redirectResult = await ScanCallUrlAndGetLastRedirection(url).ConfigureAwait(false);
                    result.LastRedirectionUrl = redirectResult.Url;
                    result.AvResult = redirectResult.AvScanResult;
                    result.HttpStatusCode = redirectResult.HttpStatusCode;
                }

                if (!string.IsNullOrEmpty(result.LastRedirectionUrl) && result.LastRedirectionUrl != result.OriginalUrl && result.AvResult?.Result != ScanResult.VirusDetected && domainInfo.AdvancedDomainInfo.IsValid)
                {
                    result.EndPointAnalisys.Add(GeEndPointInfoAnalisys(result.LastRedirectionUrl));
                }
            }
            return result;
        }

        private EndPointInfo GeEndPointInfoAnalisys(string urlDomain)
        {
            var result = new EndPointInfo { BasicDomainValidation = false, AdvancedDomainInfo = new AdvancedDomainInfo { IsValid = false, IsExcluded = true } };

            if (DomainNameCheckerHandlerOptions.UseBasicDomainCheck)
            {
                result.BasicDomainValidation = CheckDomain(urlDomain);
            }

            if (DomainNameCheckerHandlerOptions.UseDomainNameParser)
            {
                result.AdvancedDomainInfo = DomainNameParser.TryParse(urlDomain);
            }

            return result;
        }

        private async Task<UrlAvResult> ScanCallUrlAndGetLastRedirection(string urlDomain)
        {
            if (string.IsNullOrEmpty(urlDomain)) return new UrlAvResult { Url = string.Empty, AvScanResult = new AvResult { Result = ScanResult.Unknown } };

            var result = new UrlAvResult { AvScanResult = new AvResult { Result = ScanResult.Unknown } };
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(DomainNameCheckerHandlerOptions.HttpRequestTimeout));
            HttpResponseMessage response = null;
            var redirectCount = 0;

            try
            {
                using var httpClient = new HttpClientCreator().GetHttpClient();
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                response = await httpClient.GetAsync(urlDomain, cts.Token).ConfigureAwait(false);

                // .NET does not allow redirections from https to http, this fixes that issue
                // https://github.com/dotnet/runtime/issues/23801
                // https://stackoverflow.com/questions/53575146/how-to-force-httpclient-to-follow-https-http-redirect
                while (redirectCount < 100 && response.StatusCode == HttpStatusCode.Moved)
                {
                    response = await httpClient.GetAsync(response.Headers.Location, cts.Token).ConfigureAwait(false);
                    redirectCount++;
                }

                response.EnsureSuccessStatusCode();

                if (DomainNameCheckerHandlerOptions.UseClamAv)
                {
                    using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    result.AvScanResult = await ClamAVParserService.ScanStream(stream).ConfigureAwait(false);
                }
                else
                {
                    result.AvScanResult = new AvResult { Result = ScanResult.Unknown, RawResult = "URL not scanned" };
                }

                result.Url = response.RequestMessage?.RequestUri?.ToString();
                result.HttpStatusCode = response.StatusCode;

                return result;
            }
            catch (HttpRequestException ex)
            {
                PostNLLogger.Error(ex);
                result.HttpStatusCode = default;

                return result;
            }
            catch (TaskCanceledException ex)
            {
                PostNLLogger.Error(ex);
                if (cts.Token.IsCancellationRequested)
                {
                    // Timed Out
                    result.HttpStatusCode = System.Net.HttpStatusCode.RequestTimeout;

                    return result;
                }

                throw;
            }
            catch (Exception ex)
            {
                PostNLLogger.Error(ex);
                throw;
            }
            finally
            {
                response?.Dispose();
            }
        }

        private static bool CheckDomain(string urlDomain)
        {
            return Uri.TryCreate(urlDomain, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}