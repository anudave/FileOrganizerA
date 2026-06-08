# 🗄️ Complete Database Guide - Everything You Need to Know

## 📌 Database Basics First

### What is a Database?
🏢 **Real World Analogy:** Think of a DATABASE like a **LIBRARY**
- **Database** = Entire library building
- **Tables** = Different sections (Fiction, Science, History, etc.)
- **Records** = Individual books
- **Fields/Columns** = Book details (Title, Author, ISBN, Pages)
- **Rows** = One complete book entry

---

## 🎯 Your Database Details

### Database Name
```
fileorganizer.db
```

### Database Location
```
C:\Users\anwar\AppData\Roaming\FileOrganizer\fileorganizer.db
```

### Database Type
```
SQLite (lightweight, file-based database)
```

### Where You See It in Code
**File:** `WpfApp1\Data\FileOrganizerContext.cs`

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
	// This is where the database path is defined:
	var dbPath = System.IO.Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),  // C:\Users\anwar\AppData\Roaming
		"FileOrganizer",                                                        // Subfolder name
		"fileorganizer.db"                                                      // Database filename
	);

	var dbDir = System.IO.Path.GetDirectoryName(dbPath);
	if (!Directory.Exists(dbDir))
		Directory.CreateDirectory(dbDir);

	optionsBuilder.UseSqlite($"Data Source={dbPath}");  // ← SQLite connection
}
```

**What This Means:**
- `Environment.SpecialFolder.ApplicationData` = Your AppData\Roaming folder
- `"FileOrganizer"` = The subfolder name
- `"fileorganizer.db"` = The database file name
- `UseSqlite()` = Tells app to use SQLite engine

---

## 📊 Database Tables Explained

### What is a TABLE?
🗂️ **Real World Analogy:** A TABLE is like a **SPREADSHEET**
- **Columns (Fields)** = Column headers (A, B, C, D...)
- **Rows (Records)** = Individual data entries
- **Cell** = One piece of data

### Visual Example:
```
TABLE: FileOrganizationRules
═══════════════════════════════════════════════════════════════

Column Names: │ Id  │ RuleName              │ FilePattern │ DestinationFolder
(FIELDS)      │     │                       │             │

Row 1         │ 3   │ AI-Suggested: Other   │ *.ini       │ C:\Videos\vid
(RECORD)      │     │                       │             │

Row 2         │ 4   │ AI-Suggested: Other   │ *.ini       │ C:\Downloads\dc
(RECORD)      │     │                       │             │

Row 3         │ 5   │ Documents             │ *.pdf       │ C:\Documents
(RECORD)      │     │                       │             │
```

---

## 🗄️ All 8 Tables in Your Database

### 1️⃣ **FileOrganizationRules** - The Master Rules

#### What It Does
Stores **ALL the rules** that define how files get organized.

#### Real-World Analogy
📚 **Library Classification System**
- Dewey Decimal: "All books about Science go in section 500-599"
- Your app: "All .pdf files go to Documents folder"

#### Code Reference
**File:** `WpfApp1\Data\FileOrganizerContext.cs`
```csharp
public DbSet<FileOrganizationRule> FileOrganizationRules { get; set; }
```

**Model File:** `WpfApp1\Models\FileOrganizationRule.cs`

#### Table Structure (Fields/Columns)
```
┌─────────────────────────────────────────────────────────────────┐
│ Table: FileOrganizationRules                                    │
├─────────────────────────────────────────────────────────────────┤
│ Field Name          │ Data Type │ Description                   │
├─────────────────────────────────────────────────────────────────┤
│ Id                  │ int       │ Unique ID (1, 2, 3...)        │
│ RuleName            │ string    │ Name like "Videos"            │
│ FilePattern         │ string    │ Pattern like "*.mp4|*.mkv"    │
│ DestinationFolder   │ string    │ Path like "C:\Videos"         │
│ IsActive            │ bool      │ 1=On, 0=Off                   │
│ CreatedDate         │ DateTime  │ When created                  │
└─────────────────────────────────────────────────────────────────┘
```

#### Example Records
```
Record 1:
├─ Id: 3
├─ RuleName: "AI-Suggested: Other"
├─ FilePattern: "*.ini"
├─ DestinationFolder: "C:\Users\anwar\Videos\vid"
├─ IsActive: 1 (True)
└─ CreatedDate: 2026-06-15 14:30:00

