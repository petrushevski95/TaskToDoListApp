using System.ComponentModel.DataAnnotations;

namespace TaskToDoListApp.DTOs.Task.Request
{
    public class TaskDto
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public DateTime DueDate { get; set; } 
        [Required]
        public bool IsCompleted { get; set; }
        [Required]
        public string Priority { get; set; }
    }
}
