using System.Text.Json.Serialization;

namespace Pomolog.Api.Models
{
    public class PomodoroSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
        public int DurationMinutes { get; set; } = 25;

        // Relasi ke tabel persimpangan (Satu sesi bisa punya banyak task)
        [JsonIgnore]
        public ICollection<SessionTaskRecord> TaskRecords { get; set; } = new List<SessionTaskRecord>();
    }
}