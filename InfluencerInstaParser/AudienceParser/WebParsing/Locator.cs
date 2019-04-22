using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class Locator
    {
        private const double RadiusE = 6378135; // Equatorial radius
        private const double RadiusP = 6356750; // Polar radius
        private const double RadianConv = 180 / Math.PI;

        private readonly PageDownloaderProxy _proxy;
        private readonly PageContentScrapper _scrapper;
        private readonly string _userAgent;
        private readonly Logger _logger;
        private static Dictionary<string, KeyValuePair<double, double>> _cities;

        public Locator(PageDownloaderProxy downloaderProxy, PageContentScrapper scrapper, string userAgent)
        {
            _scrapper = scrapper;
            _proxy = downloaderProxy;
            _userAgent = userAgent;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public bool TryGetPostLocation(string pageContent, out string city)
        {
            if (_scrapper.IsLocationHasAddress(pageContent))
            {
                city = _scrapper.GetPostAddressLocation(pageContent);
                return true;
            }


            var locationUrl =
                $"/explore/locations/{_scrapper.GetLocationId(pageContent)}/{_scrapper.GetLocationSlug(pageContent)}/";
            _logger.Info($"Getting location from {locationUrl}");

            var locationPage = _proxy.GetPageContent(locationUrl, _userAgent);
            if (locationPage.Contains("\"city\":"))
            {
                _logger.Info($"Getting city from page content");
                city = _scrapper.GetCity(locationPage);
                return true;
            }

            city = "";
            return false;
        }

        public bool TryGetPostLocationByPoints(string pageContent, double maxDistance, out string city)
        {
            if (_cities == null) FillCities();
            var locationUrl =
                $"/explore/locations/{_scrapper.GetLocationId(pageContent)}/{_scrapper.GetLocationSlug(pageContent)}/";
            var locationPage = _proxy.GetPageContent(locationUrl, _userAgent);
            var cityLat = _scrapper.GetLocationLat(locationPage);
            var cityLong = _scrapper.GetLocationLong(locationPage);
            city = GetNearestCityByPoints(cityLat, cityLong, out var distance);
            return distance > maxDistance;
        }

        private string GetNearestCityByPoints(double cityLat, double cityLong, out double distance)
        {
            distance = double.MaxValue;
            var currentCity = "";
            foreach (var city in _cities)
            {
                var currentDistance = GetDistanceBetweenPoints(cityLat, cityLong,
                    city.Value.Key, city.Value.Value);
                if (!(distance < currentDistance)) continue;
                currentCity = city.Key;
                distance = currentDistance;
            }

            return currentCity;
        }

        private static double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
        {
            var dLat = (lat2 - lat1) / RadianConv;
            var dLong = (long2 - long1) / RadianConv;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat2) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            return Math.Sqrt(Math.Pow(RadiusE * RadiusP * Math.Cos(lat1 / RadianConv), 2) /
                             (Math.Pow(RadiusE * Math.Cos(lat1 / RadianConv), 2) +
                              Math.Pow(RadiusP * Math.Sin(lat1 / RadianConv), 2))) *
                   (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))) / 1000;
        }

        private void FillCities()
        {
            _cities = new Dictionary<string, KeyValuePair<double, double>>();
            var cityLines = File.ReadAllLines("citiesLocations.txt", Encoding.UTF8);
            foreach (var city in cityLines)
            {
                var cityParams = city.Split(":");
                var cityName = cityParams[0];
                var cityLat = double.Parse(cityParams[1]);
                var cityLong = double.Parse(cityParams[2]);
                _cities.Add(cityName, new KeyValuePair<double, double>(cityLat, cityLong));
            }
        }
    }
}