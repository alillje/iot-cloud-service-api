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

    private readonly IAdafruitService _adafruitService;

    public TempDataController(IAdafruitService adafruitService)
    {
        _adafruitService = adafruitService;
    }

    /// <summary>
    /// Retrieves the latest TempData object from Elasticsearch.
    /// </summary>
    /// <returns>An IActionResult containing the latest TempData object.</returns>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentTempData()
    {
        var response = await _adafruitService.GetLatestAsync();
        return Ok(response);
    }
    
}
