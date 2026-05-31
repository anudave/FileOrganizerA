# Smart File Organizer - Comprehensive Documentation

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture](#architecture)
3. [Features](#features)
4. [Technology Stack](#technology-stack)
5. [Core Components](#core-components)
6. [Usage Guide](#usage-guide)
7. [Development Setup](#development-setup)
8. [Testing](#testing)
9. [Troubleshooting](#troubleshooting)

---

## System Overview

### Problem Statement

Managing files across multiple folders and cloud storage manually is time-consuming and error-prone. The **Smart File Organizer** automates file organization and management with:

- **Rule-based organization:** Define patterns to automatically categorize files
- **Scheduled automation:** Run organization tasks on a schedule
- **Cloud storage integration:** Organize files in Google Drive
- **Tracking and analytics:** Monitor organization history and statistics

### Solution

A WPF desktop application that provides:

1. **Local File Organization** - Organize files on your computer based on rules
2. **Cloud Storage Integration** - Connect to and organize Google Drive files
3. **Automation & Scheduling** - Set up automatic file organization schedules
4. **Analytics & Tracking** - Monitor file organization history

---

## Architecture

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                   Presentation Layer (WPF)                  │
├─────────────────────────────────────────────────────────────┤
│  MainWindow  │  FileOrganizationView  │  CloudOrganizationView │
│  RuleView    │  SchedulerView         │  SettingsView       │
│  AnalyticsView                                               │
└─────────────────────┬───────────────────────────────────────┘
					  │
┌─────────────────────────────────────────────────────────────┐
│                    Business Logic Layer                      │
├─────────────────────────────────────────────────────────────┤
│  • FileOrganizationService                                   │
│  • RuleManagementService                                     │
│  • SchedulerService                                          │
│  • CloudStorageService                                       │
│  • CloudIntegrationHelper                                    │
│  • FolderStructureService                                    │
│  • SettingsService                                           │
└─────────────────────┬───────────────────────────────────────┘
					  │
┌─────────────────────────────────────────────────────────────┐
│                    Data Access Layer (EF Core)              │
├─────────────────────────────────────────────────────────────┤
│  • FileOrganizerContext (DbContext)                          │
│  • Database Migrations                                       │
│  • In-Memory Database (for testing)                          │
└─────────────────────┬───────────────────────────────────────┘
					  │
┌─────────────────────────────────────────────────────────────┐
│                    Data Storage Layer                        │
├─────────────────────────────────────────────────────────────┤
│  • SQLite Database                                           │
│  • Local File System                                         │
│  • Google Drive API (Cloud)                                  │
└─────────────────────────────────────────────────────────────┘
```

### Design Patterns Used

1. **Service Pattern** - Business logic separated from UI
2. **Repository Pattern** - Data access abstraction
3. **Dependency Injection** - Service dependencies managed globally
4. **Observer Pattern** - Event-driven scheduler updates
5. **Singleton Pattern** - DbContext instance management

---

## Features

### Feature 1: Local File Organization

**Description:** Organize files on your computer based on rule patterns

**How it works:**
1. Select a folder using drag-and-drop or browse button
2. System scans folder for files
3. Matches files against active rules
4. Moves/copies files to destination folders
5. Logs organization activity

**Example Use Case:**
```
Rule: PDF files → C:\Documents\PDFs
Rule: Images (*.jpg, *.png) → C:\Pictures
Rule: Videos (*.mp4, *.avi) → C:\Videos

Result: Files automatically sorted into folders
```

### Feature 2: Cloud Storage Integration

**Description:** Connect to Google Drive and organize cloud files

**How it works:**
1. Click "Connect Google Drive"
2. Browser opens for authentication
3. Grant permissions to access your Google Drive
4. Select files from cloud to organize
5. Apply organization rules to cloud files

**Technology:** Google Drive API v3 with OAuth 2.0

### Feature 3: Automated Scheduling

**Description:** Schedule file organization to run automatically

**Supported Schedule Types:**
- **Daily** - Run at specific time each day
- **Weekly** - Run on specific day of week
- **Monthly** - Run on specific date of month
- **Custom** - Run at custom intervals

**Example:**
```
Schedule Name: "Weekly Organization"
Source Folder: C:\Downloads
Schedule Type: Weekly
Run Time: Every Sunday at 2:00 PM
```

### Feature 4: Analytics & Reporting

**Description:** Track file organization history and statistics

**Tracked Data:**
- Total files organized
- Organization date/time
- Successful vs failed operations
- Files by category
- Sync history with Google Drive

---

## Technology Stack

### Core Framework
- **Language:** C# (.NET 10)
- **Framework:** WPF (Windows Presentation Foundation)
- **Target:** Windows 10/11

### Database
- **ORM:** Entity Framework Core 9.0
- **Database:** SQLite
- **Testing:** InMemory Database

### External APIs
- **Google Drive API v3** - Cloud file management
- **Google OAuth 2.0** - Authentication

### Testing
- **Test Framework:** xUnit
- **Mocking:** Moq
- **In-Memory DB:** EF Core InMemory

### Dependencies
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
  <PackageReference Include="Google.Apis.Auth" Version="1.68.0.0" />
  <PackageReference Include="Google.Apis.Drive.v3" Version="1.68.0.0" />
</ItemGroup>
```

---

## Core Components

### 1. FileOrganizationService

**Responsibility:** Main business logic for organizing files

```csharp
public class FileOrganizationService
{
	// Organize files from a source folder
	public OrganizationResult OrganizeFiles(string sourceFolder, bool moveFiles)

	// Check if file matches a rule pattern
	public bool MatchesRule(string fileName, FileOrganizationRule rule)

	// Get all files from folder recursively
	public List<string> GetAllFilesInFolder(string folderPath)
}
```

**Usage Example:**
```csharp
var service = new FileOrganizationService(dbContext);
var result = service.OrganizeFiles("C:\\Downloads", moveFiles: true);
Console.WriteLine($"Organized: {result.SuccessCount} files");
```

### 2. RuleManagementService

**Responsibility:** CRUD operations for organization rules

```csharp
public class RuleManagementService
{
	public FileOrganizationRule CreateRule(string name, string pattern, string destination, bool isActive)
	public List<FileOrganizationRule> GetAllRules()
	public FileOrganizationRule UpdateRule(int id, string name, string pattern, string destination, bool isActive)
	public bool DeleteRule(int id)
	public FileOrganizationRule GetRule(int id)
}
```

**Data Model:**
```csharp
public class FileOrganizationRule
{
	public int Id { get; set; }
	public string RuleName { get; set; }              // e.g., "PDF Files"
	public string FilePattern { get; set; }           // e.g., "*.pdf"
	public string DestinationFolder { get; set; }     // e.g., "C:\Documents"
	public bool IsActive { get; set; }                // Enable/disable rule
	public DateTime CreatedDate { get; set; }         // Creation timestamp
}
```

### 3. SchedulerService

**Responsibility:** Automated task scheduling and execution

```csharp
public class SchedulerService
{
	public void StartScheduler()                      // Begin checking schedules
	public void StopScheduler()                       // Stop scheduler
	public FileOrganizationSchedule CreateSchedule(...) // Create new schedule
	public List<FileOrganizationSchedule> GetAllSchedules()
	public bool DeleteSchedule(int id)
}
```

**Schedule Types:**
- Daily, Weekly, Monthly, Custom

### 4. CloudIntegrationHelper

**Responsibility:** Simplified cloud operations interface

```csharp
public class CloudIntegrationHelper
{
	public async Task<(bool success, string message)> AuthenticateGoogleDriveAsync()
	public async Task<(bool success, string message)> DisconnectGoogleDriveAsync()
	public bool IsAuthenticated()
	public async Task<(bool success, List<CloudFileInfo> files, string message)> GetCloudFilesAsync()
	public CloudAuthStatus GetAuthStatus()
}
```

### 5. GoogleOAuthService

**Responsibility:** Google OAuth 2.0 authentication

```csharp
public class GoogleOAuthService
{
	public async Task<UserCredential> AuthenticateAsync()
	public DriveService GetDriveService(UserCredential credential)
	public async Task RevokeAccessAsync()
}
```

---

## Usage Guide

### Setup Instructions

#### 1. Clone Repository
```bash
git clone https://github.com/anudave/FileOrganizerA.git
cd WpfApp1
```

#### 2. Build Project
```bash
dotnet build
```

#### 3. Setup Google Drive (Optional)
See `CLOUD_INTEGRATION_SETUP.md` for detailed instructions

#### 4. Run Application
```bash
dotnet run
```

### User Workflows

#### Workflow 1: Organize Local Files

```
1. Launch Smart File Organizer
2. Go to "Folder Organization" tab
3. Drag & drop folder OR click "Browse Folder"
4. View file count and size
5. Go to "Rule Management" tab
6. Create organization rules:
   - Rule Name: "PDF Documents"
   - File Pattern: "*.pdf"
   - Destination: "C:\My Documents\PDFs"
   - Click "Create Rule"
7. Return to "Folder Organization" tab
8. Click "▶ Start Organization"
9. View results in report
```

#### Workflow 2: Connect Google Drive

```
1. Go to "Cloud Storage Integration" tab
2. Click "🔐 Connect Google Drive"
3. Browser opens with Google login
4. Grant permissions
5. Files appear in list
6. Select files to organize
7. Click "🔄 Organize Selected"
8. View organization results
```

#### Workflow 3: Schedule Automation

```
1. Go to "Scheduler" tab
2. Click "Create New Schedule"
3. Enter schedule details:
   - Name: "Weekly Cleanup"
   - Source Folder: "C:\Downloads"
   - Schedule Type: "Weekly"
   - Run Time: Monday 2:00 PM
4. Click "Save Schedule"
5. Schedule runs automatically at set time
```

---

## Development Setup

### Prerequisites
- Visual Studio 2022 or later / VS Code
- .NET 10 SDK
- SQLite (included with EF Core)
- Google account (for cloud features)

### Project Structure
```
WpfApp1/
├── Views/                          # WPF UI Views
│   ├── FileOrganizationView.xaml
│   ├── CloudOrganizationView.xaml
│   └── ...
├── Services/                       # Business Logic
│   ├── FileOrganizationService.cs
│   ├── RuleManagementService.cs
│   └── ...
├── Models/                         # Data Models
│   ├── FileOrganizationRule.cs
│   ├── CloudStorageAccount.cs
│   └── ...
├── Data/                           # Database
│   ├── FileOrganizerContext.cs
│   └── Migrations/
└── WpfApp1.Tests/                  # Unit Tests
	└── Services/
```

### Building from Source

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test WpfApp1.Tests

# Run application
dotnet run --project WpfApp1
```

---

## Testing

### Test Coverage

**Unit Tests:**
- `RuleManagementServiceTests.cs` - 9 tests
- `FileOrganizationServiceTests.cs` - 8 tests
- `SchedulerServiceTests.cs` - 10 tests

**Total: 27+ Test Cases**

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "RuleManagementServiceTests"

# Run with verbose output
dotnet test -v normal

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

### Test Examples

#### Example 1: Testing Rule Creation
```csharp
[Fact]
public void CreateRule_WithValidInput_CreatesRule()
{
	// Arrange
	var context = CreateTestContext();
	var service = new RuleManagementService(context);

	// Act
	var result = service.CreateRule("PDFs", "*.pdf", "C:\\PDFs", true);

	// Assert
	Assert.NotNull(result);
	Assert.Equal("PDFs", result.RuleName);
}
```

#### Example 2: Testing File Matching
```csharp
[Fact]
public void MatchesRule_WithMatchingPattern_ReturnsTrue()
{
	// Arrange
	var rule = new FileOrganizationRule
	{
		FilePattern = "*.pdf"
	};

	// Act
	bool result = service.MatchesRule("document.pdf", rule);

	// Assert
	Assert.True(result);
}
```

---

## Troubleshooting

### Issue: Build Fails with "Missing Package"

**Solution:**
```bash
# Restore NuGet packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

### Issue: Database Not Found

**Solution:**
```bash
# Apply migrations
dotnet ef database update

# Or reset database
dotnet ef database drop
dotnet ef database update
```

### Issue: Google Drive Connection Fails

**Troubleshooting:**
1. Verify `credentials.json` is in application directory
2. Check internet connection
3. Ensure OAuth API is enabled in Google Cloud Console
4. Try clicking "Disconnect" then "Connect" again

See `CLOUD_INTEGRATION_SETUP.md` for detailed Google Drive setup

### Issue: Files Not Organizing

**Troubleshooting:**
1. Verify rules are created and active
2. Check file patterns match actual files
3. Verify destination folders exist
4. Check file permissions
5. View organization results for error messages

---

## Code Examples

### Example: Create and Use a Rule

```csharp
var dbContext = DbContextService.GetInstance();
var ruleService = new RuleManagementService(dbContext);

// Create a rule
var rule = ruleService.CreateRule(
	ruleName: "Image Files",
	filePattern: "*.jpg;*.png;*.gif",
	destinationFolder: "C:\\Pictures",
	isActive: true
);

Console.WriteLine($"Created rule: {rule.RuleName}");
```

### Example: Organize Files

```csharp
var fileService = new FileOrganizationService(dbContext);

// Organize all files in a folder
var result = fileService.OrganizeFiles(
	sourceFolder: "C:\\Downloads",
	moveFiles: true  // true to move, false to copy
);

// Check results
Console.WriteLine($"Successfully organized: {result.SuccessCount}");
Console.WriteLine($"Failed: {result.FailureCount}");
Console.WriteLine($"Skipped: {result.SkippedCount}");
```

### Example: Schedule Automation

```csharp
var schedulerService = new SchedulerService(dbContext, fileService);

// Create a schedule
var schedule = schedulerService.CreateSchedule(
	scheduleName: "Daily Downloads Cleanup",
	sourceFolder: "C:\\Downloads",
	scheduleType: "daily",
	scheduleTime: DateTime.Now.AddHours(1),
	isActive: true
);

// Start scheduler
schedulerService.StartScheduler();
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-01-21 | Initial release |

---

## Contributing Guidelines

1. Create feature branch: `git checkout -b feature/your-feature`
2. Make meaningful commits
3. Write tests for new features
4. Submit Pull Request
5. Minimum 10 commits per contributor
6. Work across multiple days

---

## Contact & Support

For issues, questions, or suggestions:
- GitHub Issues: https://github.com/anudave/FileOrganizerA/issues
- Email: anudave@example.com

---

## License

This project is part of the C# Course curriculum.

**Last Updated:** January 21, 2025  
**Documentation Version:** 1.0

