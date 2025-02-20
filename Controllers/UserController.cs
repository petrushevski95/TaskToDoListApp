using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskToDoListApp.services.user;
using TaskToDoListApp.DTOs.User.Request;
using System.Security.Claims;

namespace TaskToDoListApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisteDtoRequest userDto)
        {
            var result = await _userService.RegisterUserAsync(userDto);

            if (result.Message == "Email already in use.")
            {
                return BadRequest(new { Message = "Email already in use.", DebugInfo = result.DebugInfo });
            }

            if (result.Message == "User registered successfully.")
            {
                return Ok(new { Message = "User registered successfully.", DebugInfo = result.DebugInfo });
            }

            return BadRequest(new { Message = "Registration failed.", DebugInfo = result.DebugInfo });
        }

        /// <summary>
        /// Update user details.
        /// </summary>
        [Authorize] // Requires authentication
        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateDtoRequest request)
        {
            // Get the logged-in user's ID
            var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Allow user to update only their own data unless they are Admin
            if (loggedInUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // Forbidden if not the user or an admin
            }

            var result = await _userService.UpdateUserAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        [AllowAnonymous] // Ensure this is present
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDtoRequest userDto)
        {
            if (userDto == null)
                return BadRequest(new { message = "Invalid request body." });

            var result = await _userService.LoginUserAsync(userDto);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
