using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jellee.Models
{
    public class GooglePlacesResponse
    {
        [JsonProperty("results")]
        public List<PlaceResult> Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class PlaceResult
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vicinity")]
        public string Vicinity { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }

        [JsonProperty("rating")]
        public double? Rating { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("photos")]
        public List<Photo> Photos { get; set; }
    }

    public class Geometry
    {
        [JsonProperty("location")]
        public Location Location { get; set; }
    }

    public class Location
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    public class Photo
    {
        [JsonProperty("photo_reference")]
        public string PhotoReference { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}