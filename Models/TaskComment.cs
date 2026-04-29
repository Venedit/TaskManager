using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class TaskComment
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Зв'язок з користувачем (автор коментаря)
        public string AuthorId { get; set; } = string.Empty;
        public ApplicationUser? Author { get; set; }

        // Зв'язок з задачею
        public int TaskId { get; set; }
        public TaskItem? Task { get; set; }
    }
}