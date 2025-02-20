namespace TaskToDoListApp.DTOs.User.Response
{
    public class UserLoginRegisterDtoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }

        public string DebugInfo { get; set; }
    }
}
