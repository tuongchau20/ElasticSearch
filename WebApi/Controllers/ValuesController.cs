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
        #region SearchBy
        [HttpGet("SearchByName/{query}")]
        public async Task<IActionResult> SearchByName(string query)
        {
            try
            {
                var normalizedQuery = "*" + query + "*";
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Should(
                                sh => sh.QueryString(qs => qs
                                    .Query(normalizedQuery)
                                    .Fields(f => f
                                        .Field(ff => ff.Name.common)
                                        .Field(ff => ff.Name.official)
                                    )
                                )
                                //sh => sh.Nested(n => n
                                //    .Path(p => p.Name.nativeName)
                                //    .Query(nq => nq
                                //        .Bool(nb => nb
                                //            .Should(ns => ns
                                //                .Match(m => m
                                //                    .Field(f => f.Name.nativeName.Values.First().common)
                                //                    .Query(query)
                                //                ),
                                //                ns => ns
                                //                .Match(m => m
                                //                    .Field(f => f.Name.nativeName.Values.First().official)
                                //                    .Query(query)
                                //                )
                                //            )
                                //        )
                                //    )
                                //)
                            )
                        )
                    )
                    .Size(5)
                    .AllowPartialSearchResults(true)
                );

                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(searchResponse.Documents);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error: " + ex.Message);
            }
        }




        [HttpGet("SearchByCCN3/{ccn3}")]
        public async Task<IActionResult> GetByCCN3(string ccn3)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s =>
                    s.Query(q => q
                        .Match(m => m
                            .Field(f => f.Ccn3)
                            .Query("*" + ccn3 + "*")
                        )
                    ).Size(5)
                     .AllowPartialSearchResults(true)
                );

                sw.Stop();

                logger.LogInformation(sw.ElapsedMilliseconds.ToString());

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
                Stopwatch sw = Stopwatch.StartNew();
                var searchResponse = await _elasticClient.SearchAsync<CountryModel>(s =>
                          s.Query(q => q
                              .Match(m => m
                                  .Field(f => f.Cca2)
                                  .Query("+"+cca2+"+")
                              )
                          ).Size(5)
                           .AllowPartialSearchResults(true)
                     );
                sw.Stop();
                logger.LogInformation(sw.ElapsedMilliseconds.ToString());
                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(searchResponse.Documents);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }
        #endregion


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
