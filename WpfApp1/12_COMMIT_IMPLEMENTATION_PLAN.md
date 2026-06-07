# 📋 12-Commit Implementation Plan

## Overview
**Total Features**: 8 (3 Critical + 3 High + 2 Medium)
**Total Commits**: 12
**Estimated Timeline**: 4-6 weeks
**Target Branch**: `addis`

---

## Commit Timeline

### **Phase 1: Foundation (Commits 1-3)**

#### **Commit 1: Add Dry Run / Preview Mode - Part 1 (Models & Service)**
```
Description: Implement preview mechanism for file organization
Changes:
  - Create OrganizationPreview model
  - Add PreviewFileMove class
  - Implement GetOrganizationPreview() in FileOrganizationService
  - Add database table migration for preview logs

Files to Create:
  ✓ Models/OrganizationPreview.cs
  ✓ Models/PreviewFileMove.cs

Files to Modify:
  ✓ Services/FileOrganizationService.cs (add GetPreview method)
  ✓ Data/FileOrganizerContext.cs (add DbSet)
  ✓ Data/Migrations/xxx_AddPreviewSupport.cs

Estimated Lines Changed: 150-200 LOC
Estimated Time: 2 hours
```

---

#### **Commit 2: Add Dry Run / Preview Mode - Part 2 (UI)**
```
Description: Add preview UI to FileOrganizationView
Changes:
  - Add "Preview" button
  - Show preview results in dialog
  - Display file move list with reasons
  - Add "Confirm & Organize" and "Cancel" buttons

Files to Modify:
  ✓ Views/FileOrganizationView.xaml (add preview button)
  ✓ Views/FileOrganizationView.xaml.cs (add preview logic)

Estimated Lines Changed: 100-150 LOC
Estimated Time: 1.5 hours
```

---

#### **Commit 3: Add Duplicate File Handling - Models & Options**
```
Description: Create duplicate handling strategy and models
Changes:
  - Create DuplicateAction enum (Skip, Overwrite, RenameNew, RenameExisting)
  - Create DuplicateHandlingPolicy class
  - Create DuplicateFile model
  - Add database migration

Files to Create:
  ✓ Models/DuplicateAction.cs (enum)
  ✓ Models/DuplicateHandlingPolicy.cs
  ✓ Models/DuplicateFile.cs

Files to Modify:
  ✓ Data/FileOrganizerContext.cs
  ✓ Data/Migrations/xxx_AddDuplicateHandling.cs

Estimated Lines Changed: 80-120 LOC
Estimated Time: 1.5 hours
```

---

### **Phase 2: Duplicate Handling (Commits 4-5)**

#### **Commit 4: Implement Duplicate Detection Logic**
```
Description: Add duplicate detection to FileOrganizationService
Changes:
  - Implement DetectDuplicates() method
  - Add GetFileHash() for content comparison
  - Implement CheckForDuplicates() before moving
  - Create conflict resolution strategy

Files to Modify:
  ✓ Services/FileOrganizationService.cs (add duplicate detection)

New Methods:
  - DetectDuplicateFiles(sourceFile, destFolder)
  - GetFileSizeHash(FileInfo)
  - ResolveDuplicate(sourceFile, destFile, DuplicateAction)

Estimated Lines Changed: 200-250 LOC
Estimated Time: 2.5 hours
```

---

#### **Commit 5: Add Duplicate Handling UI & Settings**
```
Description: Add duplicate handling options to settings
Changes:
  - Add duplicate policy to AppSettings model
  - Create DuplicateHandlingDialog XAML/Code
  - Add settings UI in SettingsView
  - Connect policy to file organization

Files to Create:
  ✓ Views/DuplicateHandlingDialog.xaml
  ✓ Views/DuplicateHandlingDialog.xaml.cs

Files to Modify:
  ✓ Views/SettingsView.xaml (add duplicate settings)
  ✓ Views/SettingsView.xaml.cs
  ✓ Models/AppSettings.cs (add DuplicatePolicy)

Estimated Lines Changed: 150-200 LOC
Estimated Time: 2 hours
```

---

### **Phase 3: Error Recovery (Commits 6-7)**

