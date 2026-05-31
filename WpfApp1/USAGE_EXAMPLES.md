# Usage Examples - Smart File Organizer

This document provides practical code examples for using the Smart File Organizer.

## Table of Contents
1. [File Organization](#file-organization)
2. [Rule Management](#rule-management)
3. [Cloud Integration](#cloud-integration)
4. [Scheduling](#scheduling)

---

## File Organization

### Organize Files in a Folder

```csharp
using WpfApp1.Services;
using WpfApp1.Data;

// Get services
var dbContext = DbContextService.GetInstance();
var organizationService = new FileOrganizationService(dbContext);

// Organize files
var result = organizationService.OrganizeFiles(
	sourceFolder: "C:\\Downloads",
	moveFiles: true  // true to move, false to copy
);

// Check results
Console.WriteLine($"Successfully organized: {result.SuccessCount}");
Console.WriteLine($"Failed: {result.FailureCount}");
Console.WriteLine($"Skipped: {result.SkippedCount}");

// View messages
foreach (var message in result.Messages)
{
	Console.WriteLine(message);
}
```

### Check if File Matches Rule

```csharp
var rule = new FileOrganizationRule
{
	FilePattern = "*.pdf",
	DestinationFolder = "C:\\Documents"
};

bool matches = organizationService.MatchesRule("document.pdf", rule);
// Result: true

bool matches2 = organizationService.MatchesRule("image.jpg", rule);
// Result: false
```

### Get All Files from Folder

```csharp
var files = organizationService.GetAllFilesInFolder("C:\\MyFolder");
Console.WriteLine($"Found {files.Count} files");

foreach (var file in files)
{
	Console.WriteLine(file);  // Full path
}
```

---

## Rule Management

### Create a New Rule

```csharp
using WpfApp1.Services;

var ruleService = new RuleManagementService(dbContext);

var rule = ruleService.CreateRule(
	ruleName: "PDF Documents",
	filePattern: "*.pdf",
	destinationFolder: "C:\\Documents\\PDFs",
	isActive: true
);

Console.WriteLine($"Created rule: {rule.RuleName}");
```

### Get All Rules

```csharp
var allRules = ruleService.GetAllRules();

// Filter active rules
var activeRules = allRules.Where(r => r.IsActive).ToList();
Console.WriteLine($"Active rules: {activeRules.Count}");
```

### Update a Rule

```csharp
var updated = ruleService.UpdateRule(
	ruleId: rule.Id,
	ruleName: "Updated PDF Rule",
	filePattern: "*.pdf",
	destinationFolder: "C:\\Backup\\PDFs",
	isActive: true
);

Console.WriteLine($"Updated: {updated.RuleName}");
```

### Delete a Rule

```csharp
bool success = ruleService.DeleteRule(rule.Id);
if (success)
{
	Console.WriteLine("Rule deleted successfully");
}
```

---

## Cloud Integration

### Connect to Google Drive

```csharp
using WpfApp1.Services;

var helper = new CloudIntegrationHelper(dbContext);

// Authenticate
var (success, message) = await helper.AuthenticateGoogleDriveAsync();

if (success)
{
	Console.WriteLine("Connected to Google Drive!");
}
else
{
	Console.WriteLine($"Error: {message}");
}
```

### Check Authentication Status

```csharp
bool isAuthenticated = helper.IsAuthenticated();
Console.WriteLine($"Authenticated: {isAuthenticated}");

// Get detailed status
var status = helper.GetAuthStatus();
Console.WriteLine($"Email: {status.UserEmail}");
Console.WriteLine($"Last Sync: {status.LastSyncDate}");
Console.WriteLine($"Files Synced: {status.TotalFilesSynced}");
```

### List Cloud Files

```csharp
var (success, files, message) = await helper.GetCloudFilesAsync();

if (success)
{
	Console.WriteLine($"Found {files.Count} files in Google Drive");

	foreach (var file in files)
	{
		Console.WriteLine($"- {file.Name} ({file.Size} bytes)");
	}
}
else
{
	Console.WriteLine($"Error: {message}");
}
```

### Disconnect from Google Drive

```csharp
var (success, message) = await helper.DisconnectGoogleDriveAsync();
if (success)
{
	Console.WriteLine("Disconnected successfully");
}
```

### Update Sync Statistics

```csharp
// After organizing files
helper.UpdateSyncStats(fileCount: 15);
Console.WriteLine("Sync statistics updated");
```

---

## Scheduling

### Create a Schedule

```csharp
using WpfApp1.Services;

var schedulerService = new SchedulerService(dbContext, organizationService);

// Create daily schedule at 2:00 PM
var schedule = schedulerService.CreateSchedule(
	scheduleName: "Daily Downloads Cleanup",
	sourceFolder: "C:\\Downloads",
	scheduleType: "daily",
	scheduleTime: new DateTime(
		DateTime.Now.Year, 
		DateTime.Now.Month, 
		DateTime.Now.Day, 
		14, 0, 0  // 2:00 PM
	),
	isActive: true
);

Console.WriteLine($"Schedule created: {schedule.ScheduleName}");
```

### Start Scheduler

```csharp
schedulerService.StartScheduler();
Console.WriteLine("Scheduler started - will run every 60 seconds");
```

### Stop Scheduler

```csharp
schedulerService.StopScheduler();
Console.WriteLine("Scheduler stopped");
```

### Get All Schedules

```csharp
var allSchedules = schedulerService.GetAllSchedules();
Console.WriteLine($"Total schedules: {allSchedules.Count}");

var activeSchedules = allSchedules.Where(s => s.IsActive).ToList();
Console.WriteLine($"Active schedules: {activeSchedules.Count}");
```

---

## Complete Example: Full Workflow

```csharp
using WpfApp1.Services;
using WpfApp1.Data;

class Program
{
	static async Task Main(string[] args)
	{
		// Setup
		var dbContext = DbContextService.GetInstance();
		var ruleService = new RuleManagementService(dbContext);
		var fileService = new FileOrganizationService(dbContext);
		var cloudHelper = new CloudIntegrationHelper(dbContext);
		var schedulerService = new SchedulerService(dbContext, fileService);

		// Step 1: Create rules
		Console.WriteLine("Creating rules...");
		ruleService.CreateRule("PDFs", "*.pdf", "C:\\Documents\\PDFs", true);
		ruleService.CreateRule("Images", "*.jpg", "C:\\Pictures", true);
		ruleService.CreateRule("Videos", "*.mp4", "C:\\Videos", true);

		// Step 2: Organize local files
		Console.WriteLine("Organizing files...");
		var result = fileService.OrganizeFiles("C:\\Downloads", moveFiles: true);
		Console.WriteLine($"Organized: {result.SuccessCount} files");

		// Step 3: Connect to Google Drive (if needed)
		Console.WriteLine("Connecting to Google Drive...");
		var (success, msg) = await cloudHelper.AuthenticateGoogleDriveAsync();
		if (success)
		{
			Console.WriteLine("Google Drive connected!");

			var (ok, files, _) = await cloudHelper.GetCloudFilesAsync();
			if (ok)
			{
				Console.WriteLine($"Cloud files: {files.Count}");
			}
		}

		// Step 4: Setup scheduling
		Console.WriteLine("Creating schedule...");
		var schedule = schedulerService.CreateSchedule(
			"Daily Cleanup",
			"C:\\Downloads",
			"daily",
			DateTime.Now.AddHours(1),
			true
		);

		schedulerService.StartScheduler();
		Console.WriteLine("Scheduler running...");

		// Keep running
		Console.WriteLine("Press any key to stop...");
		Console.ReadKey();

		schedulerService.StopScheduler();
		dbContext.Dispose();
	}
}
```

---

## Error Handling

Always wrap service calls in try-catch:

```csharp
try
{
	var result = fileService.OrganizeFiles(folderPath, true);
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
	Console.WriteLine($"Error: {ex.Message}");
}
```

---

## Tips

1. **Always dispose DbContext**: Use `using` statement or call `Dispose()`
2. **Async operations**: Cloud operations are async - use `await`
3. **File paths**: Use `Path.Combine()` for cross-platform compatibility
4. **Error messages**: Check result messages for details
5. **Logging**: Service calls provide detailed messages in results

---

For more information, see:
- `DOCUMENTATION.md` - System documentation
- `API_REFERENCE.md` - Complete API reference
- `CLOUD_INTEGRATION_SETUP.md` - Cloud setup guide