Record 2:
├─ Id: 4
├─ RuleName: "AI-Suggested: Other"
├─ FilePattern: "*.ini"
├─ DestinationFolder: "C:\Users\anwar\Downloads\dc"
├─ IsActive: 1 (True)
└─ CreatedDate: 2026-06-15 14:31:00
```

#### How It Works
✅ When file "config.ini" is found
✅ App checks this table for matching patterns
✅ Finds "*.ini" in Record 1 and Record 2
✅ User can choose which destination to use

---

### 2️⃣ **FileOrganizationLogs** - The History Book

#### What It Does
Records **EVERY file movement** for audit trail and troubleshooting.

#### Real-World Analogy
📋 **Bank Transaction Log**
- "User transferred $100 to John at 3:45 PM on June 15"
- Your app: "Moved photo.jpg to Pictures at 3:45 PM on June 15"

#### Code Reference
**File:** `WpfApp1\Data\FileOrganizerContext.cs`
```csharp
public DbSet<FileOrganizationLog> FileOrganizationLogs { get; set; }
```

**Model File:** `WpfApp1\Models\FileOrganizationLog.cs`

#### Table Structure
```
┌─────────────────────────────────────────────────────────────────┐
│ Table: FileOrganizationLogs                                     │
├─────────────────────────────────────────────────────────────────┤
│ Field Name          │ Data Type │ Description                   │
├─────────────────────────────────────────────────────────────────┤
│ Id                  │ int       │ Log entry ID                  │
│ SourceFilePath      │ string    │ Original location             │
│ DestinationPath     │ string    │ New location                  │
│ Timestamp           │ DateTime  │ When it happened              │
│ Status              │ string    │ "Success" or "Failed"         │
│ ErrorMessage        │ string    │ Why it failed (if failed)     │
└─────────────────────────────────────────────────────────────────┘
```

#### Example Records
```
Record 1:
├─ Id: 101
├─ SourceFilePath: "C:\Downloads\document.pdf"
├─ DestinationPath: "C:\Organized\Documents\"
├─ Timestamp: 2026-06-15 14:45:30
├─ Status: "Success"
└─ ErrorMessage: null

Record 2:
├─ Id: 102
├─ SourceFilePath: "C:\Downloads\photo.jpg"
├─ DestinationPath: "C:\Organized\Pictures\"
├─ Timestamp: 2026-06-15 14:46:15
├─ Status: "Success"
└─ ErrorMessage: null

Record 3:
├─ Id: 103
├─ SourceFilePath: "C:\Downloads\largefile.iso"
├─ DestinationPath: "C:\Organized\Archives\"
├─ Timestamp: 2026-06-15 14:47:00
├─ Status: "Failed"
└─ ErrorMessage: "Insufficient disk space"
```

#### How It Works
✅ Every time a file is moved
✅ App creates a new record in this table
✅ You can later review what happened
✅ Helps troubleshoot if file goes missing

---

### 3️⃣ **FileOrganizationSchedules** - The Calendar

#### What It Does
Stores **SCHEDULED TASKS** for automatic file organization.

#### Real-World Analogy
📅 **Garbage Collection Schedule**
- "Monday 8 AM: Collect residential waste"
- Your app: "Monday 8 AM: Run file organization"

#### Code Reference
**File:** `WpfApp1\Data\FileOrganizerContext.cs`
```csharp
public DbSet<FileOrganizationSchedule> FileOrganizationSchedules { get; set; }
```

**Service File:** `WpfApp1\Services\SchedulerService.cs`

#### Table Structure
```
┌─────────────────────────────────────────────────────────────────┐
│ Table: FileOrganizationSchedules                                │
├─────────────────────────────────────────────────────────────────┤
│ Field Name          │ Data Type │ Description                   │
├─────────────────────────────────────────────────────────────────┤
│ Id                  │ int       │ Schedule ID                   │
│ ScheduleName        │ string    │ Name like "Daily Cleanup"     │
│ RuleIds             │ string    │ Which rules to run            │
│ Frequency           │ string    │ "Daily", "Weekly", "Monthly"  │
│ ScheduledTime       │ TimeSpan  │ Time like 08:00:00            │
│ IsActive            │ bool      │ 1=Enabled, 0=Disabled         │
│ NextRunTime         │ DateTime  │ When to run next              │
│ CreatedDate         │ DateTime  │ When schedule was created     │
└─────────────────────────────────────────────────────────────────┘
```

#### Example Records
```
Record 1:
├─ Id: 1
├─ ScheduleName: "Daily Downloads Cleanup"
├─ RuleIds: "3,4,5"  (Rules 3, 4, and 5)
├─ Frequency: "Daily"
├─ ScheduledTime: 08:00:00
├─ IsActive: 1 (True)
├─ NextRunTime: 2026-06-16 08:00:00
└─ CreatedDate: 2026-06-10 10:00:00

