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
                return Ok();
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }

        [HttpGet("SearchByName/{query}")]
        public async Task<IActionResult> SearchByName(string query)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s =>
         s.Query(q => q
             .Bool(b => b
                 .Should(
                     sh => sh.QueryString(qs => qs.Query('*' + query + '*').Fields(f => f.Field(ff => ff.Name.common))),
                     sh => sh.QueryString(qs => qs.Query('*' + query + '*').Fields(f => f.Field(ff => ff.Name.official))),
                     sh => sh.QueryString(qs => qs.Query('*' + query + '*').Fields(f => f.Field(ff => ff.Name.nativeName)))
                 )
             )
         )
         .Size(5000));

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
        [HttpGet("SearchByCCN3/{ccn3}")]
        public async Task<IActionResult> GetByCCA2(string ccn3)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s =>
                     s.Query(q => q.QueryString(d => d.Query('*' + ccn3 + '*'))).Size(5000)
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

        [HttpGet("SearchByCCA2/{cca2}")]
        public async Task<IActionResult> SearchByCCA2(string cca2)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s =>
                          s.Query(q => q.QueryString(d => d.Query('*' + cca2 + '*'))).Size(5000)
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
        public async Task<IActionResult> SearchByLanguage(string language)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s =>
                          s.Query(q => q.QueryString(d => d.Query('*' + language + '*'))).Size(5000)
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
                var countries = JsonConvert.DeserializeObject<IEnumerable<CountryModel>>(jsonData);

                var bulkIndexResponse = await _elasticClient.IndexManyAsync(countries);

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
