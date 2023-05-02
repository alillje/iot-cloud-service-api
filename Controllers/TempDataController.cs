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

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentTempData()
    {
        var response = await _elasticService.GetLatestAsync();
        return Ok(response);
    }

    [HttpGet("day-average")]
    public async Task<IActionResult> GetDailyAverage(int days)
    {
        var response = await _elasticService.GetAverageTempData(days, false);
        return Ok(response);
    }

    [HttpGet("hour-average")]
    public async Task<IActionResult> GetHourlyAverage(int hours)
    {
        var response = await _elasticService.GetAverageTempData(hours, true);
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
