using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using InfluencerInstaParser.AudienceParser.Proxy.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing
{
    public class Locator : ILocator
    {
        private static ConcurrentDictionary<string, HashSet<LocatorScrapingResult>> _cachedCities;
        private readonly ConcurrentDictionary<string, CityInformation> _citiesFromFile;
        private readonly IInstagramLocationPageScraper _scraper;
        private readonly IPageDownloader _pageDownloader;
        private const string CitiesFile = "citiesLocations.txt";

        private const double RadiusE = 6378135; // Equatorial radius, in metres
        private const double RadiusP = 6356750; // Polar Radius

        private class CityInformation
        {
            public int PublicId { get; set; }
            public double CityLat { get; set; }
            public double CityLong { get; set; }
        }


        public Locator(IInstagramLocationPageScraper scraper, IPageDownloader pageDownloader)
        {
            _citiesFromFile = new ConcurrentDictionary<string, CityInformation>();
            _cachedCities = _cachedCities ?? new ConcurrentDictionary<string, HashSet<LocatorScrapingResult>>();
            _scraper = scraper;
            _pageDownloader = pageDownloader;
            FillCities();
        }

        public LocatorScrapingResult GetLocatorScrapingResultByLocationId(ulong locationId)
        {
            if (IsCityAlreadyCached(locationId)) return GetCachedCity(locationId);
            var locationPage = GetLocationPageContentByLocationId(locationId);
            return IsLocationPageContentValid(locationPage)
                ? GetNearestCityByLocationPageContent(locationPage, locationId)
                : new LocatorScrapingResult();
        }

        private LocatorScrapingResult GetNearestCityByLocationPageContent(string locationPageContent, ulong locationId)
        {
            var cityLat = _scraper.GetLatitudeFromLocationPage(locationPageContent);
            var cityLong = _scraper.GetLongitudeFromLocationPage(locationPageContent);
            var cityName = GetNearestCityByPoints(cityLat, cityLong, out var distance);
            var cityPublicId = _citiesFromFile[cityName].PublicId;
            var result = new LocatorScrapingResult(cityName, cityPublicId, locationId, distance, cityLat, cityLong);
            CacheScrapingResult(result);
            return result;
        }

        private static void CacheScrapingResult(LocatorScrapingResult scrapingResult)
        {
            _cachedCities.TryAdd(scrapingResult.Name, new HashSet<LocatorScrapingResult>());
            _cachedCities[scrapingResult.Name].Add(scrapingResult);
        }

        private string GetLocationPageContentByLocationId(ulong locationId)
        {
            var locationUrl = MakeLocationUrl(locationId);
            var locationPage = _pageDownloader.GetPageContent(locationUrl);
            _pageDownloader.SetClientFree();
            return locationPage;
        }

        private void FillCities()
        {
            var cityLines = File.ReadAllLines(CitiesFile);
            foreach (var city in cityLines)
            {
                var cityParams = city.Split(":");
                var cityId = int.Parse(cityParams[0]);
                var cityName = cityParams[1];
                var cityLat = Convert.ToDouble(cityParams[2]);
                var cityLong = Convert.ToDouble(cityParams[3]);
                _citiesFromFile.TryAdd(cityName,
                    new CityInformation {CityLat = cityLat, CityLong = cityLong, PublicId = cityId});
            }
        }

        private string GetNearestCityByPoints(double cityLat, double cityLong, out double distance)
        {
            distance = double.MaxValue;
            var currentCity = "";
            foreach (var (cityName, cityInformation) in _citiesFromFile)
            {
                var currentDistance = GetDistanceBetweenPoints(cityLat, cityLong,
                    cityInformation.CityLat, cityInformation.CityLong);
                if (distance <= currentDistance) continue;
                currentCity = cityName;
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

        private static LocatorScrapingResult GetCachedCity(ulong cityId)
        {
            foreach (var (_, informationSet) in _cachedCities)
            {
                foreach (var information in informationSet)
                {
                    if (information.InstagramId != cityId) continue;
                    return information;
                }
            }

            return null;
        }

        private static bool IsCityAlreadyCached(ulong locationId)
        {
            foreach (var (_, informationSet) in _cachedCities)
            {
                foreach (var information in informationSet)
                {
                    if (information.InstagramId != locationId) continue;
                    return true;
                }
            }

            return false;
        }

        private static string MakeLocationUrl(ulong locationId)
        {
            return $"https://www.instagram.com/explore/locations/{locationId}/";
        }

        private bool IsLocationPageContentValid(string locationPageContent)
        {
            return locationPageContent.Contains("location:latitude") &&
                   locationPageContent.Contains("location:longitude");
        }
    }
}