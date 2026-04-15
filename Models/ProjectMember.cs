namespace TaskManager.Models;

public class ProjectMember
{
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public ProjectRole Role { get; set; }
}