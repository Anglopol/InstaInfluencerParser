using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class Locator
    {
        private const double RadiusE = 6378135; // Equatorial radius, in metres
        private const double RadiusP = 6356750; // Polar Radius

        private readonly PageDownloaderProxy _proxy;
        private readonly PageContentScrapper _scrapper;
        private readonly string _userAgent;
        private readonly Logger _logger;
        private static ConcurrentDictionary<string, KeyValuePair<double, double>> _cities;
        private static ConcurrentDictionary<string, HashSet<int>> _cachedCities;

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
            var locationId = _scrapper.GetLocationId(pageContent);
            foreach (var (key, value) in _cachedCities)
            {
                if (!value.Contains(int.Parse(locationId))) continue;
                city = key;
                return true;
            }

            var locationUrl =
                $"/explore/locations/{locationId}/{_scrapper.GetLocationSlug(pageContent)}/";
            var locationPage = _proxy.GetPageContent(locationUrl, _userAgent);
            city = "";
            if (!locationPage.Contains("location:latitude") || !locationPage.Contains("location:longitude"))
                return false;
            var cityLat = _scrapper.GetLocationLat(locationPage);
            var cityLong = _scrapper.GetLocationLong(locationPage);
            city = GetNearestCityByPoints(cityLat, cityLong, out var distance);
            if (!(distance < maxDistance)) return false;
            _cachedCities.TryAdd(city, new HashSet<int>());
            _cachedCities[city].Add(int.Parse(locationId));
            return true;
        }

        private string GetNearestCityByPoints(double cityLat, double cityLong, out double distance)
        {
            distance = double.MaxValue;
            var currentCity = "";
            foreach (var city in _cities)
            {
                var currentDistance = GetDistanceBetweenPoints(cityLat, cityLong,
                    city.Value.Key, city.Value.Value);
                if (distance <= currentDistance) continue;
                currentCity = city.Key;
                distance = currentDistance;
            }

            return currentCity;
        }

        private static double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
        {
            var dLat = (lat2 - lat1) / 180 * Math.PI;
            var dLong = (long2 - long1) / 180 * Math.PI;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(lat1 / 180 * Math.PI) * Math.Cos(lat2 / 180 * Math.PI)
                                                     * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //Numerator part of function
            var numerator = Math.Pow(RadiusE * RadiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            //Denominator part of the function
            var denominator = Math.Pow(RadiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                              + Math.Pow(RadiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            var radius = Math.Sqrt(numerator / denominator);
            return radius * c; // distance in meters
        }

        private static void FillCities()
        {
            _cities = new ConcurrentDictionary<string, KeyValuePair<double, double>>();
            _cachedCities = new ConcurrentDictionary<string, HashSet<int>>();
            var cityLines = File.ReadAllLines("citiesLocations.txt", Encoding.UTF8);
            foreach (var city in cityLines)
            {
                var cityParams = city.Split(":");
                var cityName = cityParams[0];
                var cityLat = double.Parse(cityParams[1]);
                var cityLong = double.Parse(cityParams[2]);
                _cities.TryAdd(cityName, new KeyValuePair<double, double>(cityLat, cityLong));
            }
        }
    }
}