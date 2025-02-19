using TaskToDoListApp.DTOs.User.Request;
using TaskToDoListApp.DTOs.User.Response;

namespace TaskToDoListApp.services.user
{
    public interface IUserService
    {
        Task<AuthResponseDto> RegisterUserAsync(UserRegisterDto userDto);
        Task<AuthResponseDto> LoginUserAsync(UserLoginDto userDto);
    }
}
