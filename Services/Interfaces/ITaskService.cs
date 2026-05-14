using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Services.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetUrgentTasksAsync(string userId, int count = 5);
        Task<TaskItem?> GetTaskByIdAsync(int taskId);
        Task<bool> CreateTaskAsync(TaskCreateViewModel model, string userId);
        Task<bool> UpdateTaskAsync(TaskEditViewModel model, string currentUserId);
        Task<bool> ClaimTaskAsync(int taskId, string userId);
        Task<bool> UnclaimTaskAsync(int taskId);
        Task<bool> UpdateTaskStatusAsync(int taskId, string userId, Models.TaskStatus newStatus);
        Task<bool> DeleteTaskAsync(int taskId, string userId);
        Task<bool> RejectTaskWithCommentAsync(int taskId, string userId, string commentText);
    }
}