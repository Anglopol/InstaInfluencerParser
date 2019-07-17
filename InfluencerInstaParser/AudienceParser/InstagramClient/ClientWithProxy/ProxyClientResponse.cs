using System.Net;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy
{
    public class ProxyClientResponse
    {
        public string PageContent { get; }
        public HttpStatusCode Code { get; }
        

        public ProxyClientResponse(string pageContent, HttpStatusCode code)
        {
            PageContent = pageContent;
            Code = code;
        }
    }
}