Record 2:
├─ Id: 2
├─ ScheduleName: "Weekly Photo Backup"
├─ RuleIds: "5"
├─ Frequency: "Weekly"
├─ ScheduledTime: 19:00:00 (7 PM)
├─ IsActive: 1 (True)
├─ NextRunTime: 2026-06-20 19:00:00
└─ CreatedDate: 2026-06-10 10:00:00
```

#### How It Works
✅ `SchedulerService.cs` checks this table every 60 seconds
✅ When `NextRunTime` arrives, it runs the scheduled rules
✅ After running, updates `NextRunTime` for next execution
✅ Runs in background automatically

---

### 4️⃣ **AppSettings** - The Preferences

#### What It Does
Stores **USER PREFERENCES** and app configuration.

#### Real-World Analogy
⚙️ **Phone Settings**
- Theme: Light/Dark mode
- Language: English/Spanish
- Notifications: On/Off

#### Code Reference
**File:** `WpfApp1\Data\FileOrganizerContext.cs`
```csharp
public DbSet<AppSettings> AppSettings { get; set; }
```

**Model File:** `WpfApp1\Models\AppSettings.cs`

#### Table Structure
```
┌─────────────────────────────────────────────────────────────────┐
│ Table: AppSettings                                              │
├─────────────────────────────────────────────────────────────────┤
│ Field Name                  │ Data Type │ Description           │
├─────────────────────────────────────────────────────────────────┤
│ Id                          │ int       │ Setting ID            │
│ DefaultOrganizationFolder   │ string    │ Default start folder  │
│ Theme                       │ string    │ "Dark" or "Light"     │
│ AutoStartScheduler          │ bool      │ Auto-run on startup   │
│ NotificationsEnabled        │ bool      │ Show notifications    │
│ LastUpdated                 │ DateTime  │ When changed          │
└─────────────────────────────────────────────────────────────────┘
```

#### Example Record
```
Record 1:
├─ Id: 1
├─ DefaultOrganizationFolder: "C:\Users\anwar\Organized"
├─ Theme: "Dark"
├─ AutoStartScheduler: 1 (True)
├─ NotificationsEnabled: 1 (True)
└─ LastUpdated: 2026-06-15 10:30:00
```

#### How It Works
✅ App loads this on startup
✅ You can change settings in Settings view
✅ Changes get saved back to this table
✅ Persists across app restarts

---

### 5️⃣ **FileCategorySuggestions** - The AI Brain

#### What It Does
Stores **AI predictions** of file categories.

#### Real-World Analogy
🤖 **Netflix Recommendations**
- "Based on your history, we think you'll like this movie (92% confidence)"
- Your app: "This looks like a PDF (95% confidence → Documents)"

#### Code Reference
**File:** `WpfApp1\Data\FileOrganizerContext.cs`
```csharp
public DbSet<FileCategorySuggestion> FileCategorySuggestions { get; set; }
```

**Model File:** `WpfApp1\Models\FileCategorySuggestion.cs`

#### Table Structure
```
┌─────────────────────────────────────────────────────────────────┐
│ Table: FileCategorySuggestions                                  │
├─────────────────────────────────────────────────────────────────┤
│ Field Name              │ Data Type │ Description               │
├─────────────────────────────────────────────────────────────────┤
│ Id                      │ int       │ Suggestion ID             │
│ FileName                │ string    │ File name being analyzed  │
│ SuggestedCategory       │ string    │ What AI thinks it is      │
│ ConfidenceScore         │ decimal   │ 0.0 to 1.0 (0-100%)       │
│ DestinationFolder       │ string    │ Where AI thinks it goes   │
│ UserAccepted            │ bool      │ Did user agree? Yes/No    │
│ CreatedDate             │ DateTime  │ When suggestion was made  │
└─────────────────────────────────────────────────────────────────┘
```

#### Example Records
```
Record 1:
├─ Id: 1
├─ FileName: "vacation.jpg"
├─ SuggestedCategory: "Images"
├─ ConfidenceScore: 0.98 (98%)
├─ DestinationFolder: "C:\Pictures\2026\"
├─ UserAccepted: 1 (Yes, user clicked Accept ✓)
└─ CreatedDate: 2026-06-15 14:30:00

