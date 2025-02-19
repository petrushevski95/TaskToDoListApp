using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskToDoListApp.Models;
using TaskToDoListApp.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TaskToDoListApp.DTOs.User.Request;
using TaskToDoListApp.DTOs.User.Response;


namespace TaskToDoListApp.services.user
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterUserAsync(UserRegisterDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return new AuthResponseDto { Message = "Email already in use.", Token = null };

            string salt = GenerateSalt(); // Generate salt before creating user object
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

            return new AuthResponseDto { Message = "User registered successfully.", Token = null, DebugInfo = "User created successfully." };
        }

        public async Task<AuthResponseDto> LoginUserAsync(UserLoginDto userDto)
        {
            Console.WriteLine($"Login attempt for email: {userDto.Email}");
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return new AuthResponseDto { Success = false, Message = "Invalid email or password.", Token = null, DebugInfo = "User not found in database." };
            }

            if (!VerifyPassword(userDto.Password, user.PasswordHash, user.Salt))
            {
                Console.WriteLine("Password verification failed");
                return new AuthResponseDto { Success = false, Message = "Invalid email or password.", Token = null, DebugInfo = "Password mismatch." };
            }

            var token = GenerateJwtToken(user);
            Console.WriteLine($"Generated token: {token}");
            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                DebugInfo = "Token successfully generated."
            };
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

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Use ClaimTypes.NameIdentifier here
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
