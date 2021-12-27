using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Models;

namespace project_manage_system_backend.Shares
{
    public class PMSContext : DbContext
    {
        public PMSContext(DbContextOptions<PMSContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Repo> Repositories { get; set; }
        public DbSet<Invitation> Invitations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProject>().HasKey(p => new { p.ProjectId, p.Account });
            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.Project)
                .WithMany(p => p.Users)
                .HasForeignKey(ut => ut.ProjectId);

            modelBuilder.Entity<UserProject>()
                .HasOne(pt => pt.User)
                .WithMany(t => t.Projects)
                .HasForeignKey(pt => pt.Account);
        }

    }
}
