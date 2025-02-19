using TaskToDoListApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskToDoListApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between TaskItem and User
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.User)  // Each task belongs to a user
                .WithMany()            // A user can have many tasks
                .HasForeignKey(t => t.UserId)  // The foreign key in TaskItem
                .OnDelete(DeleteBehavior.Cascade);  // If a user is deleted, delete their tasks
        }
    }
}
