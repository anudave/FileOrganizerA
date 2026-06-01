#!/usr/bin/env dotnet-script

// Install sqlite3 if not present
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;
using System;

public class DatabaseViewer
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

            // List all tables
            Console.WriteLine("📊 TABLES IN DATABASE:\n");
            ListTables(connection);

            // Show record counts
            Console.WriteLine("\n📈 RECORD COUNTS:\n");
            ShowRecordCounts(connection);

            // Show sample data
            Console.WriteLine("\n📋 SAMPLE DATA:\n");
            ShowSampleData(connection);
        }
    }

    private static void ListTables(SqliteConnection connection)
    {
        string query = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
        using (var command = connection.CreateCommand())
        {
            command.CommandText = query;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"  • {reader.GetString(0)}");
                }
            }
        }
    }

    private static void ShowRecordCounts(SqliteConnection connection)
    {
        string query = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
        using (var command = connection.CreateCommand())
        {
            command.CommandText = query;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    int count = GetRecordCount(connection, tableName);
                    Console.WriteLine($"  {tableName}: {count} records");
                }
            }
        }
    }

    private static int GetRecordCount(SqliteConnection connection, string tableName)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT COUNT(*) FROM {tableName};";
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    private static void ShowSampleData(SqliteConnection connection)
    {
        // Show AppSettings
        Console.WriteLine("📌 AppSettings:");
        QueryTable(connection, "AppSettings", 5);

        // Show FileOrganizationRules
        Console.WriteLine("\n📌 FileOrganizationRules:");
        QueryTable(connection, "FileOrganizationRules", 3);

        // Show FileOrganizationSchedules
        Console.WriteLine("\n📌 FileOrganizationSchedules:");
        QueryTable(connection, "FileOrganizationSchedules", 3);

        // Show FileOrganizationLogs
        Console.WriteLine("\n📌 FileOrganizationLogs (Latest 3):");
        QueryTableOrdered(connection, "FileOrganizationLogs", 3, "ProcessedDate DESC");

        // Verify cloud tables are deleted
        Console.WriteLine("\n✅ VERIFICATION:");
        VerifyTablesDeleted(connection);
    }

    private static void QueryTable(SqliteConnection connection, string tableName, int limit)
    {
        try
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM {tableName} LIMIT {limit};";
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine($"  (Empty table)");
                        return;
                    }

                    // Print headers
                    var fieldCount = reader.FieldCount;
                    for (int i = 0; i < fieldCount; i++)
                    {
                        Console.Write($"{reader.GetName(i),20} | ");
                    }
                    Console.WriteLine();
                    Console.WriteLine(new string('-', fieldCount * 22));

                    // Print rows
                    while (reader.Read())
                    {
                        for (int i = 0; i < fieldCount; i++)
                        {
                            var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();
                            Console.Write($"{(value ?? "").Substring(0, Math.Min(value?.Length ?? 0, 18)),20} | ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
    }

    private static void QueryTableOrdered(SqliteConnection connection, string tableName, int limit, string orderBy)
    {
        try
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM {tableName} ORDER BY {orderBy} LIMIT {limit};";
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine($"  (Empty table)");
                        return;
                    }

                    // Print headers
                    var fieldCount = reader.FieldCount;
                    for (int i = 0; i < fieldCount; i++)
                    {
                        Console.Write($"{reader.GetName(i),20} | ");
                    }
                    Console.WriteLine();
                    Console.WriteLine(new string('-', fieldCount * 22));

                    // Print rows
                    while (reader.Read())
                    {
                        for (int i = 0; i < fieldCount; i++)
                        {
                            var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();
                            Console.Write($"{(value ?? "").Substring(0, Math.Min(value?.Length ?? 0, 18)),20} | ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
    }

    private static void VerifyTablesDeleted(SqliteConnection connection)
    {
        string[] tablesToCheck = { "CloudOrganizationLogs", "CloudStorageAccounts" };
        foreach (var tableName in tablesToCheck)
        {
            bool exists = TableExists(connection, tableName);
            if (exists)
            {
                Console.WriteLine($"  ⚠️  Table exists (should be deleted): {tableName}");
            }
            else
            {
                Console.WriteLine($"  ✅ Table successfully deleted: {tableName}");
            }
        }
    }

    private static bool TableExists(SqliteConnection connection, string tableName)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }
}

DatabaseViewer.Main();