Record 2:
├─ Id: 2
├─ FileName: "invoice_2026.xlsx"
├─ SuggestedCategory: "Finance"
├─ ConfidenceScore: 0.89 (89%)
├─ DestinationFolder: "C:\Finance\2026\"
├─ UserAccepted: 1 (Yes)
└─ CreatedDate: 2026-06-15 14:31:00

Record 3:
├─ Id: 3
├─ FileName: "unknown_file.dat"
├─ SuggestedCategory: "Other"
├─ ConfidenceScore: 0.45 (45% - LOW CONFIDENCE)
├─ DestinationFolder: "C:\Other\"
├─ UserAccepted: 0 (No, user rejected ✗)
└─ CreatedDate: 2026-06-15 14:32:00
```

#### How It Works
✅ ML Engine analyzes new files
✅ Creates suggestions in this table
✅ Shows suggestions to user in UI
✅ Records if user accepted/rejected
✅ Uses feedback to improve future predictions

---

### 6️⃣ **SmartSuggestionPatterns** - AI Memory

#### What It Does
Stores **PATTERNS the AI LEARNED** from your data.

#### Real-World Analogy
🧠 **Doctor's Experience**
- Doctor sees 1000 patients
- Learns: "Red throat + fever = Strep throat"
- This pattern is stored in memory
- Next patient with same symptoms → Diagnose faster

#### Code Reference
**File:** `WpfApp1\Data\FileOrganizerContext.cs`
```csharp
public DbSet<SmartSuggestionPattern> SmartSuggestionPatterns { get; set; }
```

**Service File:** `WpfApp1\Services\MLModelService.cs`

#### Table Structure
```
┌─────────────────────────────────────────────────────────────────┐
│ Table: SmartSuggestionPatterns                                  │
├─────────────────────────────────────────────────────────────────┤
│ Field Name              │ Data Type │ Description               │
├─────────────────────────────────────────────────────────────────┤
│ Id                      │ int       │ Pattern ID                │
│ FilePattern             │ string    │ File types (*.pdf|*.doc)  │
│ Category                │ string    │ Category they go to       │
│ Frequency               │ int       │ How many times seen       │
│ ConfidenceScore         │ decimal   │ 0.0 to 1.0 accuracy       │
│ Enabled                 │ bool      │ Use this pattern?         │
└─────────────────────────────────────────────────────────────────┘
```

#### Example Records
```
Record 1:
├─ Id: 1
├─ FilePattern: "*.pdf|*.doc|*.docx"
├─ Category: "Documents"
├─ Frequency: 247 (Seen 247 times)
├─ ConfidenceScore: 0.99 (99% - VERY CONFIDENT)
└─ Enabled: 1 (Yes, use this pattern)

Record 2:
├─ Id: 2
├─ FilePattern: "*.jpg|*.png|*.gif|*.bmp"
├─ Category: "Images"
├─ Frequency: 512 (Seen 512 times)
├─ ConfidenceScore: 0.98 (98%)
└─ Enabled: 1

Record 3:
├─ Id: 3
├─ FilePattern: "*.mp3|*.wav|*.m4a"
├─ Category: "Audio"
├─ Frequency: 89 (Seen 89 times)
├─ ConfidenceScore: 0.96 (96%)
└─ Enabled: 1

