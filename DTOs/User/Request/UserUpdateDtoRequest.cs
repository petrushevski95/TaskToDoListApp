namespace TaskToDoListApp.DTOs.User.Request
{
    public class UserUpdateDtoRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Optional: Only update if provided
    }
}

