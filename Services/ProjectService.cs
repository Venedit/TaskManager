using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using TaskManager.Services.Interfaces;
using TaskManager.Data;

namespace TaskManager.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            return await _context.Projects
                .Where(p => p.Members!.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<Project?> GetProjectDetailsAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.Tasks!)
                    .ThenInclude(t => t.Assignee)
                .Include(p => p.Members!)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task CreateProjectAsync(Project project, string userId)
        {
            project.CreatedAt = DateTime.UtcNow;
            _context.Add(project);
            await _context.SaveChangesAsync();

            var member = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = userId,
                Role = ProjectRole.Owner
            };

            _context.Add(member);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteProjectAsync(int projectId, string userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null || member.Role != ProjectRole.Owner) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddMemberAsync(int projectId, string targetUserEmail, ProjectRole role)
        {
            var user = await _userManager.FindByEmailAsync(targetUserEmail);
            if (user == null) return false;

            var exists = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id);

            if (exists) return false;

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = user.Id,
                Role = role
            };

            _context.Add(member);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveMemberAsync(int projectId, string targetUserId, string currentUserId)
        {
            if (targetUserId == currentUserId) return false;

            var currentUserRole = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && pm.UserId == currentUserId)
                .Select(pm => pm.Role)
                .FirstOrDefaultAsync();

            if (currentUserRole != ProjectRole.Owner) return false;

            var memberToRemove = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == targetUserId);

            if (memberToRemove == null) return false;

            _context.ProjectMembers.Remove(memberToRemove);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMemberRoleAsync(int projectId, string targetUserId, ProjectRole newRole, string currentUserId)
        {
            if (targetUserId == currentUserId) return false;

            var currentUserRole = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && pm.UserId == currentUserId)
                .Select(pm => pm.Role)
                .FirstOrDefaultAsync();

            if (currentUserRole != ProjectRole.Owner) return false;

            var memberToUpdate = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == targetUserId);

            if (memberToUpdate == null) return false;

            memberToUpdate.Role = newRole;
            _context.Update(memberToUpdate);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateProjectAsync(Project project, string currentUserId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == currentUserId);

            if (member == null || member.Role != ProjectRole.Owner) return false;

            var existingProject = await _context.Projects.FindAsync(project.Id);
            if (existingProject == null) return false;

            existingProject.Name = project.Name;
            existingProject.Description = project.Description;

            _context.Update(existingProject);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}