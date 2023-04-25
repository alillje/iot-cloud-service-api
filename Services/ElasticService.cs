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
        public async Task<Dictionary<DateTime, double>> GetAverageTempData(int period = 10, bool hourly = false)
        {
            var index = "tempdata";

            // Get the date limitation for the given period days ago (hourly or daily)
            DateTime sinceTime = hourly ? DateTime.UtcNow.AddHours(-4) : DateTime.UtcNow.Date.AddDays(-period);

            // Perform the search with aggregation
            // Search between the given period
            var searchResponse = await _elasticClient.SearchAsync<TempData>(s => s
                .Index(index)
                .Size(0)
                .Query(q => q.DateRange(r => r
                    .Field(f => f.Timestamp)
                    .GreaterThanOrEquals(sinceTime)
                ))
                .Aggregations(a => a
                    .DateHistogram("period_average", dh => dh
                        .Field(f => f.Timestamp)
                        .CalendarInterval(hourly ? DateInterval.Hour : DateInterval.Day)
                        .ExtendedBounds(sinceTime, DateTime.UtcNow)
                        .Aggregations(aa => aa
                            .Average("period_temp_avg", avg => avg
                                .Field(f => f.Temperature)
                            )
                        )
                    )
                )
            );

            // Check if the search request was successful
            if (searchResponse.IsValid)
            {
                var averages = new Dictionary<DateTime, double>();

                // Extract the average temperatures from the aggregation results
                var dateHistogram = searchResponse.Aggregations.DateHistogram("period_average");
                
                foreach (var dailyBucket in dateHistogram.Buckets)
                {
                    var avgTemperature = dailyBucket.Average("period_temp_avg");
                    averages.Add(dailyBucket.Date, avgTemperature.Value ?? 0);
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
