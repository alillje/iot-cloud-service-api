using Nest;
using iot_cloud_service_api.Interfaces;
using iot_cloud_service_api.Models;
using System.Text.Json;

namespace iot_cloud_service_api.Services
{
    /// <summary>
    /// The AdafruitService class is responsible for interacting with the Adafruit API to retrieve temperature and humidity data.
    /// </summary>
    public class AdafruitService : IAdafruitService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;



        /// <summary>
        /// Initializes a new instance of the AdafruitService class with the provided HttpClientFactory, and IConfiguration.
        /// </summary>
        /// <param name="httpClientFactory">An instance of the IHttpClientFactory interface for creating an HttpClient.</param>
        /// <param name="configuration">An instance of the IConfiguration interface for accessing application configuration.</param>        
        public AdafruitService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        /// <summary>
        /// Asynchronously gets the latest temperature and humidity data from the Adafruit API.
        /// </summary>
        /// <returns>A TempData object containing the latest temperature and humidity values.</returns>
        /// <exception cref="Exception">Thrown when the API request to Adafruit fails.</exception>
        public async Task<TempData> GetLatestAsync()
        {
            try
            {
                // Set the header
                _httpClient.DefaultRequestHeaders.Add("X-AIO-Key", _configuration["AdaFruitApiKey"]);
                // Set the query string to limit results to the latest recorded data point
                // var query = "/data?limit=1";
                // Make a request to Adafruit's API, temperature feed
                // var tempResponse = await _httpClient.GetAsync($"{_configuration["Adafruit:TemperatureFeedUrl"]}{query}");
                var tempResponse = await _httpClient.GetAsync(_configuration["TemperatureFeedUrl"]);

                if (!tempResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get data from Adafruit. Temperature Status Code: {tempResponse.StatusCode}");
                }

                // Make a request to Adafruit's API, humidity feed
                // var humResponse = await _httpClient.GetAsync($"{_configuration["Adafruit:HumidityFeedUrl"]}{query}");
                var humResponse = await _httpClient.GetAsync(_configuration["HumidityFeedUrl"]);

                // Ensure the request was successful
                if (!humResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Humidity Status Code: {humResponse.StatusCode}");
                }

                // Read the body of the response and deserialize it into a list of ResponseData objects
                string tempResponseBody = await tempResponse.Content.ReadAsStringAsync();
                string humResponseBody = await humResponse.Content.ReadAsStringAsync();

                List<ResponseData> tempDataResponseList = JsonSerializer.Deserialize<List<ResponseData>>(tempResponseBody);
                List<ResponseData> humDataResponseList = JsonSerializer.Deserialize<List<ResponseData>>(humResponseBody);

                // Take the first item from the response lists
                ResponseData tempDataResponse = tempDataResponseList[0];
                ResponseData humDataResponse = humDataResponseList[0];

                // Create TempData object
                TempData tempData = new TempData();
                tempData.Temperature = Double.Parse(tempDataResponse.value);
                tempData.Humidity = Double.Parse(humDataResponse.value);
                tempData.Timestamp = DateTime.Parse(tempDataResponse.created_at);

                return tempData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get data from Adafruit. Error: {ex.Message}");
            }
        }

    }
}
