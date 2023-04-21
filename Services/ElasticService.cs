using Nest;
using iot_cloud_service_api.Interfaces;
using iot_cloud_service_api.Models;


namespace iot_cloud_service_api.Services
{
    public class ElasticService : IElasticService
    {
        private readonly IElasticClient _elasticClient;

        public ElasticService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        /// <summary>
        /// Retrieves a list of GithubRepo documents from Elasticsearch.
        /// </summary>
        /// <returns>An IEnumerable of GithubRepo objects.</returns>
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

        /// Gets the latest tempdata object
        public async Task<TempData> GetLatestAsync()
        {
            var index = "tempdata";
            var query = new Func<QueryContainerDescriptor<TempData>, QueryContainer>(q => q
                .MatchAll()
            );

            // Construct the Elasticsearch search request using the provided index and query, and sort by timestamp in descending order
            var searchResponse = await _elasticClient.SearchAsync<TempData>(s => s
                .Index(index)
                .Query(query)
                .Size(1)
                .Sort(ss => ss
                    .Descending(t => t.Timestamp)
                )
            );

            // Check if the search request was successful
            if (searchResponse.IsValid)
            {
                // Return the first document retrieved from Elasticsearch (which is the latest one)
                return searchResponse.Documents.First();
            }
            else
            {
                throw new Exception($"Failed to get data from Elasticsearch. Error: {searchResponse.OriginalException?.Message}");
            }
        }
        // Get daily average data
        public async Task<Dictionary<DateTime, double>> GetAverageDayTempData(int days = 10)
        {
            var index = "tempdata";

            // Get the date for 10 days ago
            DateTime tenDaysAgo = DateTime.UtcNow.Date.AddDays(-days);

            // Create the date histogram aggregation and average aggregation
            // Perform the search with aggregation
            var searchResponse = await _elasticClient.SearchAsync<TempData>(s => s
                .Index(index)
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.Timestamp)
                        .GreaterThanOrEquals(tenDaysAgo)
                        .LessThanOrEquals(DateTime.UtcNow)
                    )
                )
                .Size(0)
                .Aggregations(a => a
                    .DateHistogram("daily_buckets", dh => dh
                        .Field(f => f.Timestamp)
                        .CalendarInterval(DateInterval.Day)
                        .ExtendedBounds(tenDaysAgo, DateTime.UtcNow)
                        .Aggregations(aa => aa
                            .Average("daily_avg_temperature", avg => avg
                                .Field(f => f.Temperature)
                            )
                        )
                    )
                ));

            // Check if the search request was successful
            if (searchResponse.IsValid)
            {
                var dailyAverages = new Dictionary<DateTime, double>();

                // Extract the average temperatures from the aggregation results
                var dateHistogram = searchResponse.Aggregations.DateHistogram("daily_buckets");
                foreach (var dailyBucket in dateHistogram.Buckets)
                {
                    var avgTemperature = dailyBucket.Average("daily_avg_temperature");
                    dailyAverages.Add(dailyBucket.Date, avgTemperature.Value ?? 0);
                }

                return dailyAverages;
            }
            else
            {
                throw new Exception($"Failed to get data from Elasticsearch. Error: {searchResponse.OriginalException?.Message}");
            }
        }
    }
}
