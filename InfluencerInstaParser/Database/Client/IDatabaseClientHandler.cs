using InfluencerInstaParser.Database.DataClasses;

namespace InfluencerInstaParser.Database.Client
{
    public interface IDatabaseClientHandler
    {
        void CreateAnalysis(IModel model);
    }
}