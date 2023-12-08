namespace ADC.PostNL.BuildingBlocks.DomainNameChecker.Common.AVCheck
{
    public class AvResult
    {
        public string RawResult { get; set; }
        public ScanResult Result { get; set; }
        public List<Infectedfile> InfectedFiles { get; set; }
    }

    public class Infectedfile
    {
        public string FileName { get; set; }
        public string VirusName { get; set; }
    }

}
