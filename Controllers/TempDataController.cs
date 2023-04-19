using Microsoft.AspNetCore.Mvc;
using iot_cloud_service_api.Interfaces;

namespace iot_cloud_service_api.Controllers;

[ApiController]
[Route("tempdata")]
public class TempDataController : ControllerBase
{

    private readonly IElasticService _elasticService;

    public TempDataController(IElasticService elasticService)
    {
        _elasticService = elasticService;

    }

    [HttpGet("")]
    public async Task<IActionResult> Get()
    {
        var response = await _elasticService.GetAsync();
        return Ok(response);
    }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] string value)
    {
        return Ok(new { message = "Tempdata API" });
    }
}
