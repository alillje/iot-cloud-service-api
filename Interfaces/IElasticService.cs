using iot_cloud_service_api.Models;

namespace iot_cloud_service_api.Interfaces
{
    /// <summary>
    /// Interface for a service that interacts with an Elasticsearch index containing Github repository data.
    /// </summary>
    public interface IElasticService
    {
        public Task<IEnumerable<TempData>> GetAsync();
        public Task AddAsync(TempData tempData);
        public Task<TempData> GetLatestAsync();
        public Task<Dictionary<DateTime, double>> GetAverageTempData(int days);


    }
}
