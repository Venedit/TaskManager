using TaskManager.Models;

namespace TaskManager.Services.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetUrgentTasksAsync(string userId, int count = 5);
        Task<TaskItem?> GetTaskByIdAsync(int taskId);
        Task CreateTaskAsync(TaskItem task);
        Task<bool> UpdateTaskAsync(TaskItem task);
        Task<bool> ClaimTaskAsync(int taskId, string userId);
        Task<bool> UnclaimTaskAsync(int taskId);
        Task<bool> UpdateTaskStatusAsync(int taskId, Models.TaskStatus newStatus);
        Task<bool> DeleteTaskAsync(int taskId, string userId);
    }
}