# 🗄️ How to View Your SQLite Database

## **Method 1: DB Browser for SQLite (Recommended - GUI)**

### Steps:
1. **Download** DB Browser for SQLite:
   - Go to: https://sqlitebrowser.org/
   - Download the Windows installer

2. **Install** the application

3. **Open** your database file:
   - Click: File → Open
   - Navigate to: `C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db`
   - Click Open

4. **View your data:**
   - Left sidebar shows all tables
   - Click on a table name to view records
   - Use the "Browse Data" tab to see records in a table view

**Tables you can view:**
- ✅ FileOrganizationRules
- ✅ FileOrganizationSchedules
- ✅ FileOrganizationLogs
- ✅ FileCategorySuggestions
- ✅ SmartSuggestionPatterns
- ❌ CloudOrganizationLogs (should NOT exist - deleted)
- ❌ CloudStorageAccounts (should NOT exist - deleted)

---

## **Method 2: Visual Studio Extensions**

### Using SQLite for Visual Studio extension:
1. In Visual Studio: Extensions → Manage Extensions
2. Search for "SQLite"
3. Download "SQLite for Visual Studio"
4. Restart Visual Studio
5. View → Other Windows → SQLite Explorer
6. Add connection to your database

---

## **Method 3: Command Line (If you have sqlite3 installed)**

```bash
# Open database
sqlite3 "C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db"

# Inside sqlite3 prompt:
.tables                              # List all tables
.schema FileOrganizationRules        # Show table structure
SELECT * FROM FileOrganizationRules; # View all rules
SELECT * FROM FileOrganizationSchedules;  # View all schedules
SELECT * FROM FileOrganizationLogs;  # View logs
.exit                                # Exit
```

---

## **Method 4: Using .NET Code (In Your App)**

Add this to test if needed:

```csharp
using Microsoft.EntityFrameworkCore;
using WpfApp1.Data;

// In your code:
using (var db = new FileOrganizerContext())
{
	// Get all rules
	var rules = db.FileOrganizationRules.ToList();
	foreach (var rule in rules)
	{
		Console.WriteLine($"Rule: {rule.RuleName}");
	}

	// Get all schedules
	var schedules = db.FileOrganizationSchedules.ToList();
	foreach (var schedule in schedules)
	{
		Console.WriteLine($"Schedule: {schedule.ScheduleName}");
	}

	// Get all logs
	var logs = db.FileOrganizationLogs.ToList();
	foreach (var log in logs)
	{
		Console.WriteLine($"Log: {log.Status}");
	}
}
```

---

## **Database Location**

```
C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db
```

Size: ~86 KB
Last Modified: 5/31/2026 10:28:45 AM

---

## **Current Database Schema**

### Tables (7 total):
```
1. AppSettings
2. FileOrganizationLogs
3. FileOrganizationRules
4. FileOrganizationSchedules
5. FileCategorySuggestions
6. SmartSuggestionPatterns
7. __EFMigrationsHistory (internal)
```

### Deleted Tables (Confirmed ✅):
```
❌ CloudOrganizationLogs (REMOVED)
❌ CloudStorageAccounts (REMOVED)
```

---

## **Quick SQL Queries**

### Count records:
```sql
SELECT 'Rules' as table_name, COUNT(*) FROM FileOrganizationRules
UNION ALL
SELECT 'Schedules', COUNT(*) FROM FileOrganizationSchedules
UNION ALL
SELECT 'Logs', COUNT(*) FROM FileOrganizationLogs;
```

### View all rules:
```sql
SELECT Id, RuleName, FilePattern, DestinationFolder, IsActive 
FROM FileOrganizationRules;
```

### View all schedules:
```sql
SELECT Id, ScheduleName, ScheduleType, StartTime, IsActive, NextRunTime 
FROM FileOrganizationSchedules;
```

### View recent logs:
```sql
SELECT * FROM FileOrganizationLogs 
ORDER BY ProcessedDate DESC 
LIMIT 10;
```

### Verify cloud tables deleted:
```sql
SELECT name FROM sqlite_master 
WHERE type='table' 
AND name IN ('CloudOrganizationLogs', 'CloudStorageAccounts');
```
(Should return empty result)

---

## **Recommended: DB Browser for SQLite**

**Why?**
- ✅ Free and open-source
- ✅ Easy GUI - no SQL knowledge needed
- ✅ View data in table format
- ✅ Edit data directly
- ✅ See table structure
- ✅ Run SQL queries
- ✅ Works on Windows, Mac, Linux

**Download:** https://sqlitebrowser.org/
