using Devon4Net.Infrastructure.Common;
using Devon4Net.Infrastructure.Common.IO;
using Devon4Net.Infrastructure.DomainNameChecker.Common.AVCheck;
using Devon4Net.Infrastructure.DomainNameChecker.Common.DomainParser;
using Devon4Net.Infrastructure.DomainNameChecker.Options;
using Microsoft.Extensions.Options;
using nClam;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace Devon4Net.Infrastructure.DomainNameChecker.ClamAVParser
{
    public class ClamAVParserService : IAVChecker
    {
        private const string FileNotDefined = "File not found or file path cannot be null or empty";
        private const string StreamNotDefined = "The provided stream cannot be null or empty";
        private const string UrlNotDefined = "URL cannot be null or empty";
        private const string ClamAVNotFound = "Error trying to connect to ClamAV. Please check ClamAV is  up and running";

        private ClamAVOptions ClamAVOptions { get; set; }
        private ClamClient ClamClient { get; set; }

        public ClamAVParserService(IOptions<ClamAVOptions> clamAvOptions)
        {
            ClamAVOptions = clamAvOptions.Value;
            ClamClient = new ClamClient(ClamAVOptions.Url, ClamAVOptions.Port);
        }

        public ClamAVParserService(string clamAvUrl, int clamAvPort)
        {
            ClamClient = new ClamClient(clamAvUrl, clamAvPort);
        }

        public ClamAVParserService(IOptions<ClamAVOptions> clamAvOptions, ClamClient clamClient)
        {
            ClamAVOptions = clamAvOptions.Value;
            ClamClient = clamClient;
        }

        public async Task<AvResult> ScanStream(Stream inputStream)
        {
            if (inputStream == null) return new AvResult { Result = ScanResult.Error, RawResult = StreamNotDefined };

            try
            {
                ClamScanResult scanResult = await ClamClient.SendAndScanFileAsync(inputStream).ConfigureAwait(false);
                return new AvResult
                {
                    RawResult = scanResult.RawResult,
                    Result = GetScanResult(scanResult.Result),
                    InfectedFiles = GetInfectedFiles(scanResult.InfectedFiles)
                };
            }
            catch (SocketException ex)
            {
                Devon4NetLogger.Error(ClamAVNotFound);
                Devon4NetLogger.Error(ex);
                throw;
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error(ex);
                throw;
            }
        }

        public async Task<AvResult> ScanFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) return new AvResult { Result = ScanResult.Error, RawResult = FileNotDefined };

                ClamScanResult scanResult = await ClamClient.ScanFileOnServerAsync(FileOperations.GetFileFullPath(filePath)).ConfigureAwait(false);
                return new AvResult
                {
                    RawResult = scanResult.RawResult,
                    Result = (ScanResult)Enum.Parse(typeof(AvResult), scanResult.Result.ToString()),
                    InfectedFiles = GetInfectedFiles(scanResult.InfectedFiles)
                };
            }
            catch (SocketException ex)
            {
                Devon4NetLogger.Error(ClamAVNotFound);
                Devon4NetLogger.Error(ex);
                throw;
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error(ex);
                throw;
            }
        }

        public async Task<AvResult> ScanUrl(string url)
        {
            try
            {
                using var httpClient = new HttpClientCreator().GetHttpClient();
                if (string.IsNullOrEmpty(url)) return new AvResult { Result = ScanResult.Error, RawResult = UrlNotDefined };
                using var response = await httpClient.GetAsync(url).ConfigureAwait(false);
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                ClamScanResult scanResult = await ClamClient.SendAndScanFileAsync(stream).ConfigureAwait(false);

                return new AvResult
                {
                    RawResult = scanResult.RawResult,
                    Result = GetScanResult(scanResult.Result),
                    InfectedFiles = GetInfectedFiles(scanResult.InfectedFiles)
                };
            }
            catch (SocketException ex)
            {
                Devon4NetLogger.Error(ClamAVNotFound);
                Devon4NetLogger.Error(ex);
                throw;
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error(ex);
                throw;
            }
        }

        private static ScanResult GetScanResult(ClamScanResults clamScanResult)
        {
            var result = clamScanResult.ToString().ToLower();
            return result switch
            {
                "unknown" => ScanResult.Unknown,
                "clean" => ScanResult.Clean,
                "virusdetected" => ScanResult.VirusDetected,
                "error" => ScanResult.Error,
                _ => ScanResult.Unknown,
            };
        }

        private static List<Infectedfile> GetInfectedFiles(ReadOnlyCollection<ClamScanInfectedFile> infectedFiles)
        {
            if (infectedFiles == null || infectedFiles.Count == 0)
            {
                return new List<Infectedfile>();
            }

            var result = new List<Infectedfile>();
            foreach (var infectedFile in infectedFiles)
            {
                result.Add(new Infectedfile
                {
                    FileName = infectedFile.FileName,
                    VirusName = infectedFile.VirusName
                });
            }

            return result;
        }
    }
}
