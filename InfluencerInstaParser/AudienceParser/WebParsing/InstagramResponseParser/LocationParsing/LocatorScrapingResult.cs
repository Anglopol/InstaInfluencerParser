namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing
{
    public class LocatorScrapingResult
    {
        public string Name { get; }
        public int PublicId { get; }
        public ulong InstagramId { get; }
        public double Distance { get; }
        public double Lat { get; }
        public double Long { get; }
        public bool IsResultEmpty { get; }


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