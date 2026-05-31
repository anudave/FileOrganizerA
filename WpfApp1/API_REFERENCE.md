# API Reference Guide

## Table of Contents
- [Services API](#services-api)
- [Models](#models)
- [DbContext](#dbcontext)
- [Exceptions](#exceptions)

---

## Services API

### FileOrganizationService

**Namespace:** `WpfApp1.Services`

#### Methods

##### `OrganizeFiles(string sourceFolder, bool moveFiles)`

Organize files in a folder based on active rules.

```csharp
public FileOrganizationService.OrganizationResult OrganizeFiles(
	string sourceFolder,        // Path to source folder
	bool moveFiles = true       // true=move, false=copy
)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `sourceFolder` | string | Path to folder containing files to organize |
| `moveFiles` | bool | If true, moves files; if false, copies them |

**Returns:** `OrganizationResult`
- `SuccessCount` - Number of successfully organized files
- `FailureCount` - Number of failed operations
- `SkippedCount` - Number of skipped files
- `Messages` - List of operation messages

**Throws:**
- `DirectoryNotFoundException` - If source folder doesn't exist
- `UnauthorizedAccessException` - If access denied

**Example:**
```csharp
var result = service.OrganizeFiles("C:\\Downloads", moveFiles: true);
if (result.SuccessCount > 0)
{
	Console.WriteLine($"Organized {result.SuccessCount} files");
}
```

---

##### `MatchesRule(string fileName, FileOrganizationRule rule)`

Check if a filename matches a rule pattern.

```csharp
public bool MatchesRule(
	string fileName,                    // Filename to check
	FileOrganizationRule rule           // Rule to match against
)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `fileName` | string | Name of file to check (e.g., "document.pdf") |
| `rule` | FileOrganizationRule | Rule containing pattern to match |

**Returns:** `bool` - true if file matches pattern, false otherwise

**Example:**
```csharp
var rule = new FileOrganizationRule { FilePattern = "*.pdf" };
bool matches = service.MatchesRule("file.pdf", rule);  // true
```

**Supported Patterns:**
- `*.pdf` - All PDF files
- `*.doc*` - All Word documents (.doc, .docx)
- `file.*` - All files named 'file'

---

##### `GetAllFilesInFolder(string folderPath)`

Get list of all files in a folder recursively.

```csharp
public List<string> GetAllFilesInFolder(string folderPath)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `folderPath` | string | Path to folder to scan |

**Returns:** `List<string>` - List of full file paths

**Example:**
```csharp
var files = service.GetAllFilesInFolder("C:\\Documents");
Console.WriteLine($"Found {files.Count} files");
```

---

### RuleManagementService

**Namespace:** `WpfApp1.Services`

#### Methods

##### `CreateRule(string ruleName, string filePattern, string destinationFolder, bool isActive)`

Create a new organization rule.

```csharp
public FileOrganizationRule CreateRule(
	string ruleName,            // Name of the rule
	string filePattern,         // File pattern (e.g., "*.pdf")
	string destinationFolder,   // Target folder path
	bool isActive = true        // Whether rule is active
)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `ruleName` | string | Display name for the rule |
| `filePattern` | string | Wildcard pattern to match files |
| `destinationFolder` | string | Full path where files go |
| `isActive` | bool | Enable/disable rule |

**Returns:** `FileOrganizationRule` - Created rule object

**Throws:**
- `InvalidOperationException` - If rule name already exists
- `ArgumentNullException` - If required parameters are null

**Example:**
```csharp
var rule = service.CreateRule(
	"PDF Documents",
	"*.pdf",
	"C:\\Documents\\PDFs",
	true
);
```

---

##### `GetAllRules()`

Get all organization rules.

```csharp
public List<FileOrganizationRule> GetAllRules()
```

**Returns:** `List<FileOrganizationRule>` - All rules in database

**Example:**
```csharp
var rules = service.GetAllRules();
var activeRules = rules.Where(r => r.IsActive).ToList();
```

---

##### `UpdateRule(int id, string ruleName, string filePattern, string destinationFolder, bool isActive)`

Update an existing rule.

```csharp
public FileOrganizationRule UpdateRule(
	int id,                     // Rule ID to update
	string ruleName,            // New rule name
	string filePattern,         // New file pattern
	string destinationFolder,   // New destination
	bool isActive               // New active status
)
```

**Returns:** `FileOrganizationRule` - Updated rule, or null if not found

**Example:**
```csharp
var updated = service.UpdateRule(
	1,
	"PDF Docs",
	"*.pdf",
	"C:\\Backup\\PDFs",
	true
);
```

---

##### `DeleteRule(int id)`

Delete a rule.

```csharp
public bool DeleteRule(int id)
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `id` | int | ID of rule to delete |

**Returns:** `bool` - true if deleted, false if not found

**Example:**
```csharp
bool success = service.DeleteRule(1);
if (success) Console.WriteLine("Rule deleted");
```

---

##### `GetRule(int id)`

Get a specific rule by ID.

```csharp
public FileOrganizationRule GetRule(int id)
```

**Returns:** `FileOrganizationRule` - Rule object, or null if not found

**Example:**
```csharp
var rule = service.GetRule(1);
if (rule != null)
	Console.WriteLine($"Rule: {rule.RuleName}");
```

---

### SchedulerService

**Namespace:** `WpfApp1.Services`

#### Methods

##### `StartScheduler()`

Start the scheduler to check and execute schedules.

```csharp
public void StartScheduler()
```

**Remarks:** Runs continuously, checking every 60 seconds

**Example:**
```csharp
var scheduler = new SchedulerService(dbContext, fileOrgService);
scheduler.StartScheduler();
```

---

##### `StopScheduler()`

Stop the scheduler.

```csharp
public void StopScheduler()
```

**Example:**
```csharp
scheduler.StopScheduler();
```

---

##### `CreateSchedule(string scheduleName, string sourceFolder, string scheduleType, DateTime scheduleTime, bool isActive)`

Create a new automation schedule.

```csharp
public FileOrganizationSchedule CreateSchedule(
	string scheduleName,        // Name of schedule
	string sourceFolder,        // Folder to organize
	string scheduleType,        // "daily", "weekly", "monthly", "custom"
	DateTime scheduleTime,      // When to run
	bool isActive               // Enable/disable
)
```

**Schedule Types:**
| Type | Description | Example |
|------|-------------|---------|
| daily | Run every day | 2:00 PM |
| weekly | Run specific day | Monday 2:00 PM |
| monthly | Run specific date | 15th of month |
| custom | Custom interval | Every 6 hours |

**Example:**
```csharp
var schedule = scheduler.CreateSchedule(
	"Weekly Cleanup",
	"C:\\Downloads",
	"weekly",
	new DateTime(DateTime.Now.Year, DateTime.Now.Month, 
				 DateTime.Now.Day, 14, 0, 0),  // 2:00 PM
	true
);
```

---

##### `GetAllSchedules()`

Get all schedules.

```csharp
public List<FileOrganizationSchedule> GetAllSchedules()
```

**Returns:** `List<FileOrganizationSchedule>` - All schedules

---

##### `DeleteSchedule(int id)`

Delete a schedule.

```csharp
public bool DeleteSchedule(int id)
```

**Returns:** `bool` - true if deleted, false if not found

---

### CloudIntegrationHelper

**Namespace:** `WpfApp1.Services`

#### Methods

##### `AuthenticateGoogleDriveAsync()`

Authenticate with Google Drive.

```csharp
public async Task<(bool success, string message)> AuthenticateGoogleDriveAsync()
```

**Returns:** Tuple with:
- `success` - true if authentication succeeded
- `message` - Status message

**Example:**
```csharp
var helper = new CloudIntegrationHelper(dbContext);
var (success, message) = await helper.AuthenticateGoogleDriveAsync();
if (success)
	Console.WriteLine("Authenticated!");
else
	Console.WriteLine($"Error: {message}");
```

---

##### `IsAuthenticated()`

Check if user is authenticated.

```csharp
public bool IsAuthenticated()
```

**Returns:** `bool` - true if connected to Google Drive

---

##### `GetCloudFilesAsync()`

Get list of cloud files.

```csharp
public async Task<(bool success, List<CloudFileInfo> files, string message)> 
	GetCloudFilesAsync()
```

**Returns:** Tuple with:
- `success` - Operation success
- `files` - List of CloudFileInfo
- `message` - Status message

**CloudFileInfo Properties:**
```csharp
public class CloudFileInfo
{
	public string Id { get; set; }              // Google Drive file ID
	public string Name { get; set; }            // File name
	public long Size { get; set; }              // File size in bytes
	public DateTime ModifiedTime { get; set; }  // Last modified
	public string MimeType { get; set; }        // File type
	public string WebViewLink { get; set; }     // Online link
}
```

---

##### `GetAuthStatus()`

Get detailed authentication status.

```csharp
public CloudAuthStatus GetAuthStatus()
```

**Returns:** `CloudAuthStatus` with properties:
- `IsAuthenticated` - Connected or not
- `UserEmail` - Gmail address
- `ConnectedDate` - When connected
- `LastSyncDate` - Last organization
- `TotalFilesSynced` - Number of files organized
- `IsTokenExpired` - Token validity

---

## Models

### FileOrganizationRule

```csharp
public class FileOrganizationRule
{
	public int Id { get; set; }                  // Primary key
	public string RuleName { get; set; }         // Rule name
	public string FilePattern { get; set; }      // Wildcard pattern
	public string DestinationFolder { get; set; } // Target folder
	public bool IsActive { get; set; }           // Enable/disable
	public DateTime CreatedDate { get; set; }    // Creation time
}
```

---

### FileOrganizationSchedule

```csharp
public class FileOrganizationSchedule
{
	public int Id { get; set; }                  // Primary key
	public string ScheduleName { get; set; }     // Schedule name
	public string SourceFolder { get; set; }     // Folder to organize
	public string ScheduleType { get; set; }     // daily/weekly/monthly/custom
	public DateTime ScheduleTime { get; set; }   // Execution time
	public DateTime? LastExecutedDate { get; set; } // Last run
	public bool IsActive { get; set; }           // Enable/disable
	public DateTime CreatedDate { get; set; }    // Creation time
}
```

---

### CloudStorageAccount

```csharp
public class CloudStorageAccount
{
	public int Id { get; set; }                  // Primary key
	public string Provider { get; set; }         // "Google"
	public string UserEmail { get; set; }        // Email address
	public string AccessToken { get; set; }      // OAuth token
	public string RefreshToken { get; set; }     // Refresh token
	public DateTime TokenExpiresAt { get; set; } // Token expiry
	public bool IsConnected { get; set; }        // Connection status
	public DateTime ConnectedDate { get; set; }  // Connect time
	public DateTime? LastSyncDate { get; set; }  // Last sync
	public int TotalFilesSynced { get; set; }    // Total files organized
}
```

---

## DbContext

### FileOrganizerContext

**Namespace:** `WpfApp1.Data`

```csharp
public class FileOrganizerContext : DbContext
{
	public DbSet<FileOrganizationRule> FileOrganizationRules { get; set; }
	public DbSet<FileOrganizationSchedule> FileOrganizationSchedules { get; set; }
	public DbSet<FileOrganizationLog> FileOrganizationLogs { get; set; }
	public DbSet<CloudStorageAccount> CloudStorageAccounts { get; set; }
	public DbSet<AppSettings> AppSettings { get; set; }
}
```

**Usage:**
```csharp
var context = DbContextService.GetInstance();
var rules = context.FileOrganizationRules.ToList();
```

---

## Exceptions

### Common Exceptions

| Exception | Cause | Handling |
|-----------|-------|----------|
| `DirectoryNotFoundException` | Folder doesn't exist | Check path before using |
| `UnauthorizedAccessException` | No permission to access | Check folder permissions |
| `InvalidOperationException` | Invalid operation | Check business rules |
| `FileNotFoundException` | File doesn't exist | Verify file exists |
| `ArgumentNullException` | Required parameter null | Validate inputs |

**Example Error Handling:**
```csharp
try
{
	var result = service.OrganizeFiles(folderPath);
}
catch (DirectoryNotFoundException ex)
{
	Console.WriteLine($"Folder not found: {ex.Message}");
}
catch (UnauthorizedAccessException ex)
{
	Console.WriteLine($"Access denied: {ex.Message}");
}
catch (Exception ex)
{
	Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

---

## Best Practices

### Input Validation
```csharp
// Always validate paths
if (string.IsNullOrWhiteSpace(folderPath))
	throw new ArgumentNullException(nameof(folderPath));

if (!Directory.Exists(folderPath))
	throw new DirectoryNotFoundException($"Path: {folderPath}");
```

### Resource Cleanup
```csharp
// Dispose context when done
using (var context = new FileOrganizerContext())
{
	// Use context
}  // Automatically disposed
```

### Async Operations
```csharp
// Use async for cloud operations
var files = await cloudHelper.GetCloudFilesAsync();
```

---

**Last Updated:** January 21, 2025  
**API Version:** 1.0

