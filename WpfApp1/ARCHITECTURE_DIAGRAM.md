# 📊 Category-Based Dropdown - Architecture Diagram

## Data Flow

```
┌─────────────────────────────────────┐
│  User Interface (RuleManagementView)│
│                                     │
│  Category Dropdown:                 │
│  ┌───────────────────────────────┐  │
│  │ 📄 Documents         ▼        │  │
│  │ 🖼️ Images                      │  │
│  │ 🎬 Videos                      │  │
│  │ 🎵 Audio                       │  │
│  │ ... (13 categories)            │  │
│  └───────────────────────────────┘  │
│                                     │
│  + Browse Destination Folder        │
│  + Add Rule Button                  │
└──────────────┬──────────────────────┘
			   │
			   │ User selects category
			   │ and clicks "Add Rule"
			   ▼
┌─────────────────────────────────────────────────────┐
│  RuleManagementView.xaml.cs                        │
│  (Code-Behind)                                     │
│                                                   │
│  AddRule_Click() handler                          │
│  ├─ Gets selected category                        │
│  ├─ Calls ExtractFilePattern()                    │
│  │  └─ Converts "📄 Documents" → Category name    │
│  │  └─ Looks up in categoryPatterns dictionary    │
│  │  └─ Returns: "*.pdf|*.doc|*.docx|..."          │
│  │                                                │
│  └─ Calls _ruleService.CreateRule()               │
└──────────────┬──────────────────────────────────────┘
			   │
			   │ File patterns extracted
			   │ (multiple extensions)
			   ▼
┌─────────────────────────────────────────────────────┐
│  RuleManagementService                             │
│                                                   │
│  CreateRule(name, pattern, destination)           │
│  ├─ Creates FileOrganizationRule                  │
│  ├─ Stores: Name, Pattern, DestinationFolder     │
│  └─ Saves to database                            │
└──────────────┬──────────────────────────────────────┘
			   │
			   │ Rule persisted
			   ▼
┌─────────────────────────────────────────────────────┐
│  FileOrganizationContext (EF Core - SQLite)        │
│                                                   │
│  fileorganizer.db                                 │
│  └─ FileOrganizationRules table                   │
│     ├─ Id: 1                                      │
│     ├─ Name: "Documents"                          │
│     ├─ FilePattern: "*.pdf|*.doc|*.docx|..."     │
│     ├─ DestinationFolder: "C:\\Documents"        │
│     └─ Active: true                              │
└──────────────┬──────────────────────────────────────┘
			   │
			   │ When files need organizing
			   ▼
┌─────────────────────────────────────────────────────┐
│  FileOrganizationService                           │
│                                                   │
│  OrganizeFiles() method                           │
│  ├─ Reads rules from database                     │
│  ├─ For each file                                 │
│  │  ├─ Check against pattern                      │
│  │  │  • "*.pdf|*.doc|*.docx|..."                │
│  │  ├─ If matches ANY pattern in rule             │
│  │  │  └─ Move file to destination                │
│  │  └─ Continue                                   │
│  └─ Done                                          │
└─────────────────────────────────────────────────────┘
```

---

## Category Mapping Engine

```
User Selection → Processing → Pattern Mapping → Database Storage

┌─────────────────┐
│ 🖼️ Images        │
└────────┬────────┘
		 │
		 ▼
┌──────────────────────────────────────┐
│ Remove emoji and special chars       │
│ "🖼️ Images" → "Images"              │
└────────┬─────────────────────────────┘
		 │
		 ▼
┌──────────────────────────────────────┐
│ Dictionary Lookup:                   │
│ categoryPatterns["Images"] = ?        │
└────────┬─────────────────────────────┘
		 │
		 ▼
┌──────────────────────────────────────────────────────┐
│ Mapping Result:                                      │
│ "*.jpg|*.jpeg|*.png|*.gif|*.bmp|                    │
│  *.tiff|*.webp|*.ico"                              │
└────────┬─────────────────────────────────────────────┘
		 │
		 ▼
┌──────────────────────────────────────┐
│ Single Rule with Multiple Extensions │
│ ✅ One rule = Multiple file types!   │
└──────────────────────────────────────┘
```

