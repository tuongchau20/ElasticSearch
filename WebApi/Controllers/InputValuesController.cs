using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public SearchController(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        [HttpPost("Search")]
        public IActionResult Search([FromBody] GenFilter filter)
        {
            try
            {
                var field = filter.Field?.ToString();
                var value = filter.Value?.ToString();

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ISearchResponse<CountryModel> searchResponse;

                if (string.IsNullOrWhiteSpace(field))
                {
                    searchResponse = _elasticClient.Search<CountryModel>(s => s
                        .Query(q => q
                            .QueryString(qs => qs
                                .Query("*" + value + "*")
                            )
                        )
                        .Size(5)
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

                stopwatch.Stop();

                TimeSpan executionTime = stopwatch.Elapsed;

                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(new
                    {
                        ExecutionTime = executionTime.TotalMilliseconds + " ms",
                        Results = searchResponse.Documents
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
