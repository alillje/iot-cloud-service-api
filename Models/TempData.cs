using System.Text.Json.Serialization;
using iot_cloud_service_api.Interfaces;

namespace iot_cloud_service_api.Models
{
    /// <summary>
    /// TempData represents a single temperature and humidity data point with a timestamp.
    /// </summary>
    public class TempData : ITempData
    {
        /// <summary>
        /// Gets or sets the temperature value of the data point.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        /// <summary>
        /// Gets or sets the humidity value of the data point.
        /// </summary>
        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the data point.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
