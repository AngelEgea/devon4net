using ADC.PostNL.BuildingBlocks.Common;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser.Enum;
using ADC.PostNL.BuildingBlocks.DomainNameChecker.Options;
using System.Data;
using System.Text.RegularExpressions;

namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser
{
    public class DomainNameParser
    {
        private TopLevelDomainList TopLevelDomainList { get; set; }

        public DomainNameParser(TopLevelDomainList topLevelDomainList)
        {
            TopLevelDomainList = topLevelDomainList;
        }

        public DomainNameParser(DomainNameParserOptions domainNameCheckerOptions)
        {
            TopLevelDomainList = new TopLevelDomainList(domainNameCheckerOptions);
        }

        /// <summary>
        /// Converts the string representation of a domain to it's 3 distinct components: 
        /// Top Level Domain (TLD), Second Level Domain (SLD), and subdomain information
        /// </summary>
        /// <param name="domainString">The domain to parse</param>
        /// <param name="TLD"></param>
        /// <param name="SLD"></param>
        /// <param name="SubDomain"></param>
        /// <param name="MatchingRule"></param>
        private void ParseDomainName(string domainString, out string TLD, out string SLD, out string SubDomain, out TopLevelDomainRule MatchingRule)
        {
            TLD = string.Empty;
            SLD = string.Empty;
            SubDomain = string.Empty;
            MatchingRule = null;

            //  If the fqdn is empty, we have a problem already
            if (domainString.Trim()?.Length == 0)
                throw new ArgumentException("The domain cannot be blank");

            //  Next, find the matching rule:
            MatchingRule = FindMatchingTLDRule(domainString);

            //  At this point, no rules match, we have a problem
            if (MatchingRule == null)
                throw new FormatException("The domain does not have a recognized TLD");

            //  Based on the tld rule found, get the domain (and possibly the subdomain)
            string tempSudomainAndDomain = string.Empty;
            int tldIndex;

            //  First, determine what type of rule we have, and set the TLD accordingly
            switch (MatchingRule.Type)
            {
                case RuleType.Normal:
                    tldIndex = domainString.LastIndexOf("." + MatchingRule.Name, StringComparison.InvariantCultureIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
                case RuleType.Wildcard:
                    //  This finds the last portion of the TLD...
                    tldIndex = domainString.LastIndexOf("." + MatchingRule.Name, StringComparison.InvariantCultureIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);

                    //  But we need to find the wildcard portion of it:
                    tldIndex = tempSudomainAndDomain.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
                case RuleType.Exception:
                    tldIndex = domainString.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
            }

            //  See if we have a subdomain:
            var lstRemainingParts = new List<string>(tempSudomainAndDomain.Split('.'));

            //  If we have 0 parts left, there is just a tld and no domain or subdomain
            //  If we have 1 part, it's the domain, and there is no subdomain
            //  If we have 2+ parts, the last part is the domain, the other parts (combined) are the subdomain
            if (lstRemainingParts.Count > 0)
            {
                //  Set the domain:
                SLD = lstRemainingParts[lstRemainingParts.Count - 1];

                //  Set the subdomain, if there is one to set:
                if (lstRemainingParts.Count > 1)
                {
                    //  We strip off the trailing period, too
                    SubDomain = tempSudomainAndDomain.Substring(0, tempSudomainAndDomain.Length - SLD.Length - 1);
                }
            }
        }

        private static string RemoveMainProtocol(string domainString)
        {
            var domain = new Uri(domainString).GetLeftPart(UriPartial.Authority);
            return Regex.Replace(domain.ToLower(), "^https?:\\/\\/(www\\.)?", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Converts the string representation of a domain to its DomainName equivalent.  A return value
        /// indicates whether the operation succeeded.
        /// </summary>
        /// <param name="domainString"></param>
        /// <returns></returns>
        public AdvancedDomainInfo TryParse(string domainString)
        {
            try
            {
                if (domainString.Trim()?.Length == 0) throw new ArgumentException("The domain cannot be blank");
                var domain = RemoveMainProtocol(domainString);
                ParseDomainName(domain, out string tld, out string sld, out string subdomain, out TopLevelDomainRule tldrule);

                var isExcludedDomain = false;

                if (TopLevelDomainList.GetExcludedTopLevelDomainRules() != null && !string.IsNullOrEmpty(tld) && TopLevelDomainList.GetExcludedTopLevelDomainRules().Contains(tld))
                    isExcludedDomain = true;

                //  Construct a new DomainName object and return it
                return new AdvancedDomainInfo
                {
                    IsValid = true,
                    IsExcluded = isExcludedDomain,
                    SLD = sld,
                    SubDomain = subdomain,
                    TLD = tld,
                    TopLevelDomainRule = tldrule
                };
            }
            catch (Exception ex)
            {
                PostNLLogger.Error(ex);
                return new AdvancedDomainInfo { IsValid = false };
            }
        }

        /// <summary>
        /// Finds matching rule for a domain.  If no rule is found, 
        /// returns a null TLDRule object
        /// </summary>
        /// <param name="domainString"></param>
        /// <returns></returns>
        private TopLevelDomainRule FindMatchingTLDRule(string domainString)
        {
            //  Split our domain into parts (based on the '.')
            //  Put these parts in a list
            //  Make sure these parts are in reverse order (we'll be checking rules from the right-most pat of the domain)
            List<string> lstDomainParts = domainString.Split('.').ToList<string>();
            lstDomainParts.Reverse();

            //  Begin building our partial domain to check rules with:
            string checkAgainst = string.Empty;

            //  Our 'matches' collection:
            var ruleMatches = new List<TopLevelDomainRule>();

            foreach (string domainPart in lstDomainParts)
            {
                //  Add on our next domain part:
                checkAgainst = string.Format("{0}.{1}", domainPart, checkAgainst);

                //  If we end in a period, strip it off:
                if (checkAgainst.EndsWith("."))
                    checkAgainst = checkAgainst.Substring(0, checkAgainst.Length - 1);

                if (TopLevelDomainList == null) TopLevelDomainList = new TopLevelDomainList();

                foreach (RuleType rule in System.Enum.GetValues(typeof(RuleType)).Cast<RuleType>())
                {
                    //  Try to match rule:
                    if (TopLevelDomainList.GetTopLevelDomainRules()[rule].TryGetValue(checkAgainst, out TopLevelDomainRule result))
                    {
                        ruleMatches.Add(result);
                    }
                    var matched = result == null ? 0 : 1;
                    PostNLLogger.Information($"Domain part {checkAgainst} matched {matched} {rule} rules");
                }
            }

            //  Sort our matches list (longest rule wins, according to :
            var results = from match in ruleMatches
                          orderby match.Name.Length descending
                          select match;

            //  Take the top result (our primary match):
            var primaryMatch = results.Take(1).SingleOrDefault();

            if (primaryMatch != null)
            {
                PostNLLogger.Information($"Looks like our match is: {primaryMatch.Name}, which is a(n) {primaryMatch.Type} rule.");
            }
            else
            {
                PostNLLogger.Information($"No rules matched domain: {domainString}");
            }

            return primaryMatch;
        }
    }

}
