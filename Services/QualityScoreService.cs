using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;

namespace QualiTrack.Services;

public class QualityScoreService(AppDbContext db) : IQualityScoreService
{
    public async Task<List<QualityTrendDto>> GetQualityTrendsAsync(int year)
    {
        var sessions = await db.AuditSessions
            .Include(s => s.Schedule)
            .Include(s => s.Responses)
            .Where(s => s.Status == AuditSessionStatus.Completed
                && s.CompletedAt.HasValue
                && s.CompletedAt.Value.Year == year)
            .ToListAsync();

        var grouped = sessions
            .GroupBy(s => s.CompletedAt!.Value.Month)
            .Select(g =>
                {
                    int totalItems = 0;
                    int totalConform = 0;
                    int totalAdjustedConform = 0;

                    foreach (var session in g)
                    {
                        var conformCount = session.Responses.Count(r => r.Answer == ResponseAnswer.Conform);
                        var itemCount = session.Responses.Count;
                        var isOverdue = session.CompletedAt.HasValue && session.CompletedAt.Value > session.Schedule.ScheduledDate;
                        var adjustedConform = isOverdue ? Math.Max(0, conformCount - 1) : conformCount;

                        totalItems += itemCount;
                        totalConform += conformCount;
                        totalAdjustedConform += adjustedConform;
                    }
                    return new QualityTrendDto
                    {
                        Period = g.Key,
                        PeriodLabel = new DateTime(year, g.Key, 1).ToString("MMM yyyy"),
                        TotalSessions = g.Count(),
                        ComplianceScore = totalItems == 0 ? 0 : Math.Round((double)totalConform / totalItems * 100, 1),
                        QualityScore = totalConform == 0 ? 0 : Math.Round((double)totalAdjustedConform / totalConform * 100, 1)
                    };
                })
            .OrderBy(x => x.Period)
            .ToList();
        return grouped;
    }
}