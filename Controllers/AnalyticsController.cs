using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pomolog.Api.Data;
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

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            int userId = GetUserIdFromToken();

            // 1. Ambil HANYA tugas yang sudah "Done" dan punya waktu mulai/selesai
            var doneTasks = await _context.TaskItems
                .Where(t => t.UserId == userId && t.Status == "Done" && t.StartedAtUtc != null && t.FinishedAtUtc != null)
                .ToListAsync();

            // 2. Hitung total tugas
            int totalTasksCompleted = doneTasks.Count;

            // 3. Hitung total detik fokus sesungguhnya
            // Rumus: (Waktu Selesai - Waktu Mulai) - Waktu Jeda
            double totalFocusSeconds = doneTasks.Sum(t =>
                (t.FinishedAtUtc!.Value - t.StartedAtUtc!.Value).TotalSeconds - t.TotalPausedSeconds);

            // 4. Konversi ke menit agar lebih mudah dibaca Frontend
            double totalFocusMinutes = Math.Round(totalFocusSeconds / 60, 2);

            return Ok(new
            {
                TotalTasksCompleted = totalTasksCompleted,
                TotalFocusSeconds = totalFocusSeconds,
                TotalFocusMinutes = totalFocusMinutes
            });
        }
    }
}