#### **Commit 6: Add Error Recovery Infrastructure**
```
Description: Create rollback mechanism for failed operations
Changes:
  - Create OrganizationTransaction model
  - Add MoveHistory class
  - Implement TransactionLog database table
  - Add RollbackStack to FileOrganizationService

Files to Create:
  ✓ Models/OrganizationTransaction.cs
  ✓ Models/MoveHistory.cs
  ✓ Services/RollbackService.cs

Files to Modify:
  ✓ Data/FileOrganizerContext.cs (add DbSet)
  ✓ Data/Migrations/xxx_AddRollbackSupport.cs
  ✓ Services/FileOrganizationService.cs (add transaction tracking)

Estimated Lines Changed: 180-220 LOC
Estimated Time: 2.5 hours
```

---

#### **Commit 7: Implement Rollback Functionality**
```
Description: Add ability to undo file movements
Changes:
  - Implement RollbackLastOperation() method
  - Add rollback validation (check if files still exist)
  - Create safe move-back logic
  - Add transaction confirmation

Files to Modify:
  ✓ Services/RollbackService.cs (complete implementation)
  ✓ Services/FileOrganizationService.cs (integrate rollback)
  ✓ Views/FileOrganizationView.xaml.cs (add "Undo" button logic)

New Methods:
  - RollbackLastOperation()
  - ValidateRollback(OrganizationTransaction)
  - SafeMoveBack(sourceFile, originalLocation)

Estimated Lines Changed: 150-200 LOC
Estimated Time: 2 hours
```

---

### **Phase 4: Safety Features (Commits 8-9)**

#### **Commit 8: Add File Size & Exclusion Patterns - Models**
```
Description: Create models for file size policies and exclusion patterns
Changes:
  - Create FileSizePolicy model
  - Create ExclusionPattern model
  - Create ExclusionType enum (Extension, Name, Size, Path)
  - Add database migrations

Files to Create:
  ✓ Models/FileSizePolicy.cs
  ✓ Models/ExclusionPattern.cs
  ✓ Models/ExclusionType.cs (enum)

Files to Modify:
  ✓ Data/FileOrganizerContext.cs
  ✓ Data/Migrations/xxx_AddExclusionPatterns.cs

Estimated Lines Changed: 100-150 LOC
Estimated Time: 1.5 hours
```

---

#### **Commit 9: Implement File Size Checks & Exclusion Logic**
```
Description: Add validation for file size and exclusion patterns
Changes:
  - Implement ValidateFileSize() method
  - Implement IsFileExcluded() method
  - Add size warning dialog
  - Add exclusion matching engine

Files to Create:
  ✓ Services/ExclusionService.cs
  ✓ Services/FileSizeValidator.cs

Files to Modify:
  ✓ Services/FileOrganizationService.cs (integrate checks)
  ✓ Views/FileOrganizationView.xaml.cs (add dialogs)

New Methods:
  - ValidateFileSize(FileInfo, FileSizePolicy)
  - IsFileExcluded(FileInfo, List<ExclusionPattern>)
  - MatchesExclusionPattern(string, ExclusionPattern)

Estimated Lines Changed: 200-250 LOC
Estimated Time: 2.5 hours
```

---

### **Phase 5: Undo History (Commit 10)**

#### **Commit 10: Add Batch Undo History System**
```
Description: Create complete undo history with database persistence
Changes:
  - Create OrganizationHistory model
  - Create HistoryEntry for detailed logging
  - Implement GetOrganizationHistory() method
  - Add UI to view and undo past operations
  - Implement selective undo (revert any past operation)

Files to Create:
  ✓ Models/OrganizationHistory.cs
  ✓ Models/HistoryEntry.cs
  ✓ Services/HistoryService.cs
  ✓ Views/HistoryDialog.xaml
  ✓ Views/HistoryDialog.xaml.cs

Files to Modify:
  ✓ Data/FileOrganizerContext.cs
  ✓ Data/Migrations/xxx_AddHistorySupport.cs
  ✓ Services/FileOrganizationService.cs (log operations)
  ✓ Views/AnalyticsView.xaml.cs (add history viewer)

New Methods:
  - LogOrganizationHistory(result, rules)
  - UndoSpecificOperation(int historyId)
  - GetRecentOperations(int count)

Estimated Lines Changed: 300-400 LOC
Estimated Time: 3.5 hours
```

---

### **Phase 6: UI Enhancements (Commits 11-12)**

