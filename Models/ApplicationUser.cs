using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public ICollection<TaskItem>? CreatedTasks { get; set; }
    public ICollection<TaskItem>? AssignedTasks { get; set; }
    public ICollection<ProjectMember>? ProjectMemberships { get; set; }
}