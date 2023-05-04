using Microsoft.AspNetCore.Mvc;
using iot_cloud_service_api.Interfaces;
using iot_cloud_service_api.Models;


namespace iot_cloud_service_api.Controllers;

/// <summary>
/// Controller class for handling temperature and humidity data requests.
/// </summary>
[ApiController]
[Route("tempdata")]
public class TempDataController : ControllerBase
{

    private readonly IElasticService _elasticService;

    public TempDataController(IElasticService elasticService)
    {
        _elasticService = elasticService;

    }

    /// <summary>
    /// Retrieves a list of TempData objects from Elasticsearch.
    /// </summary>
    /// <returns>An IActionResult containing a list of TempData objects.</returns>
    [HttpGet("")]
    public async Task<IActionResult> Get()
    {
        var response = await _elasticService.GetAsync();
        return Ok(response);
    }

    /// <summary>
    /// Retrieves the latest TempData object from Elasticsearch.
    /// </summary>
    /// <returns>An IActionResult containing the latest TempData object.</returns>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentTempData()
    {
        var response = await _elasticService.GetLatestAsync();
        return Ok(response);
    }

    /// <summary>
    /// Retrieves the daily average temperature and humidity data for a given period.
    /// </summary>
    /// <param name="days">The number of days to look back.</param>
    /// <returns>An IActionResult containing the daily average temperature and humidity data.</returns>
    [HttpGet("day-average")]
    public async Task<IActionResult> GetDailyAverage(int days)
    {
        var response = await _elasticService.GetAverageTempData(days, false);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves the hourly average temperature and humidity data for a given period.
    /// </summary>
    /// <param name="hours">The number of hours to look back.</param>
    /// <returns>An IActionResult containing the hourly average temperature and humidity data.</returns>
    [HttpGet("hour-average")]
    public async Task<IActionResult> GetHourlyAverage(int hours)
    {
        var response = await _elasticService.GetAverageTempData(hours, true);
        return Ok(response);
    }

    /// <summary>
    /// Adds a TempData object to Elasticsearch.
    /// </summary>
    /// <param name="tempData">The TempData object to be added.</param>
    /// <returns>An IActionResult representing the result of the operation.</returns>
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
