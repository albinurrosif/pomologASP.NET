using Microsoft.AspNetCore.Mvc;
using Pomolog.Api.Data;
using Pomolog.Api.Models;
using Pomolog.Api.DTOs; // Import DTO kita
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Pomolog.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SessionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RecordSession([FromBody] CreateSessionDto dto)
        {
            int userId = GetUserIdFromToken();

            // 1. Simpan Sesi Induk dulu
            var newSession = new PomodoroSession
            {
                UserId = userId,
                DurationMinutes = dto.DurationMinutes,
                CompletedAtUtc = DateTime.UtcNow
            };

            _context.PomodoroSessions.Add(newSession);

            // Simpan ke DB agar newSession.Id otomatis di-generate oleh PostgreSQL
            await _context.SaveChangesAsync();

            // 2. Loop tugas-tugasnya dan simpan ke Junction Table
            foreach (var task in dto.Tasks)
            {
                var record = new SessionTaskRecord
                {
                    SessionId = newSession.Id, // Ambil ID yang baru tercipta
                    TaskId = task.TaskId,
                    MinutesSpent = task.MinutesSpent
                };
                _context.SessionTaskRecords.Add(record);
            }

            // Simpan semua relasinya ke DB
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sesi Pomodoro berhasil dicatat!", sessionId = newSession.Id });
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