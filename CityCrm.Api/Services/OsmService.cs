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
            string query = $@"[out:json];
                area[""name""=""{cityName}""]->.searchArea;
                (
                  way[""addr:street""~""{streetName}""][""addr:housenumber""=""{houseNumber}""](area.searchArea);
                  relation[""addr:street""~""{streetName}""][""addr:housenumber""=""{houseNumber}""](area.searchArea);
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
    }
}