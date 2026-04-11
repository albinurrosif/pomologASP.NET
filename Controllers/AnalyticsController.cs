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

            // Tentukan batas waktu 7 hari ke belakang dari hari ini
            var today = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-6); // 6 hari lalu + hari ini = 7 hari

            // Ambil semua sesi dalam 7 hari terakhir
            var sessions = await _context.PomodoroSessions
                .Where(s => s.UserId == userId && s.CompletedAtUtc >= sevenDaysAgo)
                .ToListAsync();

            // Kelompokkan data yang didapat berdasarkan Tanggal
            var groupedData = sessions
                .GroupBy(s => s.CompletedAtUtc.Date)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalMinutes = g.Sum(s => s.DurationMinutes),
                        SessionCount = g.Count()
                    }
                );

            // Siapkan array kosong untuk 7 hari (agar hari yang kosong tetap tampil di grafik)
            var result = new List<object>();

            for (int i = 6; i >= 0; i--)
            {
                var currentDate = today.AddDays(-i);

                // Cek apakah di hari tersebut ada data, jika tidak, beri nilai 0
                var dayData = groupedData.ContainsKey(currentDate) ? groupedData[currentDate] : new { TotalMinutes = 0, SessionCount = 0 };

                result.Add(new
                {
                    Date = currentDate.ToString("yyyy-MM-dd"), // Format ISO agar mudah dibaca JS
                    DayName = currentDate.ToString("ddd", new System.Globalization.CultureInfo("id-ID")), // Output: Sen, Sel, Rab
                    TotalMinutes = dayData.TotalMinutes,
                    SessionCount = dayData.SessionCount
                });
            }

            return Ok(result);
        }

        // GET: /api/analytics/weekly
        [HttpGet("monthly")]
        public async Task<IActionResult> GetmonthlyAnalytics()
        {
            int userId = GetUserIdFromToken();

            // Tentukan batas waktu 30 hari ke belakang dari hari ini
            var today = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-29); // 29 hari lalu + hari ini = 30 hari

            // Ambil semua sesi dalam 30 hari terakhir
            var sessions = await _context.PomodoroSessions
                .Where(s => s.UserId == userId && s.CompletedAtUtc >= sevenDaysAgo)
                .ToListAsync();

            // Kelompokkan data yang didapat berdasarkan Tanggal
            var groupedData = sessions
                .GroupBy(s => s.CompletedAtUtc.Date)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalMinutes = g.Sum(s => s.DurationMinutes),
                        SessionCount = g.Count()
                    }
                );

            // Siapkan array kosong untuk 30 hari (agar hari yang kosong tetap tampil di grafik)
            var result = new List<object>();

            for (int i = 29; i >= 0; i--)
            {
                var currentDate = today.AddDays(-i);

                // Cek apakah di hari tersebut ada data, jika tidak, beri nilai 0
                var dayData = groupedData.ContainsKey(currentDate) ? groupedData[currentDate] : new { TotalMinutes = 0, SessionCount = 0 };

                result.Add(new
                {
                    Date = currentDate.ToString("yyyy-MM-dd"), // Format ISO agar mudah dibaca JS
                    DayName = currentDate.ToString("ddd", new System.Globalization.CultureInfo("id-ID")), // Output: Sen, Sel, Rab
                    TotalMinutes = dayData.TotalMinutes,
                    SessionCount = dayData.SessionCount
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