---

## File Pattern Matching Logic

```
Rule Created:
{
  Name: "Pictures",
  FilePattern: "*.jpg|*.jpeg|*.png|*.gif|*.bmp|*.tiff|*.webp|*.ico",
  Destination: "C:\\Pictures"
}

File to Organize: "vacation-photo.jpg"

┌─────────────────────────────────────────────┐
│ Check File Against Pattern                  │
│                                             │
│ Pattern: *.jpg|*.jpeg|*.png|...            │
│ File:    vacation-photo.jpg                │
│                                             │
│ Split patterns: [*.jpg, *.jpeg, *.png ...]  │
│                                             │
│ Loop through each pattern:                  │
│ ├─ Match *.jpg?         YES ✅              │
│ │  ✓ File matches rule!                     │
│ │  ✓ Move to C:\Pictures                    │
│ │  ✓ Done                                   │
│ ├─ No need to check other patterns          │
│ └─ Continue to next file                    │
└─────────────────────────────────────────────┘
```

---

## Category to Extension Mapping

```
┌─────────────────────────────────────────────────────────────┐
│ categoryPatterns Dictionary                                 │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Key: "Documents"                                           │
│  Value: "*.pdf|*.doc|*.docx|*.txt|*.odt|*.rtf"             │
│                                                             │
│  Key: "Images"                                              │
│  Value: "*.jpg|*.jpeg|*.png|*.gif|*.bmp|*.tiff|            │
│          *.webp|*.ico"                                      │
│                                                             │
│  Key: "Videos"                                              │
│  Value: "*.mp4|*.avi|*.mkv|*.mov|*.wmv|*.flv|              │
│          *.webm|*.m4v"                                      │
│                                                             │
│  Key: "Audio"                                               │
│  Value: "*.mp3|*.wav|*.flac|*.aac|*.wma|*.ogg|*.m4a"       │
│                                                             │
│  Key: "Archives"                                            │
│  Value: "*.zip|*.rar|*.7z|*.tar|*.gz|*.iso|*.bz2"          │
│                                                             │
│  Key: "Code"                                                │
│  Value: "*.cs|*.java|*.py|*.js|*.cpp|*.c|*.h|              │
│          *.html|*.css|*.php"                                │
│                                                             │
│  Key: "Executables"                                         │
│  Value: "*.exe|*.msi|*.bat|*.cmd|*.sh|*.app"               │
│                                                             │
│  Key: "Spreadsheets"                                        │
│  Value: "*.xls|*.xlsx|*.csv|*.ods"                          │
│                                                             │
│  Key: "Presentations"                                       │
│  Value: "*.ppt|*.pptx|*.odp"                                │
│                                                             │
│  Key: "Web Files"                                           │
│  Value: "*.html|*.htm|*.css|*.js|*.xml|*.json"             │
│                                                             │
│  Key: "Text Files"                                          │
│  Value: "*.txt|*.log|*.md|*.rst"                            │
│                                                             │
│  Key: "Compressed"                                          │
│  Value: "*.zip|*.rar|*.7z|*.gz|*.tar"                       │
│                                                             │
│  Key: "Other"                                               │
│  Value: "*.*"                                               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Database Schema

```
┌──────────────────────────────────────────────────────────┐
│  FileOrganizationRules Table                             │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  Column: Id                    Type: int (PK)           │
│  Column: Name                  Type: string             │
│  Column: FilePattern           Type: string             │
│  Column: DestinationFolder     Type: string             │
│  Column: Active                Type: bool               │
│  Column: CreatedDate           Type: DateTime           │
│                                                          │
├──────────────────────────────────────────────────────────┤
│  Example Row:                                            │
│                                                          │
│  Id: 1                                                   │
│  Name: "My Pictures"                                     │
│  FilePattern: "*.jpg|*.jpeg|*.png|*.gif|*.bmp|          │
│               *.tiff|*.webp|*.ico"                       │
│  DestinationFolder: "C:\\Users\\anwar\\Pictures"        │
│  Active: true                                            │
│  CreatedDate: 2026-05-26 14:30:00                       │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## UI Component Flow

