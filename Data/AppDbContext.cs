using Microsoft.EntityFrameworkCore;
using System.Data;
using TaskToDoListApp.Models;

namespace TaskToDoListApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }  // Add this DbSet for roles
        public DbSet<UserRole> UserRoles { get; set; } // Add this DbSet for UserRole many-to-many relationship

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between TaskItem and User
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.User)  // Each task belongs to a user
                .WithMany()            // A user can have many tasks
                .HasForeignKey(t => t.UserId) // The foreign key in TaskItem
                .OnDelete(DeleteBehavior.Cascade);  // If a user is deleted, delete their tasks

            // Configure many-to-many relationship between Users and Role
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });  // Composite key for the join table
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
