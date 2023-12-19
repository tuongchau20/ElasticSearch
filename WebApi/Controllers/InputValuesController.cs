using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Diagnostics;
using System.Linq;
using WebApi.DTO;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<SearchController> _logger;

        public SearchController(IElasticClient elasticClient, ILogger<SearchController> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        [HttpPost("Search")]
        public IActionResult Search([FromBody] GenFilter filter)
        {
            try
            {
                var field = filter.Field?.ToString();
                var value = filter.Value?.ToString();


                Stopwatch sw = Stopwatch.StartNew();

                ISearchResponse<CountryModel> searchResponse;

                if (string.IsNullOrWhiteSpace(field))
                {
                    searchResponse = _elasticClient.Search<CountryModel>(s => s
                        .Query(q => q
                            .QueryString(qs => qs
                                .Query("*" + value + "*")
                            )
                        )
                    );
                }
                else
                {
                    searchResponse = _elasticClient.Search<CountryModel>(s => s
                        .Query(q => q
                            .QueryString(qs => qs
                                .Query("*" + value + "*")
                                .DefaultField(field)
                            )
                        )
                        .Size(5)
                    );
                }

                sw.Stop();

                _logger.LogInformation(sw.ElapsedMilliseconds.ToString());
                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(new
                    {
                      searchResponse.Documents
                    });
                }

                return NotFound("No matching documents found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


    }
}
