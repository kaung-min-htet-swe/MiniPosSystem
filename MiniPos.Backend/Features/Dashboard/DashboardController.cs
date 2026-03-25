using Mapper;
using Microsoft.AspNetCore.Mvc;

namespace MiniPos.Backend.Features.Dashboard;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var result = await _dashboardService.GetStats();
        if (result.IsSuccess)
            return Ok(result.Data);

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });
    }
}
