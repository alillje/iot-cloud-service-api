using System;

namespace iot_cloud_service_api.Interfaces
{
    /// <summary>
    /// ITempData represents the interface for temperature and humidity data points with a timestamp.
    /// </summary>
    public interface ITempData
    {
        /// <summary>
        /// Gets or sets the temperature value of the data point.
        /// </summary>
        double Temperature { get; set; }

        /// <summary>
        /// Gets or sets the humidity value of the data point.
        /// </summary>
        double Humidity { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the data point.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
