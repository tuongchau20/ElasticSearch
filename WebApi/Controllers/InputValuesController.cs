﻿using Microsoft.AspNetCore.Http;
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

        private async Task<bool> IsBooleanFieldAsync(string fieldName)
        {
            //1. Ánh xạ dữ liệu đến CountryModel sử dụng phương thức bất đồng bộ
            var response = await _elasticClient.Indices.GetMappingAsync<object>();
            //2. lấy dữ liệu trả về response 
            var properties = response.Indices[typeof(CountryModel)].Mappings.Properties;
            // Kiểm tra xem có thuộc tính fieldName trong danh sách các thuộc tính không
            if (properties.TryGetValue(fieldName, out var property))
            {
                // Nếu có, kiểm tra xem kiểu dữ liệu của thuộc tính đó có phải là boolean không
                return property.Type == "boolean";
            }
            // Nếu không tìm thấy thuộc tính, trả về false
            return false;
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] GenFilter filter)
        {
            try
            {
                var field = filter.Field?.ToString();
                var value = filter.Value?.ToString();

                Stopwatch sw = Stopwatch.StartNew();

                ISearchResponse<CountryModel> searchResponse;

                QueryContainer queryContainer;

                if (string.IsNullOrWhiteSpace(field))
                {
                    queryContainer = new QueryStringQuery
                    {
                        Query = "*" + value + "*"
                    };
                }
                else
                {
                    if (await IsBooleanFieldAsync(field))
                    {
                        if (!bool.TryParse(value, out bool boolValue))
                        {
                            return BadRequest("Invalid boolean value.");
                        }

                        queryContainer = new TermQuery
                        {
                            Field = field,
                            Value = boolValue
                        };
                    }
                    else
                    {
                        queryContainer = new QueryStringQuery
                        {
                            Query = "*" + value + "*",
                            DefaultField = field
                        };
                    }
                }

                searchResponse = _elasticClient.Search<CountryModel>(s => s
                    .Query(q => queryContainer)
                );

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