using iot_cloud_service_api.Models;
using System.Threading.Tasks;

namespace iot_cloud_service_api.Interfaces
{
    /// <summary>
    /// IAdafruitService interface defines the methods that an Adafruit service should implement.
    /// </summary>
    public interface IAdafruitService
    {
        /// <summary>
        /// Asynchronously gets the latest temperature and humidity data from the Adafruit API.
        /// </summary>
        /// <returns>A TempData object containing the latest temperature and humidity values.</returns>
        Task<TempData> GetLatestAsync();
    }
}
