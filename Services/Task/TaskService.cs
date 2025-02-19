using Microsoft.EntityFrameworkCore;
using TaskToDoListApp.Data;
using TaskToDoListApp.DTOs.Task.Request;
using TaskToDoListApp.Models;

namespace TaskToDoListApp.Services.Task
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(int userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .Select(task => new TaskDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    DueDate = task.DueDate,
                    IsCompleted = task.IsCompleted,
                    Priority = task.Priority
                })
                .ToListAsync();
        }

        public async Task<TaskDto> GetTaskByIdAsync(int userId, int id)
        {
            var task = await _context.Tasks
                .Where(t => t.Id == id && t.UserId == userId)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    Priority = t.Priority
                })
                .FirstOrDefaultAsync();

            return task;
        }

        public async Task<TaskDto> CreateTaskAsync(int userId, TaskDto taskDto)
        {
            var task = new TaskItem
            {
                UserId = userId,
                Title = taskDto.Title,
                DueDate = taskDto.DueDate,
                IsCompleted = taskDto.IsCompleted,
                Priority = taskDto.Priority
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            taskDto.Id = task.Id;
            return taskDto;
        }

        public async Task<TaskDto> UpdateTaskAsync(int userId, int id, TaskDto taskDto)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null) return null;

            task.Title = taskDto.Title;
            task.DueDate = taskDto.DueDate;
            task.IsCompleted = taskDto.IsCompleted;
            task.Priority = taskDto.Priority;

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            // Return the updated task with correct Id
            return new TaskDto
            {
                Id = task.Id,  // Ensure the correct Id is returned
                Title = task.Title,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority
            };
        }

        public async Task<bool> DeleteTaskAsync(int userId, int id)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
