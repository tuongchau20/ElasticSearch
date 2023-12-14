// ... (các using statements)

using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
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
                if (_elasticClient == null)
                {
                    logger.LogError("Elasticsearch client is not initialized");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Elasticsearch client is not initialized");
                }

                var apiUrl = "https://restcountries.com/v3.1/all";
                var httpClient = _httpClientFactory.CreateClient();

                var jsonResponse = await httpClient.GetStringAsync(apiUrl);

                Stopwatch sw = Stopwatch.StartNew();
                await IndexDataIntoElasticsearch(jsonResponse);
                sw.Stop();

                logger.LogInformation($"Indexing data took {sw.ElapsedMilliseconds} milliseconds");
                return Ok();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"HTTP request error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "HTTP request error");
            }
            catch (ElasticsearchClientException esEx)
            {
                logger.LogError($"Elasticsearch error: {esEx.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Elasticsearch error");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }

        [HttpGet("SearchByCCA2/{cca2}")]
        public async Task<IActionResult> GetByCCA2(string cca2)
        {
            try
            {
                if (_elasticClient == null)
                {
                    logger.LogError("Elasticsearch client is not initialized");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Elasticsearch client is not initialized");
                }
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s => 
                     s.Query(q => q.QueryString(d => d.Query('*' + cca2 + '*'))).Size(5000)
                );
                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(searchResponse.Documents);
                }

                return NotFound();
            }
            catch (ElasticsearchClientException esEx)
            {
                logger.LogError($"Elasticsearch error: {esEx.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Elasticsearch error");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }

        [HttpGet("SearchByLanguage/{language}")]
        public async Task<IActionResult> GetByAlphaCode(string language)
        {
            try
            {
                if (_elasticClient == null)
                {
                    logger.LogError("Elasticsearch client is not initialized");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Elasticsearch client is not initialized");
                }

                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s =>
                     s.Query(q => q.QueryString(d => d.Query('*' + language + '*'))).Size(5000)
                );

                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(searchResponse.Documents);
                }

                return NotFound();
            }
            catch (ElasticsearchClientException esEx)
            {
                logger.LogError($"Elasticsearch error: {esEx.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Elasticsearch error");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }

        // index data into Elasticsearch
        private async Task IndexDataIntoElasticsearch(string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    logger.LogWarning("JSON data is empty or null");
                    return;
                }

                var countries = JsonConvert.DeserializeObject<IEnumerable<CountryModel>>(jsonData);

                if (countries == null)
                {
                    logger.LogWarning("Deserialized countries list is null");
                    return;
                }

                var bulkIndexResponse = await _elasticClient.IndexManyAsync(countries);

                if (!bulkIndexResponse.IsValid)
                {
                    foreach (var itemWithError in bulkIndexResponse.ItemsWithErrors)
                    {
                        logger.LogError($"Error indexing data: {itemWithError.Error.Reason}");
                    }
                }
            }
            catch (ElasticsearchClientException esEx)
            {
                logger.LogError($"Elasticsearch error: {esEx.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message}");
            }
        }
    }
}
