using System.Collections.Generic;
using InfluencerInstaParser.Database.Serializer.Converters;
using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.Serializer
{
    public class ParameterSerializer
    {
        public static IList<Dictionary<string, object>> ToDictionary<TSourceType>(IList<TSourceType> source)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(source, settings);

            return JsonConvert.DeserializeObject<IList<Dictionary<string, object>>>(json,
                new CustomDictionaryConverter());
        }
    }
}