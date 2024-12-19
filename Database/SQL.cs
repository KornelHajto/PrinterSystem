using Microsoft.EntityFrameworkCore;
using PrinterSystem.Models;

namespace PrinterSystem.Database
{
    public class SQL : DbContext
    {
        public SQL() : base(new DbContextOptionsBuilder().UseSqlServer(Program.ConnectionString).Options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Print> Prints { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Print>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .IsRequired();

            modelBuilder.Entity<AuditLog>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .IsRequired();
        }
    }
}
