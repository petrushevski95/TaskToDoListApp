using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskToDoListApp.services.user;
using TaskToDoListApp.DTOs.User.Request;

namespace TaskToDoListApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userDto)
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
        /// Logs in a user and returns a JWT token.
        /// </summary>
        [AllowAnonymous] // Ensure this is present
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
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
