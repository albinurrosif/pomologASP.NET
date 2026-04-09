using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomolog.Api.Data;
using Pomolog.Api.Models;
using Pomolog.Api.DTOs;
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

            // VALIDASI KEAMANAN (IDOR Check)
            var requestTaskIds = dto.Tasks.Select(t => t.TaskId).ToList();
            var validTaskIds = await _context.TaskItems
                .Where(t => t.UserId == userId && requestTaskIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            var invalidTaskIds = requestTaskIds.Except(validTaskIds).ToList();
            if (invalidTaskIds.Any())
            {
                return BadRequest(new { message = "Unauthorized or Invalid Task IDs", invalidTaskIds });
            }

            // DATABASE TRANSACTION (Mencegah data setengah jadi)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newSession = new PomodoroSession
                {
                    UserId = userId,
                    DurationMinutes = dto.DurationMinutes,
                    CompletedAtUtc = DateTime.UtcNow
                };

                _context.PomodoroSessions.Add(newSession);
                await _context.SaveChangesAsync(); // Mendapatkan ID Sesi baru

                foreach (var task in dto.Tasks)
                {
                    var record = new SessionTaskRecord
                    {
                        SessionId = newSession.Id,
                        TaskId = task.TaskId,
                        MinutesSpent = task.MinutesSpent
                    };
                    _context.SessionTaskRecords.Add(record);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Konfirmasi semua berhasil!

                return Ok(new { message = "Sesi Pomodoro berhasil dicatat!", sessionId = newSession.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Batalkan semua jika ada error
                return StatusCode(500, new { message = "Terjadi kesalahan internal server.", details = ex.Message });
            }
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