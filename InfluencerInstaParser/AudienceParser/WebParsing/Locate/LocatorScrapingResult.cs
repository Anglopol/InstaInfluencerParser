namespace InfluencerInstaParser.AudienceParser.WebParsing.Locate
{
    public class LocatorScrapingResult
    {
        public string Name { get; set; }
        public int PublicId { get; set; }
        public ulong InstagramId { get; set; }
        public double Distance { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
    }
}