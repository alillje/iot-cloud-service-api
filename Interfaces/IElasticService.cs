using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iot_cloud_service_api.Models;

namespace iot_cloud_service_api.Interfaces
{
    /// <summary>
    /// Interface for a service that interacts with an Elasticsearch index containing temperature and humidity data.
    /// </summary>
    public interface IElasticService
    {
        /// <summary>
        /// Retrieves a list of TempData objects from Elasticsearch.
        /// </summary>
        /// <returns>An IEnumerable of TempData objects.</returns>
        Task<IEnumerable<TempData>> GetAsync();

        /// <summary>
        /// Adds a TempData object to Elasticsearch.
        /// </summary>
        /// <param name="tempData">The TempData object to be added.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task AddAsync(TempData tempData);

        /// <summary>
        /// Retrieves the latest TempData object from Elasticsearch.
        /// </summary>
        /// <returns>The latest TempData object.</returns>
        Task<TempData> GetLatestAsync();

        /// <summary>
        /// Retrieves the average temperature and humidity data for a given period, grouped by hour or day.
        /// </summary>
        /// <param name="days">The number of days or hours to look back, depending on the value of the 'hourly' parameter.</param>
        /// <param name="hourly">If true, the data will be grouped by hour; if false, the data will be grouped by day.</param>
        /// <returns>A dictionary containing the average temperature and humidity data for the specified period.</returns>
        Task<Dictionary<DateTime, Dictionary<String, double>>> GetAverageTempData(int days, bool hourly);
    }
}
