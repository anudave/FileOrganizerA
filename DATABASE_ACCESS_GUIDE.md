# 📊 SQLite Database Access Guide

## 🗄️ Database Location

### File Path
```
C:\Users\{YourUsername}\AppData\Roaming\FileOrganizer\fileorganizer.db
```

### How to Find It
1. Press **`Windows Key + R`** to open Run dialog
2. Type:
   ```
   %APPDATA%\FileOrganizer\fileorganizer.db
   ```
3. Press **Enter** - This will open the folder in File Explorer
4. You'll see the file: **`fileorganizer.db`**

---

## 📁 Full Path Breakdown

| Component | Value |
|-----------|-------|
| **Base Folder** | `%APPDATA%` (User's AppData\Roaming) |
| **Sub-Folder** | `FileOrganizer` |
| **Database File** | `fileorganizer.db` |
| **Example Full Path** | `C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db` |

---

## 🔧 How to Open & Manage the Database

### Option 1: Using DB Browser for SQLite (Recommended)
**Download:** https://sqlitebrowser.org/

1. Download and install DB Browser for SQLite
2. Open **DB Browser for SQLite**
3. Click **File → Open Database**
4. Navigate to: `C:\Users\{YourUsername}\AppData\Roaming\FileOrganizer\fileorganizer.db`
5. Click **Open**

### Option 2: Using Visual Studio
1. Open your project in Visual Studio
2. Go to **View → SQL Server Object Explorer**
3. Click **Add SQL Server** (the green plus icon)
4. In "Server Name" enter: 
   ```
   Data Source=C:\Users\{YourUsername}\AppData\Roaming\FileOrganizer\fileorganizer.db
   ```
5. Click **Connect**

### Option 3: Using Command Line
```powershell
# Open SQLite command line
sqlite3 "C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db"

# View all tables
.tables

# View a specific table structure
.schema FileOrganizationRules

# Query data
SELECT * FROM FileOrganizationRules;
```

### Option 4: Using Visual Studio Code
1. Install extension: **SQLite** by alexcvzz
2. Open Command Palette: **Ctrl + Shift + P**
3. Search: **SQLite: Open Database**
4. Select the database file from the path above
5. View and query data directly in VS Code

---

## 📋 Database Tables

### 1. **FileOrganizationRules** (Main Rules Table)
```sql
SELECT * FROM FileOrganizationRules;
```
**Columns:**
- `Id` - Primary Key
- `RuleName` - Name of the rule
- `FilePattern` - Patterns like `*.pdf|*.doc|*.docx`
- `DestinationFolder` - Where files go
- `IsActive` - Rule enabled/disabled (0 or 1)
- `CreatedDate` - When rule was created

**Example:**
```
ID | RuleName              | FilePattern      | DestinationFolder                  | IsActive | CreatedDate
3  | AI-Suggested: Other   | *.ini            | C:\Users\anwar\Videos\vid         | 1        | 2026-06-15
4  | AI-Suggested: Other   | *.ini            | C:\Users\anwar\Downloads\dc       | 1        | 2026-06-15
```

### 2. **FileOrganizationLogs** (Organization History)
```sql
SELECT * FROM FileOrganizationLogs;
```
**Tracks:** Each file moved, when it was moved, source & destination

### 3. **FileOrganizationSchedules** (Scheduled Tasks)
```sql
SELECT * FROM FileOrganizationSchedules;
```
**Tracks:** Recurring tasks, schedule timing, frequency

### 4. **AppSettings** (App Configuration)
```sql
SELECT * FROM AppSettings;
```
**Stores:** Theme, default folders, user preferences

### 5. **FileCategorySuggestions** (AI ML Suggestions)
```sql
SELECT * FROM FileCategorySuggestions;
```
**Tracks:** AI-predicted categories for files

### 6. **SmartSuggestionPatterns** (ML Learned Patterns)
```sql
SELECT * FROM SmartSuggestionPatterns;
```
**Stores:** Learned patterns from existing rules

---

## 🔍 Useful Database Queries

### View All Rules
```sql
SELECT Id, RuleName, FilePattern, DestinationFolder, IsActive, CreatedDate 
FROM FileOrganizationRules 
ORDER BY CreatedDate DESC;
```

### View Active Rules Only
```sql
SELECT * FROM FileOrganizationRules 
WHERE IsActive = 1;
```

### Count Total Rules
```sql
SELECT COUNT(*) as TotalRules FROM FileOrganizationRules;
```

### View Recent File Organization Logs
```sql
SELECT * FROM FileOrganizationLogs 
ORDER BY Timestamp DESC 
LIMIT 10;
```

### View AI Suggestions Made
```sql
SELECT * FROM FileCategorySuggestions 
ORDER BY CreatedDate DESC;
```

---

## 💾 Database File Size & Backup

### Check Database Size
```powershell
# In PowerShell
Get-Item "C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db" | Select-Object Length
```

### Backup Database
```powershell
# Create a backup copy
Copy-Item "C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db" "C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer_backup.db"
```

### Restore from Backup
```powershell
# Restore from backup
Remove-Item "C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db"
Copy-Item "C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer_backup.db" "C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db"
```

---

## ⚙️ Database Connection String (For Code)

Used in `FileOrganizerContext.cs`:
```csharp
var dbPath = Path.Combine(
	Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
	"FileOrganizer", 
	"fileorganizer.db"
);

optionsBuilder.UseSqlite($"Data Source={dbPath}");
```

---

## 📝 Quick Reference

| Task | Command/Action |
|------|---|
| **Open Database** | `%APPDATA%\FileOrganizer\fileorganizer.db` |
| **View Rules** | Click "File Organization Rules" table |
| **Add Rule** | Use app UI → "Create New Rule" section |
| **Edit Rule** | Click ✏️ Edit button in table |
| **Delete Rule** | Click 🗑️ Delete button in table |
| **View Logs** | Query `FileOrganizationLogs` table |
| **Clear Old Data** | Use DB Browser → right-click table → Delete rows |

---

## 🚀 Quick Start: DB Browser for SQLite

1. **Download:** https://sqlitebrowser.org/download/
2. **Install:** Run installer
3. **Launch:** Open "DB Browser for SQLite"
4. **File → Open Database**
5. **Navigate to:** `C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db`
6. **Browse Tables:** Click "Browse Data" tab
7. **Run Queries:** Click "Execute SQL" tab and paste SQL commands

---

## 📚 Learn More

- **SQLite Docs:** https://sqlite.org/
- **DB Browser Guide:** https://sqlitebrowser.org/docs/
- **Entity Framework Core:** https://learn.microsoft.com/en-us/ef/core/
