
using System.Text.Json.Serialization;

namespace Pomolog.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Todo";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relasi ke tabel persimpangan (Satu task bisa ada di banyak sesi)
        [JsonIgnore] // Agar tidak error loop saat di-return sebagai JSON
        public ICollection<SessionTaskRecord> SessionRecords { get; set; } = new List<SessionTaskRecord>();
    }
}
