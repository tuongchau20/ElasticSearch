using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using System.Diagnostics;
using WebApi.Model;


namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ValuesController> logger;

        public ValuesController(IHttpClientFactory httpClientFactory, IElasticClient elasticClient, ILogger<ValuesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _elasticClient = elasticClient;
            this.logger = logger;
        }

        [HttpGet("GetAllCountries")]
        public async Task<IActionResult> GetAllCountries([FromQuery] string indexName)
        {
            if (!IndexExists<CountryModel>(_elasticClient, indexName))
            {
                EnsureIndexExists<CountryModel>(_elasticClient, indexName);
            }
            try
            {
                var apiUrl = "https://restcountries.com/v3.1/all";
                var httpClient = _httpClientFactory.CreateClient();
                var jsonResponse = await httpClient.GetStringAsync(apiUrl);

                Stopwatch sw = Stopwatch.StartNew();
                await IndexDataIntoElasticsearch<CountryModel>(jsonResponse, indexName); 
                sw.Stop();
                logger.LogInformation(sw.ElapsedMilliseconds.ToString());
                return Ok();
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }
        [HttpGet("GetAllDog")]
        public async Task<IActionResult> GetAllDog([FromQuery] string indexName)
        {
            if (!IndexExists<DogModel>(_elasticClient, indexName))
            {
                EnsureIndexExists<DogModel>(_elasticClient, indexName);
            }
            try
            {
                var apiUrl = "https://api.algobook.info/v1/dogs/all";
                var httpClient = _httpClientFactory.CreateClient();
                var jsonResponse = await httpClient.GetStringAsync(apiUrl);

                Stopwatch sw = Stopwatch.StartNew();
                await IndexDataIntoElasticsearch<DogModel>(jsonResponse, indexName);
                sw.Stop();
                logger.LogInformation(sw.ElapsedMilliseconds.ToString());
                return Ok();
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }
        // index data into Elasticsearch
        private async Task IndexDataIntoElasticsearch<T>(string jsonData, string indexName) where T : class
        {
            try
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<T>>(jsonData);

                var bulkIndexResponse = await _elasticClient.IndexManyAsync(items, indexName); 
                if (bulkIndexResponse.IsValid)
                {
                    Console.WriteLine($"Data indexed successfully into index: {indexName}");
                }
                else
                {
                    Console.WriteLine($"Error indexing data into index {indexName}: {bulkIndexResponse.DebugInformation}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private bool IndexExists<T>(IElasticClient client, string indexName) where T : class
        {
            var existsResponse = client.Indices.Exists(indexName);
            return existsResponse.Exists;
        }
        private  void EnsureIndexExists<T>(IElasticClient client, string indexName) where T : class
        {
            var existsResponse = client.Indices.Exists(indexName);

            if (!existsResponse.Exists)
            {
                var createIndexResponse = client.Indices.Create(indexName, c => c
                    .Map<T>(m => m.AutoMap()));

                if (!createIndexResponse.IsValid)
                {
                    Console.WriteLine($"Error creating index: {createIndexResponse.DebugInformation}");
                }
            }
            else
            {
                // Update the index mapping
                var response = client.Map<T>(m => m
                    .Index(indexName)
                    .AutoMap());

                if (!response.IsValid)
                {
                    Console.WriteLine($"Error updating index mapping: {response.DebugInformation}");
                }
            }
        }


    }
}
