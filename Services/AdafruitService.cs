using Nest;
using iot_cloud_service_api.Interfaces;
using iot_cloud_service_api.Models;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;


namespace iot_cloud_service_api.Services
{
    /// <summary>
    /// ElasticService is responsible for interacting with Elasticsearch to store and retrieve TempData data.
    /// </summary>
    public class AdafruitService : IElasticService
    {
        private readonly IElasticClient _elasticClient;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;



        /// <summary>
        /// Initializes a new instance of the ElasticService class.
        /// </summary>
        /// <param name="elasticClient">An instance of the IElasticClient interface for interacting with Elasticsearch.</param>
        public AdafruitService(IElasticClient elasticClient, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _elasticClient = elasticClient;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves a list of TempData documents from Elasticsearch.
        /// </summary>
        /// <returns>An IEnumerable of TempData objects.</returns>
        public async Task<IEnumerable<TempData>> GetAsync()
        {
            var index = "tempdata";
            var query = new Func<QueryContainerDescriptor<TempData>, QueryContainer>(q => q
            .MatchAll()
        );
            // Construct the Elasticsearch search request using the provided index and query
            var searchResponse = await _elasticClient.SearchAsync<TempData>(s => s
                .Index(index)
                .Query(query)
                .Size(50)
                .From(0)
            );

            // Check if the search request was successful
            if (searchResponse.IsValid)
            {
                // Return the list of documents retrieved from Elasticsearch
                return searchResponse.Documents;
            }
            else
            {
                throw new Exception($"Failed to get data from Elasticsearch. Error: {searchResponse.OriginalException?.Message}");
            }
        }

        /// <summary>
        /// Adds a TempData object to Elasticsearch.
        /// </summary>
        /// <param name="tempData">The TempData object to be indexed in Elasticsearch.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task AddAsync(TempData tempData)
        {
            var index = "tempdata";

            // Index the TempData document in Elasticsearch
            var indexResponse = await _elasticClient.IndexAsync(tempData, i => i
                .Index(index)
            );

            // Check if the index operation was successful
            if (!indexResponse.IsValid)
            {
                throw new Exception($"Failed to index data in Elasticsearch. Error: {indexResponse.OriginalException?.Message}");
            }
            Console.Write("Successfully inserted data in Elasticsearch.");
        }


        /// <summary>
        /// Retrieves a list of TempData documents from Elasticsearch.
        /// </summary>
        /// <returns>An IEnumerable of TempData objects.</returns>
        public async Task<TempData> GetLatestAsync()
        {
            try
            {

                // Set the header
                _httpClient.DefaultRequestHeaders.Add("X-AIO-Key", _configuration["Adafruit:TemperatureApiKey"]);


                // Make a request to Adafruit's API
                var tempResponse = await _httpClient.GetAsync(_configuration["Adafruit:TemperatureFeedUrl"]);

                if (!tempResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get data from Adafruit. Temperature Status Code: {tempResponse.StatusCode}");
                }

                var humResponse = await _httpClient.GetAsync(_configuration["Adafruit:HumidityFeedUrl"]);

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

        /// <summary>
        /// Gets the average temperature and humidity data for a given period (in hours or days).
        /// </summary>
        /// <param name="period">The period to retrieve data for. Defaults to 10.</param>
        /// <param name="hourly">A boolean indicating if the data should be retrieved hourly. Defaults to false.</param>
        /// <returns>A Task with a result of a Dictionary containing DateTime keys and Dictionary values with temperature and humidity data.</returns>

        public async Task<Dictionary<DateTime, Dictionary<String, double>>> GetAverageTempData(int period = 10, bool hourly = false)
        {
            var index = "tempdata";

            // Limit the period to 30 days for daily average and 48 hours for hourly average
            if (hourly)
            {
                if (period > 48 || period <= 0)
                {
                    period = 24;
                }
            }
            else
            {
                if (period > 30 || period <= 0)
                {
                    period = 10;
                }
            }

            // Get the date limitation for the given period days ago (hourly or daily)
            DateTime sinceTime = hourly ? DateTime.UtcNow.AddHours(-period) : DateTime.UtcNow.Date.AddDays(-period);

            // Perform the search with aggregation
            // Search between the given period
            // Add aggregations for both temperature and humidity
            // Search within timespan - the previous day/hour and until 'sinceTime'
            var searchResponse = await _elasticClient.SearchAsync<TempData>(s => s
                .Index(index)
                .Size(0)
                .Query(q => q.DateRange(r => r
                    .Field(f => f.Timestamp)
                    .GreaterThanOrEquals(sinceTime)
                    .LessThanOrEquals(hourly ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddDays(-1))
                ))
                .Aggregations(a => a
                    .DateHistogram("period_average", dh => dh
                        .Field(f => f.Timestamp)
                        .CalendarInterval(hourly ? DateInterval.Hour : DateInterval.Day)
                        .ExtendedBounds(sinceTime, hourly ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddDays(-1))
                        .Order(HistogramOrder.KeyDescending)
                        .Aggregations(aa => aa
                            .Average("period_temp_avg", avg => avg
                                .Field(f => f.Temperature)
                            )
                            .Average("period_humidity_avg", avg => avg
                                .Field(f => f.Humidity)
                            )
                        )
                    )
                )
            );

            // Check if the search request was successful
            if (searchResponse.IsValid)
            {
                // var tempAverage = new Dictionary<DateTime, double>();
                // var humidityAverage = new Dictionary<DateTime, double>();
                // Create dictionary to store all average periodical data
                var averages = new Dictionary<DateTime, Dictionary<String, double>>();

                // Extract the average temperatures from the aggregation results
                var dateHistogram = searchResponse.Aggregations.DateHistogram("period_average");

                foreach (var dailyBucket in dateHistogram.Buckets)
                {
                    // Create dictionary to store average temperature and humidity for each period
                    var tempDataAverage = new Dictionary<String, double>();

                    var avgTemperature = dailyBucket.Average("period_temp_avg");
                    var avgHumidity = dailyBucket.Average("period_humidity_avg");

                    // tempAverage.Add(dailyBucket.Date, avgTemperature.Value ?? 0);
                    // humidityAverage.Add(dailyBucket.Date, avgHumidity.Value ?? 0);
                    // Add the dictionary of current averages to the dictionary of all averages
                    tempDataAverage.Add("temperature", avgTemperature.Value ?? 0);
                    tempDataAverage.Add("humidity", avgHumidity.Value ?? 0);
                    averages.Add(dailyBucket.Date, tempDataAverage);


                }
                return averages;
            }
            else
            {
                throw new Exception($"Failed to get average temp data from Elasticsearch. Error: {searchResponse.OriginalException?.Message}");
            }
        }
    }
}
