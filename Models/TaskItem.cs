using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string CreatorId { get; set; } = string.Empty;
    public ApplicationUser? Creator { get; set; }

    public string? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}