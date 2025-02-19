using TaskToDoListApp.DTOs.Task.Request;

namespace TaskToDoListApp.Services.Task
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllTasksAsync(int userId);
        Task<TaskDto> GetTaskByIdAsync(int userId, int id);
        Task<TaskDto> CreateTaskAsync(int userId, TaskDto taskDto);
        Task<TaskDto> UpdateTaskAsync(int userId, int id, TaskDto taskDto);
        Task<bool> DeleteTaskAsync(int userId, int id);
    }
}
