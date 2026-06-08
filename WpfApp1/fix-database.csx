#!/usr/bin/env dotnet-script

#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;
using System;
using System.IO;

public class DatabaseFixer
{
    public static void Main()
    {
        string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileOrganizer",
            "fileorganizer.db"
        );

        if (!File.Exists(dbPath))
        {
            Console.WriteLine($"❌ Database not found: {dbPath}");
            return;
        }

        Console.WriteLine($"✅ Connected to: {dbPath}\n");

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "ALTER TABLE FileOrganizationRules ADD COLUMN Category TEXT NOT NULL DEFAULT 'Other'";
                    command.ExecuteNonQuery();
                    Console.WriteLine("✅ Category column added successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding column: {ex.Message}");
            }
        }
    }
}

DatabaseFixer.Main();
