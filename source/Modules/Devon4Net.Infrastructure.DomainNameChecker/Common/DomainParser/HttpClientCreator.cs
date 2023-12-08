using System.Net;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.Common
{
    public class HttpClientCreator : IDisposable
    {
        private bool _disposed;

        public HttpClientCreator()
        {
            CreateHttpClient();
        }

        private HttpClient HttpClient { get; set; }
        public HttpClient GetHttpClient()
        {
            return HttpClient;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void CreateHttpClient()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 100,
                CookieContainer = cookieContainer,
                UseProxy = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.All,
                UseCookies = true
            };

            HttpClient = new HttpClient(handler, true);
            HttpClient.DefaultRequestHeaders.Add(HttpHeadersConsts.UserAgentName, HttpHeadersConsts.UserAgentValue);
            HttpClient.DefaultRequestHeaders.Add(HttpHeadersConsts.AcceptName, HttpHeadersConsts.AcceptValue);
            HttpClient.DefaultRequestHeaders.Add(HttpHeadersConsts.Connection, HttpHeadersConsts.ConnectionValue);
            HttpClient.DefaultRequestHeaders.Add(HttpHeadersConsts.UpgradeInsecureRequests, HttpHeadersConsts.UpgradeInsecureRequestsValue);
            HttpClient.DefaultRequestHeaders.Add(HttpHeadersConsts.SecFetchSite, HttpHeadersConsts.SecFetchSiteValue);
            HttpClient.DefaultRequestHeaders.Add(HttpHeadersConsts.SecFetchMode, HttpHeadersConsts.SecFetchModeValue);
            HttpClient.DefaultRequestHeaders.Add(HttpHeadersConsts.SecFetchUser, HttpHeadersConsts.SecFetchUserValue);
            HttpClient.DefaultRequestHeaders.Add(HttpHeadersConsts.SecFetchDest, HttpHeadersConsts.SecFetchDestValue);
            HttpClient.Timeout = TimeSpan.FromSeconds(100);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                if(disposing)
                {
                    HttpClient.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
