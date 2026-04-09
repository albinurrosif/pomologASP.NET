namespace Pomolog.Api.Models
{
    public class SessionTaskRecord
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public PomodoroSession? Session { get; set; }

        public int TaskId { get; set; }
        public TaskItem? Task { get; set; }

        // Ini adalah Payload-nya (Data ekstra di persimpangan)
        public int MinutesSpent { get; set; }
    }
}