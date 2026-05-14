using System.ComponentModel.DataAnnotations;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class TaskCreateViewModel
    {
        [Required(ErrorMessage = "Назва обов'язкова")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public TaskPriority Priority { get; set; }

        public string? AssigneeId { get; set; }

        [Required]
        public int ProjectId { get; set; }
    }

}