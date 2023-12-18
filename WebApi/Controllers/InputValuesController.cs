using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.DTO;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InputValuesController : ControllerBase
    {
        [HttpPost("ConvertToString")]
        public IActionResult ConvertToString([FromBody] GenFilter inputData)
        {
            try
            {
                string result = JsonConvert.SerializeObject(inputData.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
