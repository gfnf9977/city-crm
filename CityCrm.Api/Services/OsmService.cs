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

        public async Task<Geometry?> GetBuildingGeometryAsync(string cityName, string streetName, string houseNumber)
        {

            string query = $@"[out:json];
                area[""name""=""{cityName}""]->.searchArea;
                way[""addr:street""~""{streetName}""][""addr:housenumber""=""{houseNumber}""](area.searchArea);
                out geom;";

            var response = await _httpClient.GetAsync($"https://overpass-api.de/api/interpreter?data={Uri.EscapeDataString(query)}");
            
            if (!response.IsSuccessStatusCode) return null;

            var jsonString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonString);

            var elements = json["elements"] as JArray;
            if (elements == null || elements.Count == 0) return null;

            var way = elements[0];
            var geometry = way["geometry"] as JArray;
            if (geometry == null) return null;

            var coordinates = new List<Coordinate>();
            foreach (var point in geometry)
            {
                var lat = (double)point["lat"]!;
                var lon = (double)point["lon"]!;
                coordinates.Add(new Coordinate(lon, lat)); 
            }

            var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            
            if (coordinates.Count >= 4 && coordinates.First().Equals2D(coordinates.Last()))
            {
                var linearRing = factory.CreateLinearRing(coordinates.ToArray());
                return factory.CreatePolygon(linearRing);
            }
            
            return factory.CreateLineString(coordinates.ToArray());
        }
    }
}