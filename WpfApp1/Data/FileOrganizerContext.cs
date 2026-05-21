using System.IO;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Models;

namespace WpfApp1.Data
{
    public class FileOrganizerContext : DbContext
    {
        public DbSet<FileOrganizationRule> FileOrganizationRules { get; set; }
        public DbSet<FileOrganizationLog> FileOrganizationLogs { get; set; }
        public DbSet<FileOrganizationSchedule> FileOrganizationSchedules { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileOrganizer", "fileorganizer.db");
            var dbDir = System.IO.Path.GetDirectoryName(dbPath);

            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir);

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure FileOrganizationRule
            modelBuilder.Entity<FileOrganizationRule>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<FileOrganizationRule>()
                .Property(r => r.RuleName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<FileOrganizationRule>()
                .Property(r => r.FilePattern)
                .IsRequired()
                .HasMaxLength(50);

            // Configure FileOrganizationLog
            modelBuilder.Entity<FileOrganizationLog>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<FileOrganizationLog>()
                .Property(l => l.SourceFilePath)
                .IsRequired();
        }
    }
}
