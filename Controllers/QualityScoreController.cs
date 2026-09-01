using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QualiTrack.Services;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class QualityScoreController(IQualityScoreService qualityScoreService) : ControllerBase
{
    [HttpGet("trend")]
    [Authorize(Roles = "Admin, QualityManager, Auditor, AuditorInternal")]
    public async Task<IActionResult> GetTrend([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var trend = await qualityScoreService.GetQualityTrendsAsync(targetYear);
        return Ok(trend);
    }
}