using System.ComponentModel.DataAnnotations;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class TaskEditViewModel
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

        // Тільки ID виконавця, а не весь об'єкт ApplicationUser
        public string? AssigneeId { get; set; }

        // Тільки ID проєкту, щоб знати, куди повернутись
        public int ProjectId { get; set; }
    }
}