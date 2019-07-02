namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.LocationParsing
{
    public class LocatorScrapingResult
    {
        public string Name { get; set; }
        public int PublicId { get; set; }
        public ulong InstagramId { get; set; }
        public double Distance { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }

        public LocatorScrapingResult(string name, int publicId, ulong instagramId, double distance, double cityLat,
            double cityLong)
        {
            Name = name;
            PublicId = publicId;
            InstagramId = instagramId;
            Distance = distance;
            Lat = cityLat;
            Long = cityLong;
        }
    }
}