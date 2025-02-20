namespace TaskToDoListApp.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }  // Example: "Admin", "User"
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