#### **Commit 11: Add Search & Filtering - Rules & Suggestions**
```
Description: Add search and filter capabilities to RuleManagement and Suggestions
Changes:
  - Add search textbox to RuleManagementView
  - Implement rule filtering by name/category
  - Add filter dropdown (All, Documents, Images, etc.)
  - Add filter to suggestions grid
  - Implement case-insensitive search

Files to Modify:
  ✓ Views/RuleManagementView.xaml (add search UI)
  ✓ Views/RuleManagementView.xaml.cs (add filter logic)
  ✓ Services/RuleManagementService.cs (add search methods)

New Methods:
  - SearchRules(string searchTerm)
  - FilterRulesByCategory(string category)
  - FilterSuggestionsByStatus(string status)

Estimated Lines Changed: 150-200 LOC
Estimated Time: 2 hours
```

---

#### **Commit 12: Add Notifications & Polish**
```
Description: Add Windows notifications and final UI polish
Changes:
  - Implement NotificationService
  - Add Windows Toast notifications
  - Add notifications for:
	* Schedule completion
	* Error alerts
	* Operation summaries
  - Final UI tweaks and refinements
  - Update documentation

Files to Create:
  ✓ Services/NotificationService.cs
  ✓ Models/NotificationMessage.cs

Files to Modify:
  ✓ Services/SchedulerService.cs (add notifications)
  ✓ Services/FileOrganizationService.cs (add notifications)
  ✓ Views/RuleManagementView.xaml.cs (add notifications)
  ✓ MainWindow.xaml.cs (initialize notifications)

Estimated Lines Changed: 150-200 LOC
Estimated Time: 2 hours
```

---

## Implementation Summary

```
Phase 1: Foundation (Dry Run + Duplicates Models)
├── Commit 1: Dry Run Models & Service ........... 2h
├── Commit 2: Dry Run UI ........................ 1.5h
└── Commit 3: Duplicate Models .................. 1.5h
   Subtotal: 5 hours, ~430-470 LOC

Phase 2: Duplicate Handling Logic
├── Commit 4: Duplicate Detection ............... 2.5h
└── Commit 5: Duplicate UI & Settings ........... 2h
   Subtotal: 4.5 hours, ~350-450 LOC

Phase 3: Error Recovery
├── Commit 6: Rollback Infrastructure ........... 2.5h
└── Commit 7: Rollback Implementation ........... 2h
   Subtotal: 4.5 hours, ~330-420 LOC

Phase 4: Safety Features
├── Commit 8: Size & Exclusion Models ........... 1.5h
└── Commit 9: Size & Exclusion Logic ............ 2.5h
   Subtotal: 4 hours, ~300-400 LOC

Phase 5: Undo History
└── Commit 10: History System Complete .......... 3.5h
   Subtotal: 3.5 hours, ~300-400 LOC

Phase 6: Enhancements & Polish
├── Commit 11: Search & Filtering .............. 2h
└── Commit 12: Notifications & Polish .......... 2h
   Subtotal: 4 hours, ~300-400 LOC

───────────────────────────────────────────────
TOTAL: 25.5 hours, ~2,010-2,540 LOC
```

---

## Commit Details Table

| # | Commit Title | Duration | LOC | Phase | Dependencies |
|---|---|---|---|---|---|
| 1 | Dry Run: Models & Service | 2h | 150-200 | 1 | None |
| 2 | Dry Run: UI | 1.5h | 100-150 | 1 | Commit 1 |
| 3 | Duplicate Handling: Models | 1.5h | 80-120 | 1 | None |
| 4 | Duplicate Detection Logic | 2.5h | 200-250 | 2 | Commit 3 |
| 5 | Duplicate UI & Settings | 2h | 150-200 | 2 | Commit 4 |
| 6 | Error Recovery: Infrastructure | 2.5h | 180-220 | 3 | None |
| 7 | Rollback Implementation | 2h | 150-200 | 3 | Commit 6 |
| 8 | File Size & Exclusion: Models | 1.5h | 100-150 | 4 | None |
| 9 | File Size & Exclusion: Logic | 2.5h | 200-250 | 4 | Commit 8 |
| 10 | Batch Undo History System | 3.5h | 300-400 | 5 | Commits 6-7 |
| 11 | Search & Filtering | 2h | 150-200 | 6 | None |
| 12 | Notifications & Polish | 2h | 150-200 | 6 | Commits 1-10 |

