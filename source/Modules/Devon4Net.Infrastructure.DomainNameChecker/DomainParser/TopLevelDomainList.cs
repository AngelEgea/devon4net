using ADC.PostNL.BuildingBlocks.Common;
using ADC.PostNL.BuildingBlocks.Common.IO;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Common;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser.Enum;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Options;
using System.Reflection;
using System.Text;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser
{
    public class TopLevelDomainList
    {
        private TldRuleLoad TLDRuleLoad { get; set; }
        private DomainNameParserOptions DomainNameCheckerOptions { get; set; }
        private IEnumerable<string> TopLevelDomainRulesList { get; set; }
        private IEnumerable<string> ExcludedTopLevelDomainList { get; set; }

        private IDictionary<RuleType, IDictionary<string, TopLevelDomainRule>> TopLevelDomainRules { get; set; }

        public TopLevelDomainList()
        {
            DomainNameCheckerOptions = new DomainNameParserOptions
            {
                UseDomainNameParser = true,
                AdvancedDomainCheck = new AdvancedDomaincheck
                {
                    UseEmbebedTopLevelDomainList = true
                }
            };

            Setup();
        }

        public TopLevelDomainList(string topLevelDomainfilePath)
        {
            DomainNameCheckerOptions = new DomainNameParserOptions
            {
                UseDomainNameParser = true,
                AdvancedDomainCheck = new AdvancedDomaincheck
                {
                    LocalTopLevelDomainList = new LocalTopLevelDomainList
                    {
                        UseLocalTopLevelDomainList = true,
                        LocalTopLevelDomainListPath = topLevelDomainfilePath
                    }
                }
            };

            Setup();
        }

        public TopLevelDomainList(Uri topLevelDomainfileUrl)
        {
            DomainNameCheckerOptions = new DomainNameParserOptions
            {
                UseDomainNameParser = true,
                AdvancedDomainCheck = new AdvancedDomaincheck
                {
                    OnlineTopLevelDomainList = new Onlinetopleveldomainlist
                    {
                        UseOnlineTopLevelDomainList = true,
                        TopLevelDomainListUrl = topLevelDomainfileUrl.AbsoluteUri
                    }
                }
            };

            Setup();
        }

        public TopLevelDomainList(DomainNameParserOptions domainNameCheckerOptions)
        {
            DomainNameCheckerOptions = domainNameCheckerOptions;
            Setup();
        }

        public IDictionary<RuleType, IDictionary<string, TopLevelDomainRule>> GetTopLevelDomainRules()
        {
            return TopLevelDomainRules;
        }

        public IEnumerable<string> GetExcludedTopLevelDomainRules()
        {
            return ExcludedTopLevelDomainList;
        }

        private void Setup()
        {
            SetLoadRulesMethod();
            LoadTopLevelDomainList();
            LoadExcludedTopLevelDomainList();
            GetTLDRules();
        }

        private void LoadTopLevelDomainList()
        {
            TopLevelDomainRulesList = TLDRuleLoad switch
            {
                TldRuleLoad.Embedded => GetEmbeddedRulesData(),
                TldRuleLoad.Local => GetRulesDataFromLocalFile(),
                TldRuleLoad.Url => GetRulesDataFromUrl(),
                TldRuleLoad.NoRuleSet => new List<string>(),
                _ => GetEmbeddedRulesData(),
            };
        }

        private void LoadExcludedTopLevelDomainList()
        {
            ExcludedTopLevelDomainList = DomainNameCheckerOptions?.AdvancedDomainCheck?.ExcludedDomainList;
        }

        private static IEnumerable<string> GetEmbeddedRulesData()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream("ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser.PublicDomainData.effective_tld_names.dat");
            using var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }

        private IEnumerable<string> GetRulesDataFromLocalFile()
        {
            if (string.IsNullOrEmpty(DomainNameCheckerOptions.AdvancedDomainCheck.LocalTopLevelDomainList.LocalTopLevelDomainListPath))
            {
                TLDRuleLoad = TldRuleLoad.NoRuleSet;
                yield return string.Empty;
            }

            var fileName = FileOperations.GetFileFullPath(DomainNameCheckerOptions.AdvancedDomainCheck.LocalTopLevelDomainList.LocalTopLevelDomainListPath);
            foreach (var line in File.ReadAllLines(fileName, Encoding.UTF8))
                yield return line;
        }

        private IEnumerable<string> GetRulesDataFromUrl()
        {
            if (string.IsNullOrEmpty(DomainNameCheckerOptions.AdvancedDomainCheck.OnlineTopLevelDomainList.TopLevelDomainListUrl))
            {
                TLDRuleLoad = TldRuleLoad.NoRuleSet;
                yield return string.Empty;
            }

            using var httpClient = new HttpClientCreator().GetHttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            var datFile = httpClient.GetStreamAsync(DomainNameCheckerOptions.AdvancedDomainCheck.OnlineTopLevelDomainList.TopLevelDomainListUrl).Result;
            using var reader = new StreamReader(datFile);
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }

        private void SetLoadRulesMethod()
        {
            if (DomainNameCheckerOptions?.UseDomainNameParser == true)
            {
                if (DomainNameCheckerOptions.AdvancedDomainCheck.LocalTopLevelDomainList?.UseLocalTopLevelDomainList == true)
                {
                    TLDRuleLoad = TldRuleLoad.Local;
                }
                else
                    if (DomainNameCheckerOptions.AdvancedDomainCheck.OnlineTopLevelDomainList?.UseOnlineTopLevelDomainList == true)
                {
                    TLDRuleLoad = TldRuleLoad.Url;
                }
                else
                {
                    TLDRuleLoad = TldRuleLoad.Embedded;
                }
            }
            else
            {
                TLDRuleLoad = TldRuleLoad.NoRuleSet;
            }
        }

        private void GetTLDRules()
        {
            TopLevelDomainRules = new Dictionary<RuleType, IDictionary<string, TopLevelDomainRule>>();
            var rules = System.Enum.GetValues(typeof(RuleType)).Cast<RuleType>();
            foreach (var rule in rules)
            {
                TopLevelDomainRules[rule] = new Dictionary<string, TopLevelDomainRule>(StringComparer.InvariantCultureIgnoreCase);
            }

            var ruleStrings = TopLevelDomainRulesList;

            //  Strip out any lines that are:
            //  a.) A comment
            //  b.) Blank
            foreach (var ruleString in ruleStrings.Where(ruleString => !ruleString.StartsWith("//", StringComparison.InvariantCultureIgnoreCase) && ruleString.Trim().Length != 0))
            {
                var result = new TopLevelDomainRule(ruleString);
                TopLevelDomainRules[result.Type][result.Name] = result;
            }
            PostNLLogger.Information($"Loaded {TopLevelDomainRules.Values.Sum(r => r.Values.Count)} rules into cache.");
        }
    }
}