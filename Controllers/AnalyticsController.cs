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

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}