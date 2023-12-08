namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.DomainParser.Enum
{
    public enum RuleType
    {
        /// <summary>
        /// A normal rule
        /// </summary>
        Normal,

        /// <summary>
        /// A wildcard rule, as defined by www.publicsuffix.org
        /// </summary>
        Wildcard,

        /// <summary>
        /// An exception rule, as defined by www.publicsuffix.org
        /// </summary>
        Exception
    }
}
