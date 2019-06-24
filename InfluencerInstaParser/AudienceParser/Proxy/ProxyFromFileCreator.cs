using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace InfluencerInstaParser.AudienceParser.Proxy
{
    public class ProxyFromFileCreator : IProxyCreatorSingleton
    {
        private readonly string _pathToProxyFile;

        public ProxyFromFileCreator(string pathToProxyFile)
        {
            _pathToProxyFile = pathToProxyFile;
        }

        public IEnumerable<IWebProxy> GetWebProxies()
        {
            return CreateProxies();
        }

        private IEnumerable<WebProxy> CreateProxies()
        {
            var proxyLines = File.ReadAllLines(_pathToProxyFile, Encoding.UTF8);

            return from line in proxyLines
                select line.Split(":")
                into proxyParams
                let proxyUri = $"http://{proxyParams[0]}:{proxyParams[1]}"
                let userName = proxyParams[2]
                let password = proxyParams[3]
                select new WebProxy
                {
                    Address = new Uri(proxyUri), BypassProxyOnLocal = true,
                    Credentials = new NetworkCredential(userName, password)
                };
        }
    }
}