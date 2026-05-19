# 🚀 Quick Start Guide - Testing Your Application

## **Before You Start**

1. ✅ Visual Studio 2026 (Insiders) installed
2. ✅ .NET 10 SDK installed
3. ✅ Project built successfully
4. ✅ No compilation errors

---

## **STEP 1: BUILD & RUN**

```powershell
cd C:\Users\anwar\Downloads\FileOrganizerA\WpfApp1
# Open WpfApp1.sln in Visual Studio
# Press F5 to run (or Ctrl+Shift+B to build first)
```

**Expected Output:**
- Window opens titled "Smart File Organizer"
- Green status: "Database: Connected"
- Orange status: "AI Model: Active"
- File Organization tab is active

---

## **STEP 2: CREATE YOUR FIRST RULE**

### **Navigate to Rule Management**
1. Click "Rule Management" button (orange button, left sidebar)
2. You should see a form to create rules

### **Create Rule #1: PDF Organization**
```
Rule Name: PDF Organization
File Pattern: *.pdf
Destination Folder: C:\Users\[YourUsername]\Documents\PDFs

Note: The PDFs folder should exist or will be created
```

1. Fill in the fields
2. Click "+ Add Rule"
3. You should see success message
4. Rule appears in DataGrid below

### **Create Rule #2: Image Organization**
```
Rule Name: Image Files
File Pattern: *.jpg
Destination Folder: C:\Users\[YourUsername]\Pictures

OR create two separate rules:
  - Pattern: *.jpg → Pictures
  - Pattern: *.png → Pictures
```

1. Repeat the process
2. Now you should have 2+ rules visible

---

## **STEP 3: TEST FILE ORGANIZATION**

### **Prepare Test Folder**
Create a test folder with mixed files:
```
C:\Test_Files\
├── document1.pdf
├── document2.pdf
├── photo1.jpg
├── photo2.png
├── video.mp4
├── music.mp3
└── readme.txt
```

**Note**: Some files can be duplicates to test conflict resolution!

### **Navigate to File Organization**
1. Click "File Organization" button
2. You see the drop zone

### **Select Your Test Folder**
- **Option A (Drag & Drop):**
  1. Open File Explorer
  2. Navigate to C:\Test_Files\
  3. Drag the folder into the drop zone

- **Option B (Browse):**
  1. Click "Browse Folder" button
  2. Navigate to C:\Test_Files\
  3. Select the folder

### **Expected Result:**
- Folder path shows: C:\Test_Files\
- Statistics show: 7 files, size (e.g., 15.5 MB)

### **Start Organization**
1. Click "▶ Start Organization" button (green button)
2. Watch the status bar: "Organizing files..."
3. In the green text area, you'll see real-time logs:

```
Starting file organization for: C:\Test_Files
Found 7 files to process
Loaded 2 active rules

✓ ORGANIZED: document1.pdf → C:\Users\...\Documents\PDFs\
✓ ORGANIZED: document2.pdf → C:\Users\...\Documents\PDFs\
✓ ORGANIZED: photo1.jpg → C:\Users\...\Pictures\
✓ ORGANIZED: photo2.png → C:\Users\...\Pictures\
⊘ SKIPPED: video.mp4 (no matching rule)
⊘ SKIPPED: music.mp3 (no matching rule)
⊘ SKIPPED: readme.txt (no matching rule)

═══════════════════════════════════
✓ Successfully Organized: 4 files
⊘ Skipped: 3 files
✗ Failed: 0 files
═══════════════════════════════════
```

4. Click OK on the summary dialog

### **Verify Files Were Moved**
1. Check C:\Users\[You]\Documents\PDFs\ → Should have 2 PDFs
2. Check C:\Users\[You]\Pictures\ → Should have 2 images
3. Check original C:\Test_Files\ → Should have 3 remaining files

---

## **STEP 4: TEST ANALYTICS**

### **Navigate to Analytics**
1. Click "Analytics" button
2. You should see dashboard

### **Expected Display:**
```
Statistics Cards:
  Total Organized: 4 files
  Total Size Moved: [size] MB
  Success Rate: 100%
  Failed Operations: 0

Time-based:
  Today: 4 files
  This Week: 4 files
  This Month: 4 files

History Table:
  Shows your 4 successful operations with:
  - Date/time
  - Source files
  - Destination
  - Status (green checkmark)
```

### **Test Refresh Button**
1. Click "🔄 Refresh"
2. Statistics should update

---

## **STEP 5: TEST DUPLICATE HANDLING**

### **Create Duplicate Test**
```
C:\Duplicate_Test\
├── document.pdf
└── document.pdf (second copy)
```

1. Create a rule: *.pdf → C:\Users\[You]\Documents\PDFs2\
2. Run organization

