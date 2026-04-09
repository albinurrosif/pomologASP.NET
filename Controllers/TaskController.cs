using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomolog.Api.Data;
using Pomolog.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Pomolog.Api.DTOs;

namespace Pomolog.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: Ambil semua tugas
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            int userId = GetUserIdFromToken();
            var tasks = await _context.TaskItems.Where(t => t.UserId == userId).ToListAsync();
            return Ok(tasks);
        }

        // 2. POST: Buat tugas baru
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto) // Gunakan DTO
        {
            var newTask = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = GetUserIdFromToken(),
                CreatedAt = DateTime.UtcNow,
                Status = "Todo"
            };

            _context.TaskItems.Add(newTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllTasks), new { id = newTask.Id }, newTask);
        }

        // 3. PATCH: Selesai Tugas (Finish)
        [HttpPatch("{id}/finish")]
        public async Task<IActionResult> FinishTask(int id)
        {
            int currentUserId = GetUserIdFromToken();
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUserId);
            if (task == null) return NotFound();

            if (task.Status == "Done") return BadRequest("Tugas sudah selesai.");

            task.Status = "Done";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tugas selesai!", task });
        }

        // 4. DELETE: Hapus Tugas
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            int currentUserId = GetUserIdFromToken();
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUserId);
            if (task == null) return NotFound();

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tugas berhasil dihapus." });
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}