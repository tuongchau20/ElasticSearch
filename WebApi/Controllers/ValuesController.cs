using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(IHttpClientFactory httpClientFactory, IElasticClient elasticClient, ILogger<ValuesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var apiUrl = "https://restcountries.com/v3.1/all";
                var httpClient = _httpClientFactory.CreateClient();
                var jsonResponse = await httpClient.GetStringAsync(apiUrl);

                // Indexing 
                var indexResponse = await _elasticClient.IndexAsync<object>(jsonResponse, idx => idx.Index("your_index_name"));

                if (!indexResponse.IsValid)
                {
                   
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error indexing data to Elasticsearch");
                }

                return Ok(jsonResponse);
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }


        [HttpGet("[action]/{cca2}")]
        public async Task<IActionResult> GetByCCA2(string cca2)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://restcountries.com/v3.1/alpha/{cca2}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                  
                    var indexResponse = await _elasticClient.IndexAsync(new IndexRequest<object>(jsonResponse, "your_index_name"));

                    return Ok(jsonResponse);
                }

                return NotFound();
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }

        [HttpGet("[action]/{alphaCode}")]
        public async Task<IActionResult> GetByAlphaCode(string alphaCode)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://restcountries.com/v3.1/alpha/{alphaCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                   
                    var indexResponse = await _elasticClient.IndexAsync(new IndexRequest<object>(jsonResponse, "your_index_name"));

                    return Ok(jsonResponse);
                }

                return NotFound();
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }
    }
}
