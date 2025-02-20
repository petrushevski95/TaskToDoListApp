using TaskToDoListApp.DTOs.User.Response;

namespace TaskToDoListApp.Services.Admin
{
    public interface IAdminService
    {
        // Method to delete a user by their ID
        Task<UserLoginRegisterDtoResponse> BanUserAsync(int userId);

        Task<UserLoginRegisterDtoResponse> UnbanUserAsync(int userId);

        // Method to assign a role to a user by their ID
        Task<UserUpdateDtoResponse> AssignRoleAsync(int userId, string role);
    }
}