---

## Weekly Timeline Estimate

```
Week 1: Phase 1 & 2 (Commits 1-5)
  Monday-Tuesday: Commits 1-2 (Dry Run)
  Wednesday: Commit 3 (Duplicate Models)
  Thursday-Friday: Commits 4-5 (Duplicate Logic)
  → 4.5 hours × 3 days = 13.5 hours cumulative

Week 2: Phase 3 (Commits 6-7)
  Monday-Tuesday: Commit 6 (Rollback Infrastructure)
  Wednesday-Thursday: Commit 7 (Rollback Implementation)
  Friday: Testing & bug fixes
  → 4.5 hours

Week 3: Phase 4 (Commits 8-9)
  Monday: Commit 8 (Models)
  Tuesday-Wednesday: Commit 9 (Logic)
  Thursday-Friday: Testing & refinement
  → 4 hours

Week 4: Phase 5 (Commit 10)
  Monday-Tuesday: Commit 10 (History System)
  Wednesday-Friday: Testing & integration
  → 3.5 hours

Week 5: Phase 6 (Commits 11-12)
  Monday-Tuesday: Commit 11 (Search & Filtering)
  Wednesday-Thursday: Commit 12 (Notifications)
  Friday: Final testing & deployment
  → 4 hours

Final Week: Integration & QA
  - Full system testing
  - Bug fixes
  - Documentation updates
  - Performance optimization
```

---

## Dependencies & Order Requirements

```
COMMIT 1 ─┐
		  ├─→ COMMIT 2 (Dry Run Complete)

COMMIT 3 ─┐
		  ├─→ COMMIT 4 ─┐
						├─→ COMMIT 5 (Duplicate Complete)

COMMIT 6 ─┐
		  ├─→ COMMIT 7 ─┐
						├─→ COMMIT 10 (History Complete)

COMMIT 8 ─┐
		  ├─→ COMMIT 9 (Exclusion Complete)

COMMITS 1-10 ─┐
			  ├─→ COMMIT 11 ─┐
			  ├─→ COMMIT 12 ─┤ (All Features Complete)
			  ─────────────────┘
```

---

## Success Criteria for Each Commit

### Commit 1: Build passes ✓, Models created ✓, Service method works ✓
### Commit 2: UI renders ✓, Preview shows correct data ✓, UX is intuitive ✓
### Commit 3: Models created ✓, Migrations work ✓, Enums defined ✓
### Commit 4: Detection logic works ✓, Edge cases handled ✓, Performance acceptable ✓
### Commit 5: Settings UI works ✓, Policy persists ✓, Integration complete ✓
### Commit 6: Transaction tracking works ✓, Database updates ✓, No data loss ✓
### Commit 7: Rollback executes ✓, Files restored ✓, Validation works ✓
### Commit 8: Models created ✓, DB migrations work ✓, Enums defined ✓
### Commit 9: Size checks work ✓, Exclusions apply ✓, Warnings appear ✓
### Commit 10: History records ✓, Undo works ✓, UI displays correctly ✓
### Commit 11: Search works ✓, Filters apply ✓, Performance good ✓
### Commit 12: Notifications appear ✓, All features integrated ✓, Polish complete ✓

---

## Testing Plan for Each Commit

```
Commit 1-2: Test dry run with various folder structures
Commit 3-5: Test duplicate detection with existing files
Commit 6-7: Test rollback with failed operations
Commit 8-9: Test exclusions and size limits
Commit 10: Test undo on multiple operations
Commit 11: Test search/filter performance
Commit 12: Test notifications on all events
```

---

## Rollback Strategy

If any commit fails testing:
1. Revert single commit
2. Fix issues
3. Create amended commit with same number
4. Re-test thoroughly
5. Merge only when passing

---

## Branch Strategy

All commits to: `addis` branch
- Each commit is atomic and can be reverted individually
- Frequent pushes to keep remote in sync
- Tag version at Commit 12: `v1.1-beta`

---

## Next Steps

1. ✅ **Review this plan** - Approve or suggest changes
2. 🚀 **Start Commit 1** - Begin dry run implementation
3. 📝 **Track progress** - Update status as commits complete
4. 🧪 **Test thoroughly** - Validate after each commit
5. 📤 **Push to remote** - Keep `addis` branch updated

**Are you ready to start implementing?** 🎯

