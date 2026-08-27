using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QualiTrack.Services;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(IKpiService kpiService) : ControllerBase
{
    [HttpGet("kpi")]
    public async Task<IActionResult> GetMyKpi()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var kpi = await kpiService.GetUserKpiAsync(userId);
        return Ok(kpi);
    }
}