### **Expected Result:**
1. First file: document.pdf
2. Second file: document_1.pdf

This shows conflict resolution works!

---

## **STEP 6: TEST ERROR SCENARIOS**

### **Test 1: Invalid Destination Folder**
1. Go to Rule Management
2. Try creating rule with destination: Z:\NonExistent\Path\
3. Should show error: "Destination folder does not exist"

### **Test 2: Invalid Pattern**
1. Try pattern without `*.` prefix
2. Should show error: "File pattern must start with *."

### **Test 3: Missing Fields**
1. Try creating rule without Rule Name
2. Should show error: "Rule name is required"

---

## **STEP 7: EDIT AND DELETE RULES**

### **Edit a Rule**
1. In Rule Management, click "Edit" button on a rule
2. Modal window appears
3. Change values (e.g., destination folder)
4. Click "Save"
5. Rule updates in grid

### **Delete a Rule**
1. Click "Delete" button on a rule
2. Confirm deletion
3. Rule disappears from grid

---

## **STEP 8: TEST SCHEDULER (Optional - Code Only)**

### **View Scheduler Code**
The SchedulerService is implemented but doesn't have UI yet.

**What it does (in code):**
- Runs every 60 seconds
- Checks all active schedules
- Executes if time matches
- Logs results to database

**To test (advanced):**
You can modify FileOrganizationView.xaml.cs to initialize scheduler:

```csharp
// Add to FileOrganizationView constructor
private SchedulerService _schedulerService;

public FileOrganizationView()
{
	InitializeComponent();
	_schedulerService = new SchedulerService(_dbContext, _organizationService);
	_schedulerService.StartScheduler(); // Starts background service
}
```

---

## **TROUBLESHOOTING**

### **Database Connection Error**
```
Error: "Error initializing database"
Solution: 
  1. Check file permissions in AppData\Roaming
  2. Close all instances of the app
  3. Delete fileorganizer.db and restart
```

### **Rule Not Working**
```
Files not organized when clicking button
Solution:
  1. Check file pattern (must be *.ext format)
  2. Ensure destination folder exists
  3. Check rule is Active (checkbox should be checked)
  4. Check rule was saved (should appear in grid)
```

### **Files Already in Destination**
```
Files not moving because destination has them
Solution:
  1. Rename existing files first
  2. Files get _1, _2 suffix if conflict
  3. Check history in Analytics for details
```

### **Build Fails**
```
Solution:
  1. Clean solution: Ctrl+Shift+B
  2. Close and reopen Visual Studio
  3. Delete bin/obj folders
  4. Restore NuGet packages
```

---

## **WHAT EACH TAB DOES**

| Tab | Purpose | What to Test |
|-----|---------|-------------|
| **File Organization** | Select folder & run organization | Drag-drop, browse, organization, results |
| **Rule Management** | Create/edit/delete rules | Add rule, edit rule, delete rule, validation |
| **Analytics** | View statistics & history | Statistics cards, history table, refresh |

---

## **SUCCESS CRITERIA**

Your testing is successful when:

- ✅ Database initializes without errors
- ✅ Can create rules with validation
- ✅ Can select folders via drag-drop or browse
- ✅ Files organize correctly based on rules
- ✅ Statistics display accurate numbers
- ✅ History shows all operations
- ✅ Duplicate files handled with _1 suffix
- ✅ Unmatched files shown as "Skipped"
- ✅ Can edit and delete rules
- ✅ Analytics updates after organization

---

## **DEMO FLOW (For Presentation)**

If you're demonstrating this to instructors:

```
1. SHOW STRUCTURE (30 sec)
   - Show MainWindow with navigation buttons
   - Mention database and services

2. CREATE RULES (1 min)
   - Go to Rule Management
   - Create 2-3 sample rules
   - Show they appear in grid

3. ORGANIZE FILES (2 min)
   - Go to File Organization
   - Drag in test folder
   - Show file count and size
   - Click "Start Organization"
   - Show real-time logs
   - Show completion summary

4. VERIFY (1 min)
   - Open File Explorer
   - Show files moved to correct destinations
   - Show no remaining files (except skipped)

5. ANALYTICS (1 min)
   - Go to Analytics
   - Show statistics cards
   - Show organization history table
   - Demonstrate refresh

6. EXPLAIN TECHNOLOGY (1 min)
   - Database (SQLite, EF Core)
   - Services (MVVM pattern)
   - Scheduler (background automation)

Total Demo Time: ~6-7 minutes (impressive!)
```

---

## **YOU'RE ALL SET! 🎉**

Your application is ready to test and demonstrate.

**Next Steps:**
1. Test the app following this guide
2. Fix any issues you find
3. Create a Pull Request on GitHub
4. Submit for grading

Good luck! 🚀

