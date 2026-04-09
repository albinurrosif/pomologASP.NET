using System.ComponentModel.DataAnnotations;

namespace Pomolog.Api.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;
    }
}