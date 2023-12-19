using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
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
                var @operator = filter.Operator?.ToString();
                var value = filter.Value?.ToString();

                if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(@operator) || string.IsNullOrWhiteSpace(value))
                {
                    return BadRequest("Invalid filter parameters.");
                }

                var searchResponse = _elasticClient.Search<CountryModel>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Should(
                                sh => sh
                                    .Match(m => m
                                        .Field(field)
                                        .Query(value)
                                    )
                            )
                        )
                    ).Size(5)
                );
                
                if (searchResponse.IsValid && searchResponse.Documents.Any())
                {
                    return Ok(searchResponse.Documents);
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
