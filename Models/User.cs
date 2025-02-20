using System.ComponentModel.DataAnnotations;
using TaskToDoListApp.Models;

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
    public bool IsBanned { get; set; } = false; // Default: Not banned
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
