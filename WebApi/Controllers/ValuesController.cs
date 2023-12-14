using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using System.Diagnostics;


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

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var apiUrl = "https://restcountries.com/v3.1/all";
                var httpClient = _httpClientFactory.CreateClient();

                var jsonResponse = await httpClient.GetStringAsync(apiUrl);

                // Index data 
                Stopwatch sw = Stopwatch.StartNew();
                await IndexDataIntoElasticsearch(jsonResponse);

                sw.Stop();

                logger.LogInformation(sw.ElapsedMilliseconds.ToString());
                return Ok(jsonResponse);
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }


        [HttpGet("SearchByCCA2/{cca2}")]
        public async Task<IActionResult> GetByCCA2(string cca2)
        {
            try
            {
                // Perform Elasticsearch search using cca2
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s => s
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Cca2)
                            .Query(cca2)
                        )
                    )
                );

                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(searchResponse.Documents);
                }

                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }

        [HttpGet("SearchByLanguage/{language}")]
        public async Task<IActionResult> GetByAlphaCode(string language)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s => s
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Languages)
                            .Query(language)
                        )
                    )
                );

                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(searchResponse.Documents);
                }

                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }

        // index data into Elasticsearch
        private async Task IndexDataIntoElasticsearch(string jsonData)
        {
            try
            {
                var countries = JsonConvert.DeserializeObject<IList<CountryModel>>(jsonData);

                var bulkIndexResponse = await _elasticClient.IndexManyAsync(countries.Concat(countries));

                if (bulkIndexResponse.IsValid)
                {
                    Console.WriteLine("Data indexed successfully");
                }
                else
                {
                    Console.WriteLine($"Error indexing data: {bulkIndexResponse.DebugInformation}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }


    }
}
