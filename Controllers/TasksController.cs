using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskToDoListApp.DTOs.Task.Request;
using TaskToDoListApp.DTOs.Task.Response;
using TaskToDoListApp.Services.Task;

namespace TaskToDoListApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the currently authenticated user's ID from the JWT token.
        /// </summary>
        /// <returns>User ID as an integer.</returns>
        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("No NameIdentifier claim found in token.");
                return null;
            }

            _logger.LogInformation($"User ID from token: {userIdClaim}");
            return int.Parse(userIdClaim);
        }

        /// <summary>
        /// Gets all tasks for the authenticated user.
        /// </summary>
        /// <returns>List of tasks.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(TaskNotFoundResponseDto))]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID missing from token" });
            }

            var tasks = await _taskService.GetAllTasksAsync(userId.Value);

            if (!tasks.Any())
            {
                return NotFound(new TaskNotFoundResponseDto
                {
                    Type = "NotFound",
                    Title = "Tasks Not Found",
                    Status = 404,
                    Detail = "No tasks found for the user.",
                    Instance = HttpContext.Request.Path,
                });
            }

            return Ok(tasks);
        }

        /// <summary>
        /// Gets a specific task by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(TaskNotFoundResponseDto))]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID missing from token" });
            }

            var task = await _taskService.GetTaskByIdAsync(userId.Value, id);

            if (task == null)
            {
                return NotFound(new TaskNotFoundResponseDto
                {
                    Type = "NotFound",
                    Title = "Task Not Found",
                    Status = 404,
                    Detail = $"No task found with ID {id} for this user.",
                    Instance = HttpContext.Request.Path,
                });
            }

            return Ok(task);
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<object>> CreateTask([FromBody] TaskDto taskDto)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID missing from token" });
            }

            var createdTask = await _taskService.CreateTaskAsync(userId.Value, taskDto);

            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id },
                new { Message = "Task successfully created", Task = createdTask });
        }

        /// <summary>
        /// Updates an existing task.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(TaskNotFoundResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDto taskDto)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID missing from token" });
            }

            var updatedTask = await _taskService.UpdateTaskAsync(userId.Value, id, taskDto);

            if (updatedTask == null)
            {
                return NotFound(new TaskNotFoundResponseDto
                {
                    Type = "NotFound",
                    Title = "Task Not Found",
                    Status = 404,
                    Detail = $"No task found with ID {id} for this user.",
                    Instance = HttpContext.Request.Path,
                });
            }

            return Ok(new { Message = "Task successfully updated", Task = updatedTask });
        }

        /// <summary>
        /// Deletes a task by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(TaskNotFoundResponseDto))]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID missing from token" });
            }

            var result = await _taskService.DeleteTaskAsync(userId.Value, id);

            if (!result)
            {
                return NotFound(new TaskNotFoundResponseDto
                {
                    Type = "NotFound",
                    Title = "Task Not Found",
                    Status = 404,
                    Detail = $"No task found with ID {id} for this user.",
                    Instance = HttpContext.Request.Path,
                });
            }

            return Ok(new { Message = $"Task with ID {id} has been successfully deleted" });
        }
    }
}
