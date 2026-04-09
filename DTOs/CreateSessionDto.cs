using System.ComponentModel.DataAnnotations;

namespace Pomolog.Api.DTOs
{
    // Bentuk JSON utama yang akan dikirim dari React/Next.js
    public class CreateSessionDto
    {
        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public List<TaskBreakdownDto> Tasks { get; set; } = new List<TaskBreakdownDto>();
    }

    // Bentuk array tugas-tugas di dalam sesi tersebut
    public class TaskBreakdownDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int MinutesSpent { get; set; }
    }
}