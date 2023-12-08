using Devon4Net.Infrastructure.DomainNameChecker.DomainParser.Enum;

namespace Devon4Net.Infrastructure.DomainNameChecker.DomainParser
{
    /// <summary>
    /// Meta information class for an individual TLD rule
    /// </summary>
    public class TopLevelDomainRule
    {
        public string Name { get; }
        public RuleType Type { get; }

        /// <summary>
        /// Construct a TLDRule based on a single line from
        /// the www.publicsuffix.org list
        /// </summary>
        /// <param name="RuleInfo"></param>
        public TopLevelDomainRule(string RuleInfo)
        {
            if (RuleInfo.StartsWith("*", StringComparison.InvariantCultureIgnoreCase))
            {
                Type = RuleType.Wildcard;
                Name = RuleInfo.Substring(2);
            }
            else if (RuleInfo.StartsWith("!", StringComparison.InvariantCultureIgnoreCase))
            {
                Type = RuleType.Exception;
                Name = RuleInfo.Substring(1);
            }
            else
            {
                Type = RuleType.Normal;
                Name = RuleInfo;
            }
        }
    }
}
