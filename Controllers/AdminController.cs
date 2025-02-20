using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskToDoListApp.Services.Admin;

namespace TaskToDoListApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Admin bans a user.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("ban/{userId}")]
        public async Task<IActionResult> BanUser(int userId)
        {
            var result = await _adminService.BanUserAsync(userId);
            if (!result.Success)
            {
                return BadRequest(result); // Return an error message if banning failed
            }
            return Ok(result); // Return success message if user is banned
        }

        /// <summary>
        /// Admin assigns a role to a user.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]  // Added HTTP verb and route
        public async Task<IActionResult> AssignRole(int userId, [FromQuery] string role)
        {
            var result = await _adminService.AssignRoleAsync(userId, role);
            if (!result.Success)
            {
                return BadRequest(result); // Return an error message if role assignment failed
            }
            return Ok(result); // Return success message if role is assigned
        }

        /// <summary>
        /// Admin removes ban from a user.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("unban/{userId}")]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            var result = await _adminService.UnbanUserAsync(userId);
            if (!result.Success)
            {
                return BadRequest(result); // Return an error message if unbanning failed
            }
            return Ok(result); // Return success message if user is unbanned
        }
    }
}
