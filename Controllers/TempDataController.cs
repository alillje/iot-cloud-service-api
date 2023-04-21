using Microsoft.AspNetCore.Mvc;
using iot_cloud_service_api.Interfaces;
using iot_cloud_service_api.Models;


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
    public async Task<IActionResult> Add([FromBody] TempData tempData)
    {
        try
        {
            await _elasticService.AddAsync(tempData);
            return StatusCode(201);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}