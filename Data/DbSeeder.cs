using TaskToDoListApp.Models;

namespace TaskToDoListApp.Data
{
    public static class DbSeeder
    {
        public static void SeedRoles(AppDbContext context)
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Admin" },  // Use RoleName property
                    new Role { RoleName = "User" }
                );
                context.SaveChanges();
            }
        }
    }
}