Record 4:
├─ Id: 4
├─ FilePattern: "*.exe|*.msi"
├─ Category: "Applications"
├─ Frequency: 156
├─ ConfidenceScore: 0.97 (97%)
└─ Enabled: 1
```

#### How It Works
✅ When you create rules, ML engine learns patterns
✅ Stores learned patterns in this table
✅ Frequency increases each time pattern is seen
✅ Next file with *.pdf comes → Instantly suggests "Documents"
✅ AI gets smarter the more you use it

---

### 7️⃣ **ExclusionPatterns** - The Blacklist

#### What It Does
Stores file patterns to **IGNORE/SKIP** during organization.

#### Real-World Analogy
🚫 **Airport Security Whitelist**
- "These people are officials - do NOT stop them"
- Your app: "These files are system files - do NOT move them"

#### Code Reference
**File:** `WpfApp1\Data\FileOrganizerContext.cs`
```csharp
// Not explicitly shown, but should be:
public DbSet<ExclusionPattern> ExclusionPatterns { get; set; }
```

#### Table Structure
```
┌─────────────────────────────────────────────────────────────────┐
│ Table: ExclusionPatterns                                        │
├─────────────────────────────────────────────────────────────────┤
│ Field Name              │ Data Type │ Description               │
├─────────────────────────────────────────────────────────────────┤
│ Id                      │ int       │ Exclusion ID              │
│ Pattern                 │ string    │ File pattern (*.tmp)      │
│ PatternType             │ string    │ "Extension" or "Folder"   │
│ Reason                  │ string    │ Why excluded              │
│ Enabled                 │ bool      │ Is this rule active?      │
└─────────────────────────────────────────────────────────────────┘
```

#### Example Records
```
Record 1:
├─ Id: 1
├─ Pattern: "*.tmp"
├─ PatternType: "Extension"
├─ Reason: "Temporary files - auto-deleted anyway"
└─ Enabled: 1

Record 2:
├─ Id: 2
├─ Pattern: "Thumbs.db"
├─ PatternType: "Filename"
├─ Reason: "Windows cache file - leave alone"
└─ Enabled: 1

Record 3:
├─ Id: 3
├─ Pattern: ".git"
├─ PatternType: "Folder"
├─ Reason: "Version control - preserve structure"
└─ Enabled: 1

