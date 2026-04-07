using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomolog.Api.Data;
using Pomolog.Api.Models;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Pomolog.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Dependency Injection: ASP.NET otomatis memasukkan AppDbContext ke sini
        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET /api/tasks (Ambil semua tugas)
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            int userId = GetUserIdFromToken(); // Ambil ID User dari JWT Token  
            var tasks = await _context.TaskItems.Where(t => t.UserId == userId).ToListAsync();
            return Ok(tasks); // Sama dengan: res.status(200).json(tasks)
        }

        // 2. POST /api/tasks (Buat tugas baru)
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem newTask)
        {
            int userId = GetUserIdFromToken(); // Ambil ID User dari JWT Token
            newTask.UserId = userId;
            newTask.CreatedAt = DateTime.UtcNow; // Selalu gunakan UTC!

            _context.TaskItems.Add(newTask);
            await _context.SaveChangesAsync(); // Sama dengan: await newTask.save() di Mongoose

            return CreatedAtAction(nameof(GetAllTasks), new { id = newTask.Id }, newTask);
        }

        // 3. PATCH /api/tasks/{id}/start (Mulai Pomodoro)
        [HttpPatch("{id}/start")]
        public async Task<IActionResult> StartTask(int id)
        {
            int currentUserId = GetUserIdFromToken();
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUserId);
            if (task == null) return NotFound("Tugas tidak ditemukan.");

            if (task.Status == "Done") return BadRequest("Tugas sudah selesai, tidak bisa dimulai lagi.");
            if (task.Status == "InProgress") return BadRequest("Tugas sedang berjalan.");

            task.Status = "InProgress";
            task.StartedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tugas dimulai!", task });
        }

        // 4. PATCH /api/tasks/{id}/pause (Logika Jeda & Anti-Cheat)
        [HttpPatch("{id}/pause")]
        public async Task<IActionResult> PauseTask(int id, [FromBody] int pausedSeconds)
        {
            int currentUserId = GetUserIdFromToken();
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUserId);
            if (task == null) return NotFound();
            if (task.StartedAtUtc == null) return BadRequest("Tugas belum dimulai!");

            // --- LOGIKA ANTI-CHEAT ---
            // Cek berapa lama waktu berlalu sejak tugas dimulai
            var elapsedTime = (DateTime.UtcNow - task.StartedAtUtc.Value).TotalSeconds;

            // Jika waktu jeda yang dikirim Frontend lebih besar dari waktu asli yang berjalan, berarti user curang!
            if (pausedSeconds > elapsedTime)
            {
                return BadRequest("Terdeteksi manipulasi waktu jeda (Cheat!).");
            }

            task.TotalPausedSeconds += pausedSeconds;
            task.Status = "Paused";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tugas dijeda.", totalPaused = task.TotalPausedSeconds });
        }

        // 5. PATCH /api/tasks/{id}/finish (Selesai Pomodoro)
        [HttpPatch("{id}/finish")]
        public async Task<IActionResult> FinishTask(int id)
        {
            int currentUserId = GetUserIdFromToken();
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUserId);
            if (task == null) return NotFound();

            if (task.StartedAtUtc == null) return BadRequest("Tidak bisa menyelesaikan tugas yang belum dimulai.");
            if (task.Status == "Done") return BadRequest("Tugas sudah selesai.");

            task.Status = "Done";
            task.FinishedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tugas selesai!", task });
        }

        // 6. DELETE /api/tasks/{id} (Hapus Tugas)
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

        // Fungsi untuk mengambil ID User dari JWT Token yang sedang aktif
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}