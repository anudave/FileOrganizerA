using System;
using System.IO;
using Microsoft.Data.Sqlite;

public class DbChecker
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
            Console.WriteLine($"Database not found at {dbPath}");
            return;
        }

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetString(0));
                }
            }
        }
    }
}

DbChecker.Main();