```
┌─────────────────────────────────────┐
│  RuleManagementView (XAML)          │
├─────────────────────────────────────┤
│                                     │
│  ┌─ TextBlock ─────────────────┐   │
│  │ "Rule Name:"                │   │
│  └─────────────────────────────┘   │
│  ┌─ TextBox ──────────────────┐    │
│  │ [____________]             │    │
│  └────────────────────────────┘    │
│                                     │
│  ┌─ TextBlock ─────────────────┐   │
│  │ "Category:"                 │   │
│  └─────────────────────────────┘   │
│  ┌─ ComboBox ─────────────────┐    │
│  │ [📄 Documents ▼]           │    │
│  │ ├─ 📄 Documents            │    │
│  │ ├─ 🖼️ Images               │    │
│  │ ├─ 🎬 Videos               │    │
│  │ ├─ 🎵 Audio                │    │
│  │ └─ ...                      │    │
│  └────────────────────────────┘    │
│                                     │
│  ┌─ TextBlock ─────────────────┐   │
│  │ "Destination Folder:"       │   │
│  └─────────────────────────────┘   │
│  ┌─ TextBox ──────────────────┐    │
│  │ [________] [📁 Browse]     │    │
│  └────────────────────────────┘    │
│                                     │
│  ┌─ Button ──────────────────┐     │
│  │ [+ Add Rule]              │     │
│  └───────────────────────────┘     │
│                                     │
└─────────────────────────────────────┘
		 │ (User interaction)
		 │
		 ▼
  (RuleManagementView.xaml.cs
   Code-Behind Logic)
```

---

## Comparison: Before vs After

### Before (File-by-File Patterns)
```
Dropdown contains:
├─ 📄 PDF Files (*.pdf)
├─ 📝 Word Documents (*.doc*)
├─ 📊 Excel Spreadsheets (*.xls*)
├─ 📈 PowerPoint Presentations (*.ppt*)
├─ 🖼️ JPEG Images (*.jpg)
├─ 🖼️ PNG Images (*.png)
├─ 🖼️ GIF Images (*.gif)
├─ 🎬 MP4 Videos (*.mp4)
├─ 🎬 AVI Videos (*.avi)
├─ 📦 ZIP Archives (*.zip)
├─ 📦 RAR Archives (*.rar)
├─ 📄 Text Files (*.txt)
├─ ⚙️ Executable Files (*.exe)
├─ ⚙️ Installer Files (*.msi)
├─ 🎵 MP3 Audio (*.mp3)
├─ 🎵 WAV Audio (*.wav)
├─ 🎵 FLAC Audio (*.flac)
└─ All Files (*.*)

Result: 18 individual items
Pattern extracted: Single pattern (e.g., "*.pdf")
User must create multiple rules per category
```

### After (Category-Based)
```
Dropdown contains:
├─ 📄 Documents
├─ 🖼️ Images
├─ 🎬 Videos
├─ 🎵 Audio
├─ 📦 Archives
├─ 💻 Code
├─ ⚙️ Executables
├─ 📋 Spreadsheets
├─ 📊 Presentations
├─ 🌐 Web Files
├─ 📜 Text Files
├─ 🔒 Compressed
└─ ⭐ Other

Result: 13 intuitive categories
Pattern mapped: Multiple patterns (e.g., "*.pdf|*.doc|*.docx|...")
User creates one rule per category ✅
```

---

## Summary

```
┌──────────────────────────────────────────────────────┐
│  CATEGORY-BASED DROPDOWN IMPLEMENTATION              │
├──────────────────────────────────────────────────────┤
│                                                      │
│  ✅ Clean UI with 13 intuitive categories            │
│  ✅ One rule per category handles ALL file types     │
│  ✅ Smart mapping from category → file patterns      │
│  ✅ Database stores multiple patterns per rule       │
│  ✅ File matching works with OR logic                │
│  ✅ User experience greatly improved!                │
│                                                      │
│  Status: COMPLETE & READY TO USE! 🚀               │
│                                                      │
└──────────────────────────────────────────────────────┘
```
