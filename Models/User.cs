using System.ComponentModel.DataAnnotations;

namespace TaskToDoListApp.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string PasswordHash { get; set; }
        public string Salt { get; set; } 
        public string Role { get; set; } = "User";
    }
}