Record 4:
├─ Id: 4
├─ Pattern: "System Volume*"
├─ PatternType: "Folder"
├─ Reason: "Windows system - protected"
└─ Enabled: 1
```

#### How It Works
✅ When organizing files, app checks this table first
✅ If file matches exclusion pattern, SKIP IT
✅ Protects important system files
✅ Prevents accidental file loss

---

### 8️⃣ **sqlite_sequence** - Internal (IGNORE THIS)

#### What It Does
**SQLite's internal counter** for auto-incrementing IDs.

#### Real-World Analogy
🎫 **Queue Number System**
- Next ticket: #127
- So customer gets #127, then #128, etc.

#### Table Structure
```
┌─────────────────────────────────────────────────────┐
│ Table: sqlite_sequence                              │
├─────────────────────────────────────────────────────┤
│ Field Name  │ Data Type │ Description               │
├─────────────────────────────────────────────────────┤
│ name        │ string    │ Table name                │
│ seq         │ int       │ Next ID number            │
└─────────────────────────────────────────────────────┘
```

#### Example
```
name                        │ seq
FileOrganizationRules       │ 6
FileOrganizationLogs        │ 200
FileOrganizationSchedules   │ 4
AppSettings                 │ 1
FileCategorySuggestions     │ 156
SmartSuggestionPatterns     │ 12
ExclusionPatterns           │ 8
```

#### How It Works
✅ When you create new rule, gets ID #6
✅ Next rule gets #7
✅ SQLite uses this to auto-generate unique IDs
✅ **You never touch this table manually**

---

## 🔄 The Complete Workflow

### Step-by-Step Workflow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ STEP 1: USER CREATES A RULE                                │
│ (In RuleManagementView.xaml)                                │
│                                                              │
│ User enters:                                                │
│ ├─ Rule Name: "Videos"                                     │
│ ├─ File Pattern: "*.mp4|*.mkv"                             │
│ ├─ Destination: "C:\Videos"                                │
│ └─ Clicks: + Add Rule button                               │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 2: APP SAVES RULE                                     │
│ (In RuleManagementService.CreateRule)                      │
│                                                              │
│ New record inserted into:                                   │
│ FileOrganizationRules table                                │
│ ├─ Id: 6 (auto-generated from sqlite_sequence)             │
│ ├─ RuleName: "Videos"                                      │
│ ├─ FilePattern: "*.mp4|*.mkv"                              │
│ ├─ DestinationFolder: "C:\Videos"                          │
│ ├─ IsActive: 1 (True)                                      │
│ └─ CreatedDate: NOW                                        │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 3: AI LEARNS                                          │
│ (In MLModelService.TrainModelFromExistingRules)            │
│                                                              │
│ AI engine reads all rules and learns:                      │
│ "Files with .mp4 or .mkv should go to Videos"             │
│                                                              │
│ New record inserted into:                                   │
│ SmartSuggestionPatterns table                              │
│ ├─ Id: 7                                                   │
│ ├─ FilePattern: "*.mp4|*.mkv"                              │
│ ├─ Category: "Videos"                                      │
│ ├─ Frequency: 1 (first time seeing this)                   │
│ └─ ConfidenceScore: 0.95                                   │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 4: SCHEDULE ORGANIZATION                              │
│ (In SchedulerView.xaml)                                    │
│                                                              │
│ User sets up a schedule:                                   │
│ ├─ Name: "Daily Video Cleanup"                            │
│ ├─ Frequency: Daily                                        │
│ ├─ Time: 8:00 AM                                           │
│ └─ Rules: Rule 6 (Videos)                                  │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 5: SCHEDULE SAVED                                     │
│ (In SchedulerService.CreateSchedule)                      │
│                                                              │
│ New record inserted into:                                   │
│ FileOrganizationSchedules table                            │
│ ├─ Id: 3                                                   │
│ ├─ ScheduleName: "Daily Video Cleanup"                    │
│ ├─ RuleIds: "6"                                            │
│ ├─ Frequency: "Daily"                                      │
│ ├─ ScheduledTime: 08:00:00                                │
│ ├─ IsActive: 1 (True)                                      │
│ └─ NextRunTime: Tomorrow 08:00:00                         │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 6: FILE ARRIVES                                       │
│ (File system monitoring)                                    │
│                                                              │
│ New file appears in Downloads:                              │
│ "movie.mp4"                                                 │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 7: AI ANALYZES                                        │
│ (In SmartFileCategorizerEngine)                            │
│                                                              │
│ AI checks SmartSuggestionPatterns table                    │
│ Finds: "*.mp4 matches Videos pattern (95% confidence)"     │
│                                                              │
│ New record inserted into:                                   │
│ FileCategorySuggestions table                              │
│ ├─ Id: 42                                                  │
│ ├─ FileName: "movie.mp4"                                   │
│ ├─ SuggestedCategory: "Videos"                             │
│ ├─ ConfidenceScore: 0.95                                   │
│ └─ DestinationFolder: "C:\Videos"                          │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 8: SCHEDULED TIME ARRIVES (8:00 AM)                   │
│ (SchedulerService.OnScheduleTimerElapsed)                  │
│                                                              │
│ Timer check:                                                │
│ ├─ Reads FileOrganizationSchedules                         │
│ ├─ Finds Rule 6 "Daily Video Cleanup" due now              │
│ ├─ Reads FileOrganizationRules for Rule 6                  │
│ ├─ Gets pattern "*.mp4|*.mkv"                              │
│ ├─ Finds "movie.mp4" in Downloads                          │
│ └─ Checks ExclusionPatterns (not excluded ✓)              │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 9: FILE MOVES                                         │
│ (FileOrganizationService.MoveFile)                         │
│                                                              │
│ Movie.mp4 moves from:                                       │
│ C:\Users\anwar\Downloads\movie.mp4                         │
│        ↓↓↓ TO ↓↓↓                                           │
│ C:\Videos\movie.mp4                                         │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 10: LOG RECORDED                                      │
│ (In FileOrganizationService.OrganizeFiles)                 │
│                                                              │
│ New record inserted into:                                   │
│ FileOrganizationLogs table                                 │
│ ├─ Id: 156                                                 │
│ ├─ SourceFilePath: "C:\Users\anwar\Downloads\movie.mp4"   │
│ ├─ DestinationPath: "C:\Videos\movie.mp4"                 │
│ ├─ Timestamp: 2026-06-16 08:00:45                         │
│ ├─ Status: "Success"                                      │
│ └─ ErrorMessage: null                                      │
└──────────────────────────────┬──────────────────────────────┘
								↓
┌─────────────────────────────────────────────────────────────┐
│ STEP 11: FEEDBACK LOOP                                     │
│ (Smart Learning)                                            │
│                                                              │
│ App updates:                                                │
│ SmartSuggestionPatterns                                    │
│ ├─ Frequency: increased from 1 to 2                        │
│ ├─ ConfidenceScore: increased from 0.95 to 0.96           │
│ └─ (AI becomes SMARTER!)                                   │
└─────────────────────────────────────────────────────────────┘

							✨ DONE! ✨
		 File is organized, AI learned, history recorded
```

