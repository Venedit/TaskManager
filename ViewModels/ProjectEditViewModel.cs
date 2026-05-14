using System.ComponentModel.DataAnnotations;

namespace TaskManager.ViewModels
{
    public class ProjectEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва проєкту обов'язкова")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}