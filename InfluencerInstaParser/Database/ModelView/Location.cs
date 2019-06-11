using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.ModelView
{
    public class Location
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("countOfUsers")] public int CountOfUsers { get; set; }
        [JsonProperty("publicId")] public int CityId { get; set; }

        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("audienceFrom")] public string Owner { get; set; }
        [JsonProperty("date")] public string DateOfParsing { get; set; }


        protected bool Equals(Location other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Owner, other.Owner);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Owner != null ? Owner.GetHashCode() : 0);
            }
        }
    }
}