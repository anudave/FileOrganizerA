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
        public DbSet<ExclusionPattern> ExclusionPatterns { get; set; }

        // ML/AI Suggestion Tables
        public DbSet<SmartSuggestionPattern> SmartSuggestionPatterns { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileOrganizer", "fileorganizer.db");
            var dbDir = System.IO.Path.GetDirectoryName(dbPath);

            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir);

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            // Disable automatic migration warnings
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        /// <summary>
        /// Ensure database schema is correct
        /// </summary>
        public void EnsureMigrated()
        {
            try
            {
                // Just create the database and tables if they don't exist
                Database.EnsureCreated();

                // Ensure Category column exists - add it if missing
                EnsureCategoryColumn();

                EnsureAllTablesExist();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database setup error: {ex.Message}");
            }
        }

        private void EnsureAllTablesExist()
        {
            try
            {
                var connection = Database.GetDbConnection();
                bool closeConnection = connection.State == System.Data.ConnectionState.Closed;
                if (closeConnection) connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS FileOrganizationSchedules (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ScheduleName TEXT,
                            TargetFolderPath TEXT,
                            ScheduleType TEXT,
                            StartTime TEXT,
                            DaysOfWeek TEXT,
                            IntervalHours INTEGER NOT NULL,
                            RunOnce TEXT,
                            IsActive INTEGER NOT NULL,
                            LastRunTime TEXT NOT NULL,
                            NextRunTime TEXT,
                            LastRunStatus TEXT,
                            LastRunMessage TEXT,
                            CreatedDate TEXT NOT NULL
                        );

                        DROP TABLE IF EXISTS FileCategorySuggestions;
                        DROP TABLE IF EXISTS ExclusionPatterns;

                        CREATE TABLE IF NOT EXISTS SmartSuggestionPatterns (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            FilePattern TEXT NOT NULL,
                            Category TEXT NOT NULL,
                            CommonDestinationFolder TEXT,
                            Frequency INTEGER NOT NULL,
                            Accuracy REAL NOT NULL,
                            Confidence REAL NOT NULL,
                            LastUpdated TEXT NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS AppSettings (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            StartOnBoot INTEGER NOT NULL,
                            RunInBackground INTEGER NOT NULL,
                            EnableSmartSuggestions INTEGER NOT NULL,
                            EnableNotifications INTEGER NOT NULL,
                            DefaultOrganizationFolder TEXT,
                            LogRetentionDays INTEGER NOT NULL
                        );
                    ";
                    command.ExecuteNonQuery();
                }

                if (closeConnection) connection.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring tables: {ex.Message}");
            }
        }

        private void EnsureCategoryColumn()
        {
            try
            {
                var connection = Database.GetDbConnection();
                connection.Open();

                // Check if Category column exists
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA table_info(FileOrganizationRules)";
                    using (var reader = command.ExecuteReader())
                    {
                        bool categoryExists = false;
                        while (reader.Read())
                        {
                            if (reader["name"].ToString() == "Category")
                            {
                                categoryExists = true;
                                break;
                            }
                        }

                        // If column doesn't exist, add it
                        if (!categoryExists)
                        {
                            reader.Close();
                            using (var addCommand = connection.CreateCommand())
                            {
                                addCommand.CommandText = "ALTER TABLE FileOrganizationRules ADD COLUMN Category TEXT";
                                addCommand.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine("Added Category column");
                            }
                        }
                    }
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring Category column: {ex.Message}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AppSettings
            modelBuilder.Entity<AppSettings>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<AppSettings>()
                .Property(s => s.DefaultOrganizationFolder)
                .IsRequired(false);


            modelBuilder.Entity<AppSettings>()
                .Property(s => s.DuplicateHandlingStrategy)
                .IsRequired();

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
                .HasMaxLength(500);

            // Configure ExclusionPattern
            modelBuilder.Entity<ExclusionPattern>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ExclusionPattern>()
                .Property(e => e.PatternName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ExclusionPattern>()
                .Property(e => e.Pattern)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<ExclusionPattern>()
    .Property(e => e.Description)
    .HasMaxLength(500);

modelBuilder.Entity<FileOrganizationRule>()
    .Property(r => r.Category)
    .HasMaxLength(50);

            modelBuilder.Entity<FileOrganizationRule>()
                .Property(r => r.DestinationFolder)
                .IsRequired()
                .HasMaxLength(260);


            // Configure FileOrganizationLog
            modelBuilder.Entity<FileOrganizationLog>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<FileOrganizationLog>()
                .Property(l => l.SourceFilePath)
                .IsRequired();

            // Configure FileOrganizationSchedule
            modelBuilder.Entity<FileOrganizationSchedule>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<FileOrganizationSchedule>()
                .Property(s => s.ScheduleName)
                .IsRequired();

            modelBuilder.Entity<FileOrganizationSchedule>()
                .Property(s => s.TargetFolderPath)
                .IsRequired();

            modelBuilder.Entity<FileOrganizationSchedule>()
                .Property(s => s.ScheduleType)
                .IsRequired();

            modelBuilder.Entity<FileOrganizationSchedule>()
                .Property(s => s.StartTime)
                .IsRequired();

            modelBuilder.Entity<FileOrganizationSchedule>()
                .Property(s => s.LastRunStatus)
                .IsRequired();

            modelBuilder.Entity<FileOrganizationSchedule>()
                .Property(s => s.LastRunMessage)
                .IsRequired();


            // Configure SmartSuggestionPattern (ML/AI)
            modelBuilder.Entity<SmartSuggestionPattern>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<SmartSuggestionPattern>()
                .Property(p => p.FilePattern)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<SmartSuggestionPattern>()
                .Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
