using TaskToDoListApp.DTOs.User.Request;
using TaskToDoListApp.DTOs.User.Response;

namespace TaskToDoListApp.services.user
{
    public interface IUserService
    {
        Task<UserLoginRegisterDtoResponse> RegisterUserAsync(UserRegisteDtoRequest userDto);
        Task<UserLoginRegisterDtoResponse> LoginUserAsync(UserLoginDtoRequest userDto);
        Task<UserUpdateDtoResponse> UpdateUserAsync(int userId, UserUpdateDtoRequest request); // Update return type here
    }
}
