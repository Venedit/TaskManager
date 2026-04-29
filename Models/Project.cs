using System.ComponentModel.DataAnnotations;


namespace TaskManager.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskItem>? Tasks { get; set; }
    public ICollection<ProjectMember>? Members { get; set; }
}