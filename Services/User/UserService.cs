using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskToDoListApp.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TaskToDoListApp.DTOs.User.Request;
using TaskToDoListApp.DTOs.User.Response;
using TaskToDoListApp.Models;


namespace TaskToDoListApp.services.user
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, IConfiguration configuration, ILogger<UserService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserLoginRegisterDtoResponse> RegisterUserAsync(UserRegisteDtoRequest userDto)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                    return new UserLoginRegisterDtoResponse { Message = "Email already in use.", Token = null };

                string salt = GenerateSalt();
                string hashedPassword = HashPassword(userDto.Password, salt);

                var user = new User
                {
                    Email = userDto.Email,
                    FullName = userDto.FullName,
                    Salt = salt,
                    PasswordHash = hashedPassword
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {Email}", user.Email);

                return new UserLoginRegisterDtoResponse { Success = true, Message = "User registered successfully.", Token = null };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user.");
                return new UserLoginRegisterDtoResponse { Success = false, Message = "Registration failed.", Token = null };
            }
        }

        public async Task<UserLoginRegisterDtoResponse> LoginUserAsync(UserLoginDtoRequest userDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", userDto.Email);

                var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Email}", userDto.Email);
                    return new UserLoginRegisterDtoResponse { Success = false, Message = "Invalid email or password.", Token = null };
                }

                // Check if the user is banned
                if (user.IsBanned)
                {
                    _logger.LogWarning("User is banned: {Email}", userDto.Email);
                    return new UserLoginRegisterDtoResponse { Success = false, Message = "Your account is banned.", Token = null };
                }

                if (!VerifyPassword(userDto.Password, user.PasswordHash, user.Salt))
                {
                    _logger.LogWarning("Invalid password for user: {Email}", userDto.Email);
                    return new UserLoginRegisterDtoResponse { Success = false, Message = "Invalid email or password.", Token = null };
                }

                var token = await GenerateJwtToken(user);

                _logger.LogInformation("Token generated successfully for user: {Email}", userDto.Email);
                return new UserLoginRegisterDtoResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    Token = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging in user: {Email}", userDto.Email);
                return new UserLoginRegisterDtoResponse { Success = false, Message = "Login failed.", Token = null };
            }
        }


        public async Task<DTOs.User.Response.UserUpdateDtoResponse> UpdateUserAsync(int userId, DTOs.User.Request.UserUpdateDtoRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new DTOs.User.Response.UserUpdateDtoResponse { Success = false, Message = "User not found." };

            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrEmpty(request.Email))
            {
                bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId);
                if (emailExists)
                    return new DTOs.User.Response.UserUpdateDtoResponse { Success = false, Message = "Email is already in use." };
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                string salt = GenerateSalt();
                user.Salt = salt;
                user.PasswordHash = HashPassword(request.Password, salt);
            }

            await _context.SaveChangesAsync();

            return new DTOs.User.Response.UserUpdateDtoResponse { Success = true, Message = "User updated successfully." };
        }




        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));
        }

        private bool VerifyPassword(string password, string storedHash, string salt)
        {
            string computedHash = HashPassword(password, salt);
            return computedHash == storedHash;
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Fetch user roles asynchronously
            var roles = await _context.UserRoles
                                      .Where(ur => ur.UserId == user.Id)
                                      .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.RoleName)
                                      .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
