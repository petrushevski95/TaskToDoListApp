using Microsoft.EntityFrameworkCore;
using TaskToDoListApp.Data;
using TaskToDoListApp.DTOs.User.Response;
using System.Threading.Tasks;
using TaskToDoListApp.Services.Admin;
using TaskToDoListApp.Models;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    // Admin bans a user by their ID
    public async Task<UserLoginRegisterDtoResponse> BanUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new UserLoginRegisterDtoResponse { Success = false, Message = "User not found." };
        }

        // Log that the user is being banned
        if (user.IsBanned)
        {
            return new UserLoginRegisterDtoResponse { Success = false, Message = "User is already banned." };
        }

        user.IsBanned = true;
        await _context.SaveChangesAsync();

        return new UserLoginRegisterDtoResponse { Success = true, Message = $"User {user.Email} has been banned successfully." };
    }


    // Admin assigns a role to a user by their ID
    public async Task<UserUpdateDtoResponse> AssignRoleAsync(int userId, string role)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new UserUpdateDtoResponse { Success = false, Message = "User not found." };
        }

        var roleEntity = await _context.Roles.SingleOrDefaultAsync(r => r.RoleName == role);
        if (roleEntity == null)
        {
            return new UserUpdateDtoResponse { Success = false, Message = "Role not found." };
        }

        // Check if the user already has the role
        var existingUserRole = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleEntity.Id);

        if (existingUserRole)
        {
            return new UserUpdateDtoResponse { Success = false, Message = $"User already has the '{role}' role." };
        }

        // Remove existing roles for this user
        var userRoles = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
        _context.UserRoles.RemoveRange(userRoles); // Remove all roles of the user

        // Add the new role
        _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleEntity.Id });
        await _context.SaveChangesAsync();

        return new UserUpdateDtoResponse { Success = true, Message = $"Role '{role}' assigned successfully to {user.Email}." };
    }




    // Admin unban a user by their ID
    public async Task<UserLoginRegisterDtoResponse> UnbanUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new UserLoginRegisterDtoResponse { Success = false, Message = "User not found." };
        }

        if (!user.IsBanned)
        {
            return new UserLoginRegisterDtoResponse { Success = false, Message = "User is not banned." };
        }

        user.IsBanned = false;
        await _context.SaveChangesAsync();

        return new UserLoginRegisterDtoResponse { Success = true, Message = $"User {user.Email} has been unbanned successfully." };
    }

}
