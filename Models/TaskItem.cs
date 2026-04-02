using System.ComponentModel.DataAnnotations;

namespace Pomolog.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign key to User

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Todo";

        public DateTime? StartedAtUtc { get; set; }
        public DateTime? FinishedAtUtc { get; set; }

        public int TotalPausedSeconds { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}