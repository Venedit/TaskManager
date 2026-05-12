using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TaskManager.Models;
using TaskManager.Services.Interfaces;
using TaskManager.Data;
using TaskManager.ViewModels;

namespace TaskManager.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskItem>> GetUrgentTasksAsync(string userId, int count = 5)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Where(t => (t.AssigneeId == userId || t.CreatorId == userId) && t.Status != Models.TaskStatus.Completed)
                .OrderBy(t => t.Deadline)
                .Take(count)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .Include(t => t.Creator)
                .Include(t => t.Comments!)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task CreateTaskAsync(TaskItem task)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.Deadline = task.Deadline.ToUniversalTime();

            _context.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateTaskAsync(TaskEditViewModel model, string currentUserId)
        {
            var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (task == null) return false;


            var userRole = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == task.ProjectId && pm.UserId == currentUserId)
            .Select(pm => pm.Role)
            .FirstOrDefaultAsync();

            if(userRole != ProjectRole.Owner && userRole != ProjectRole.Manager) return false;


            task.Title = model.Title;
            task.Description = model.Description;
            task.Deadline = model.Deadline.ToUniversalTime();
            task.Priority = model.Priority;
            task.Status = model.Status;
            task.AssigneeId = model.AssigneeId;

            _context.Update(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClaimTaskAsync(int taskId, string userId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);

            if (!isMember) return false;

            task.AssigneeId = userId;
            task.Status = Models.TaskStatus.InProgress;

            _context.Update(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnclaimTaskAsync(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            task.AssigneeId = null;
            task.Status = Models.TaskStatus.New;

            _context.Update(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, string userId, Models.TaskStatus newStatus)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            var userRole = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId)
            .Select(pm => pm.Role)
            .FirstOrDefaultAsync();

            if (task.CreatorId != userId && task.AssigneeId != userId && userRole != ProjectRole.Owner && userRole != ProjectRole.Manager)
                return false; 
            task.Status = newStatus;

            _context.Update(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int taskId, string userId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);

            if (task.CreatorId == userId || (member != null && (member.Role == ProjectRole.Owner || member.Role == ProjectRole.Manager)))
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        public async Task<bool> RejectTaskWithCommentAsync(int taskId, string userId, string commentText)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            var comment = new TaskComment
            {
                TaskId = taskId,
                AuthorId = userId,
                Text = commentText,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskComments.Add(comment);

            task.Status = Models.TaskStatus.InProgress;
            _context.Update(task);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}