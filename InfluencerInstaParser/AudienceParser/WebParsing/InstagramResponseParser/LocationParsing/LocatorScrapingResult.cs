namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing
{
    public class LocatorScrapingResult
    {
        public string Name { get; set; }
        public int PublicId { get; set; }
        public ulong InstagramId { get; set; }
        public double Distance { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public bool IsResultEmpty;

        public LocatorScrapingResult(string name, int publicId, ulong instagramId, double distance, double cityLat,
            double cityLong)
        {
            Name = name;
            PublicId = publicId;
            InstagramId = instagramId;
            Distance = distance;
            Lat = cityLat;
            Long = cityLong;
            IsResultEmpty = false;
        }

        public LocatorScrapingResult()
        {
            IsResultEmpty = true;
        }
    }
}