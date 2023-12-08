namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.Common
{
    public static class HttpHeadersConsts
    {
        public const string AcceptName = "Accept";
        public const string AcceptEncoding = "Accept-Encoding";
        public const string AcceptEncodingValue = " gzip, deflate, br";
        public const string AcceptValue = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
        public const string Connection = "Connection";
        public const string ConnectionValue = "keep-alive";
        public const string UpgradeInsecureRequests = "Upgrade-Insecure-Requests";
        public const string UpgradeInsecureRequestsValue = "1";
        public const string UserAgentName = "User-Agent";
        public const string UserAgentValue = "MMBE/1.0";

        public const string SecFetchSite = "Sec-Fetch-Site";
        public const string SecFetchMode = "Sec-Fetch-Mode";
        public const string SecFetchUser = "Sec-Fetch-User";
        public const string SecFetchDest = "Sec-Fetch-Dest";
        public const string SecFetchSiteValue = "same-origin";
        public const string SecFetchModeValue = " navigate";
        public const string SecFetchUserValue = "?1";
        public const string SecFetchDestValue = "document";
    }
}
