using System.Text.Json.Serialization;
using iot_cloud_service_api.Interfaces;

namespace iot_cloud_service_api.Models;


    public class TempData : ITempData
    {

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

