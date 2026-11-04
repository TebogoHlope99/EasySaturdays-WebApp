using EasySaturdays.Models;
using Microsoft.EntityFrameworkCore;

namespace EasySaturdays.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<VolunteerShift> VolunteerShifts { get; set; }
        public DbSet<RoleChangeRequest> RoleChangeRequests { get; set; }
        public DbSet<AdminRequest> AdminRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Campaign>().Property(c => c.GoalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Campaign>().Property(c => c.RaisedAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Donation>().Property(d => d.Amount).HasPrecision(18, 2);
        }
    }
}