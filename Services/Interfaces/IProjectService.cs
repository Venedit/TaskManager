using TaskManager.Models;

namespace TaskManager.Services.Interfaces
{
    public interface IProjectService
    {
        Task<List<Project>> GetUserProjectsAsync(string userId);
        Task<Project?> GetProjectDetailsAsync(int projectId);
        Task CreateProjectAsync(Project project, string userId);
        Task<bool> DeleteProjectAsync(int projectId, string userId);
        Task<bool> AddMemberAsync(int projectId, string targetUserEmail, ProjectRole role);
        Task<bool> RemoveMemberAsync(int projectId, string targetUserId, string currentUserId);
        Task<bool> UpdateMemberRoleAsync(int projectId, string targetUserId, ProjectRole newRole, string currentUserId);
        Task<bool> UpdateProjectAsync(Project project, string currentUserId);
    }
}