---

## 📝 Where You See This in Code

### Database Configuration
**File:** `WpfApp1\Data\FileOrganizerContext.cs`
```csharp
public class FileOrganizerContext : DbContext
{
	// These DbSet properties represent each table
	public DbSet<FileOrganizationRule> FileOrganizationRules { get; set; }
	public DbSet<FileOrganizationLog> FileOrganizationLogs { get; set; }
	public DbSet<FileOrganizationSchedule> FileOrganizationSchedules { get; set; }
	public DbSet<AppSettings> AppSettings { get; set; }
	public DbSet<FileCategorySuggestion> FileCategorySuggestions { get; set; }
	public DbSet<SmartSuggestionPattern> SmartSuggestionPatterns { get; set; }
	// ExclusionPatterns would go here too

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var dbPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"FileOrganizer",
			"fileorganizer.db"  // ← Database file
		);
		optionsBuilder.UseSqlite($"Data Source={dbPath}");
	}
}
```

### How to Query (Examples)

**Get all rules:**
```csharp
var rules = _dbContext.FileOrganizationRules.ToList();
```

**Get active rules only:**
```csharp
var activeRules = _dbContext.FileOrganizationRules
	.Where(r => r.IsActive == true)
	.ToList();
```

**Get recent logs:**
```csharp
var recentLogs = _dbContext.FileOrganizationLogs
	.OrderByDescending(l => l.Timestamp)
	.Take(10)
	.ToList();
```

**Get AI suggestions:**
```csharp
var suggestions = _dbContext.FileCategorySuggestions
	.Where(s => s.ConfidenceScore > 0.8m)  // Over 80% confidence
	.ToList();
```

---

## 🎓 Quick Reference Table

| Term | Definition | Example |
|------|-----------|---------|
| **Database** | Collection of related tables | fileorganizer.db |
| **Table** | Collection of records | FileOrganizationRules |
| **Record/Row** | Single entry in a table | Rule ID 3 |
| **Field/Column** | Property of a record | RuleName, FilePattern |
| **Cell/Value** | Single data point | "Videos", "*.mp4" |
| **Primary Key** | Unique identifier | Id (1, 2, 3...) |
| **Foreign Key** | Link to another table | RuleIds in Schedules |
| **Data Type** | What kind of data | int, string, bool, DateTime |

---

## 💾 Summary: Your Complete System

```
DATABASE: fileorganizer.db
├── LOCATION: C:\Users\anwar\AppData\Roaming\FileOrganizer\
├── TYPE: SQLite
└── TABLES:
	├── FileOrganizationRules (Stores your rules)
	├── FileOrganizationLogs (Stores history)
	├── FileOrganizationSchedules (Stores schedules)
	├── AppSettings (Stores preferences)
	├── FileCategorySuggestions (Stores AI predictions)
	├── SmartSuggestionPatterns (Stores AI learning)
	├── ExclusionPatterns (Stores blacklist)
	└── sqlite_sequence (Internal counter)

WORKFLOW:
1. You create a rule
2. Rule saved to database
3. AI learns the pattern
4. Schedule set up
5. When time arrives, app runs
6. File matches pattern
7. AI analyzes file
8. File moves
9. Movement logged
10. AI gets smarter from feedback
```

---

You now understand your entire database! 🎉 Every table, every field, the complete workflow, and how it all connects!
