# Smart File Organizer

A WPF desktop application for intelligent file automation and organization using drag-and-drop.

## Features
- 🗂️ Drag & Drop folder selection
- 💾 SQLite database for rule storage
- 📊 File organization tracking
- 🤖 AI-powered file categorization (upcoming)
- ⏰ Scheduled automation (upcoming)

## Technology Stack
- **Language:** C# (.NET 10)
- **Framework:** WPF (Windows Presentation Foundation)
- **Database:** SQLite
- **ORM:** Entity Framework Core 9.0
- **Architecture:** MVVM (planned)

## Project Structure
```
WpfApp1/
├── Models/                          # Data models
│   ├── FileOrganizationRule.cs     # File organization rules
│   └── FileOrganizationLog.cs      # Operation logs
├── Data/                            # Database layer
│   └── FileOrganizerContext.cs     # DbContext
├── MainWindow.xaml                  # Main UI
├── MainWindow.xaml.cs              # Code-behind
└── WpfApp1.csproj                  # Project file
```

## Getting Started

### Prerequisites
- .NET 10 SDK or later
- Visual Studio 2022 or later

### Installation
1. Clone the repository
2. Open `WpfApp1.sln` in Visual Studio
3. Restore NuGet packages (automatic on build)
4. Build the solution
5. Run the application

### Database Setup
The application automatically creates a SQLite database at:
```
C:\Users\<YourUsername>\AppData\Roaming\FileOrganizer\fileorganizer.db
```

## Development Roadmap
- [ ] Drag & Drop implementation
- [ ] Rule Management UI
- [ ] AI file categorization
- [ ] Scheduler service
- [ ] Analytics dashboard
- [ ] Settings panel

## Contributing
Each team member should:
1. Create a personal branch: `git checkout -b feature/your-feature-name`
2. Make meaningful commits with clear messages
3. Push to your branch
4. Create a Pull Request with description

## Team Contribution Guidelines
- Minimum 10 meaningful commits per person
- Work across multiple days
- Clear commit messages (not "update" or "fix")
- Each person contributes to at least one major feature
