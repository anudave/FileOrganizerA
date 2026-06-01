using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
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

    static void ListTables(SqliteConnection connection)
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

    static void ShowRecordCounts(SqliteConnection connection)
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

    static int GetRecordCount(SqliteConnection connection, string tableName)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT COUNT(*) FROM {tableName};";
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    static void ShowSampleData(SqliteConnection connection)
    {
        // Show FileOrganizationRules
        Console.WriteLine("📌 FileOrganizationRules:");
        QueryTable(connection, "FileOrganizationRules", 10);

        // Show FileOrganizationSchedules
        Console.WriteLine("\n📌 FileOrganizationSchedules:");
        QueryTable(connection, "FileOrganizationSchedules", 10);

        // Show FileOrganizationLogs (Latest 5)
        Console.WriteLine("\n📌 FileOrganizationLogs (Latest 5):");
        QueryTableOrdered(connection, "FileOrganizationLogs", 5, "ProcessedDate DESC");

        // Verify cloud tables are deleted
        Console.WriteLine("\n✅ VERIFICATION:");
        VerifyTablesDeleted(connection);
    }

    static void QueryTable(SqliteConnection connection, string tableName, int limit)
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
                        Console.Write($"{reader.GetName(i),25} | ");
                    }
                    Console.WriteLine();
                    Console.WriteLine(new string('-', fieldCount * 27));

                    // Print rows
                    int rowNum = 0;
                    while (reader.Read() && rowNum < limit)
                    {
                        for (int i = 0; i < fieldCount; i++)
                        {
                            var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "";
                            if (value.Length > 23)
                                value = value.Substring(0, 20) + "...";
                            Console.Write($"{value,25} | ");
                        }
                        Console.WriteLine();
                        rowNum++;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
    }

    static void QueryTableOrdered(SqliteConnection connection, string tableName, int limit, string orderBy)
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
                        Console.Write($"{reader.GetName(i),25} | ");
                    }
                    Console.WriteLine();
                    Console.WriteLine(new string('-', fieldCount * 27));

                    // Print rows
                    while (reader.Read())
                    {
                        for (int i = 0; i < fieldCount; i++)
                        {
                            var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "";
                            if (value.Length > 23)
                                value = value.Substring(0, 20) + "...";
                            Console.Write($"{value,25} | ");
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

    static void VerifyTablesDeleted(SqliteConnection connection)
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

    static bool TableExists(SqliteConnection connection, string tableName)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }
}
