using System.ComponentModel.DataAnnotations;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    class TaskCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва обов'язкова")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public TaskPriority Priority { get; set; }

        public Models.TaskStatus Status { get; set; }

        public string? AssigneeId { get; set; }


        public int ProjectId { get; set; }
    }
    
}