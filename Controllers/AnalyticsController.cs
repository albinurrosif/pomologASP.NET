using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomolog.Api.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Pomolog.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/analytics/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            int userId = GetUserIdFromToken();

            // 1. Menghitung total tugas yang sudah "Done"
            var totalTasksCompleted = await _context.TaskItems
                .Where(t => t.UserId == userId && t.Status == "Done")
                .CountAsync();

            // 2. Menghitung berapa kali sesi Pomodoro sukses diselesaikan
            var totalSessionsCompleted = await _context.PomodoroSessions
                .Where(s => s.UserId == userId)
                .CountAsync();

            // 3. Menghitung total waktu fokus (jumlah durasi dari semua sesi)
            var totalFocusMinutes = await _context.PomodoroSessions
                .Where(s => s.UserId == userId)
                .SumAsync(s => s.DurationMinutes);

            return Ok(new
            {
                TotalTasksCompleted = totalTasksCompleted,
                TotalSessionsCompleted = totalSessionsCompleted,
                TotalFocusTimeMinutes = totalFocusMinutes
            });
        }

        // GET: /api/analytics/weekly
        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyAnalytics()
        {
            int userId = GetUserIdFromToken();
            var today = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-6);

            // Ambil sesi Pomodoro yang selesai dalam 7 hari terakhir
            var sessions = await _context.PomodoroSessions
                .Where(s => s.UserId == userId && s.CompletedAtUtc >= sevenDaysAgo)
                .ToListAsync();

            // Kelompokkan sesi berdasarkan tanggal dan hitung total menit serta jumlah sesi per hari
            var groupedSessions = sessions
                .GroupBy(s => s.CompletedAtUtc.Date)
                .ToDictionary(g => g.Key, g => new { TotalMinutes = g.Sum(s => s.DurationMinutes), SessionCount = g.Count() });

            // Ambil tugas yang selesai dalam 7 hari terakhir
            var completedTasks = await _context.TaskItems
                .Where(t => t.UserId == userId && t.Status == "Done" && t.CompletedAt >= sevenDaysAgo)
                .ToListAsync();

            // Kelompokkan tugas berdasarkan tanggal penyelesaian dan hitung jumlah tugas per hari
            var groupedTasks = completedTasks
                .Where(t => t.CompletedAt.HasValue)
                .GroupBy(t => t.CompletedAt.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<object>();

            // Loop untuk 7 hari terakhir (termasuk hari ini)
            for (int i = 6; i >= 0; i--)
            {
                var currentDate = today.AddDays(-i);
                var sessionData = groupedSessions.ContainsKey(currentDate) ? groupedSessions[currentDate] : new { TotalMinutes = 0, SessionCount = 0 };
                var taskCount = groupedTasks.ContainsKey(currentDate) ? groupedTasks[currentDate] : 0;

                result.Add(new
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    DayName = currentDate.ToString("ddd", new System.Globalization.CultureInfo("id-ID")),
                    TotalMinutes = sessionData.TotalMinutes,
                    SessionCount = sessionData.SessionCount,
                    TasksCompleted = taskCount
                });
            }

            return Ok(result);
        }

        // GET: /api/analytics/monthly
        [HttpGet("monthly")]
        public async Task<IActionResult> GetmonthlyAnalytics()
        {
            int userId = GetUserIdFromToken();
            var today = DateTime.UtcNow.Date;
            var thirtyDaysAgo = today.AddDays(-29); // 29 hari lalu + hari ini = 30 hari

            var sessions = await _context.PomodoroSessions
                .Where(s => s.UserId == userId && s.CompletedAtUtc >= thirtyDaysAgo)
                .ToListAsync();

            var groupedSessions = sessions
                .GroupBy(s => s.CompletedAtUtc.Date)
                .ToDictionary(g => g.Key, g => new { TotalMinutes = g.Sum(s => s.DurationMinutes), SessionCount = g.Count() });

            var completedTasks = await _context.TaskItems
                .Where(t => t.UserId == userId && t.Status == "Done" && t.CompletedAt >= thirtyDaysAgo)
                .ToListAsync();

            var groupedTasks = completedTasks
                .Where(t => t.CompletedAt.HasValue)
                .GroupBy(t => t.CompletedAt.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<object>();

            for (int i = 29; i >= 0; i--)
            {
                var currentDate = today.AddDays(-i);
                var sessionData = groupedSessions.ContainsKey(currentDate) ? groupedSessions[currentDate] : new { TotalMinutes = 0, SessionCount = 0 };
                var taskCount = groupedTasks.ContainsKey(currentDate) ? groupedTasks[currentDate] : 0;

                result.Add(new
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    DayName = currentDate.ToString("ddd", new System.Globalization.CultureInfo("id-ID")),
                    TotalMinutes = sessionData.TotalMinutes,
                    SessionCount = sessionData.SessionCount,
                    TasksCompleted = taskCount
                });
            }

            return Ok(result);
        }
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
            }
            return userId;
        }
    }
}