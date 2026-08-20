using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json.Linq;

namespace CityCrm.Api.Services
{
    public class OsmService
    {
        private readonly HttpClient _httpClient;

        public OsmService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CityCrm_StudentProject/1.0");
        }

        public async Task<(Geometry? Geometry, string? ErrorMessage)> GetBuildingGeometryAsync(string cityName, string streetName, string houseNumber)
        {
            string cleanNumber = houseNumber.Trim();

            var match = System.Text.RegularExpressions.Regex.Match(cleanNumber, @"^(\d+)(.*)$");
            string osmRegex;

            if (match.Success && !string.IsNullOrEmpty(match.Groups[2].Value))
            {
                string digits = match.Groups[1].Value;
                string tail = System.Text.RegularExpressions.Regex.Escape(match.Groups[2].Value.TrimStart('-', ' '));
                osmRegex = $"^{digits}[\\s-]?{tail}$";
            }
            else
            {
                osmRegex = $"^{cleanNumber}$";
            }

            string query = $@"[out:json];
                area[""name""=""{cityName}""]->.searchArea;
                (
                  way[""addr:street""~""{streetName}"", i][""addr:housenumber""~""{osmRegex}"", i](area.searchArea);
                  relation[""addr:street""~""{streetName}"", i][""addr:housenumber""~""{osmRegex}"", i](area.searchArea);
                );
                out geom;";

            string[] endpoints = {
                "https://overpass-api.de/api/interpreter",
                "https://overpass.openstreetmap.fr/api/interpreter",
                "https://overpass.kumi.systems/api/interpreter"
            };

            string? lastErrorMessage = "ERROR";

            foreach (var endpoint in endpoints)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{endpoint}?data={Uri.EscapeDataString(query)}");

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        lastErrorMessage = "RATE_LIMIT";
                        continue;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        if (!jsonString.TrimStart().StartsWith("{")) continue;

                        var json = JObject.Parse(jsonString);
                        var elements = json["elements"] as JArray;

                        if (elements == null || elements.Count == 0)
                            return (null, null);

                        return ParseGeometry(elements);
                    }
                }
                catch
                {
                    continue;
                }
            }

            return (null, lastErrorMessage);
        }

        private (Geometry? Geometry, string? ErrorMessage) ParseGeometry(JArray elements)
        {
            var element = elements[0];
            var type = element["type"]?.ToString();
            var coordinates = new List<Coordinate>();

            if (type == "way")
            {
                var geom = element["geometry"] as JArray;
                if (geom != null)
                {
                    foreach (var point in geom)
                        coordinates.Add(new Coordinate((double)point["lon"]!, (double)point["lat"]!));
                }
            }
            else if (type == "relation")
            {
                var members = element["members"] as JArray;
                if (members != null)
                {
                    var outerWay = members.FirstOrDefault(m => m["role"]?.ToString() == "outer" && m["type"]?.ToString() == "way");
                    if (outerWay != null)
                    {
                        var geom = outerWay["geometry"] as JArray;
                        if (geom != null)
                        {
                            foreach (var point in geom)
                                coordinates.Add(new Coordinate((double)point["lon"]!, (double)point["lat"]!));
                        }
                    }
                }
            }

            if (coordinates.Count == 0) return (null, null);

            var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            if (coordinates.Count >= 4 && coordinates.First().Equals2D(coordinates.Last()))
            {
                return (factory.CreatePolygon(coordinates.ToArray()), null);
            }

            return (factory.CreateLineString(coordinates.ToArray()), null);
        }

        public async Task<(double Lat, double Lng)?> GetAddressCoordinatesAsync(string cityName, string streetName, string houseNumber)
{
    try
    {
        string query = $"{houseNumber} {streetName}, {cityName}";
        string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
        
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var json = Newtonsoft.Json.Linq.JArray.Parse(jsonString);
            
            if (json.Count > 0)
            {
                double lat = (double)json[0]["lat"]!;
                double lon = (double)json[0]["lon"]!;
                return (lat, lon);
            }
        }
    }
    catch { }
    
    return null;
}
    }
}