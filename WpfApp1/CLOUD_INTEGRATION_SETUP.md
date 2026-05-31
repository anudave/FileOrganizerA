# 🌐 Cloud Integration Setup Guide

## Google Drive Integration - Complete Setup Instructions

This guide walks you through setting up Google Drive integration for the Smart File Organizer application.

---

## 📋 Prerequisites

- Google Account
- Access to Google Cloud Console
- Visual Studio / .NET 10 development environment
- Internet connection

---

## 🔧 Step-by-Step Setup

### Step 1: Create a Google Cloud Project

1. **Visit Google Cloud Console**
   - Go to: https://console.cloud.google.com
   - Sign in with your Google account (create one if needed)

2. **Create a New Project**
   - Click the project dropdown at the top
   - Click "NEW PROJECT"
   - Enter project name: "File Organizer"
   - Click "CREATE"
   - Wait for the project to be created

3. **Select Your Project**
   - Once created, select it from the dropdown

**Screenshot Reference:**
```
Google Cloud Console → Project Selector → New Project
Project Name: "File Organizer"
Organization: (leave as default)
```

---

### Step 2: Enable Google Drive API

1. **Open APIs & Services**
   - In the left sidebar, go to: APIs & Services → Library

2. **Search for Google Drive API**
   - In the search box, type: "Google Drive API"
   - Click on "Google Drive API"

3. **Enable the API**
   - Click the "ENABLE" button
   - Wait for it to be enabled (usually instant)

**Verification:**
```
Status should show: "API Enabled"
```

---

### Step 3: Create OAuth 2.0 Credentials

1. **Go to Credentials**
   - In left sidebar: APIs & Services → Credentials

2. **Create OAuth Consent Screen**
   - Click "CREATE CREDENTIALS" → "OAuth 2.0 Client ID"
   - It will ask to "Configure OAuth Consent Screen first"
   - Click "CONFIGURE CONSENT SCREEN"

3. **Set Up Consent Screen**
   - **User Type:** Select "External"
   - **Click "CREATE"**

   - **Fill in the form:**
	 - App name: "File Organizer"
	 - User support email: (use your email)
	 - Developer contact: (use your email)
   - **Click "SAVE AND CONTINUE"**

4. **Scopes (Leave defaults)**
   - Click "SAVE AND CONTINUE"

5. **Test Users (Optional)**
   - Click "SAVE AND CONTINUE"

6. **Back to Credentials**
   - Click "CREATE CREDENTIALS" again
   - Select "OAuth 2.0 Client ID"

---

### Step 4: Download Credentials

1. **Create Desktop Application Credentials**
   - **Application Type:** "Desktop app"
   - Click "CREATE"

2. **Download the JSON File**
   - A dialog will appear
   - Click "DOWNLOAD" or "Download JSON"
   - **Important:** Save this file as **`credentials.json`**

3. **Save credentials.json**
   - Location: `C:\Users\<YourUsername>\Downloads\FileOrganizerA\WpfApp1\WpfApp1\bin\Debug\net10.0-windows\`
   - Or simply in the application's working directory

**File Contents (Example):**
```json
{
  "installed": {
	"client_id": "YOUR_CLIENT_ID.apps.googleusercontent.com",
	"client_secret": "YOUR_CLIENT_SECRET",
	"auth_uri": "https://accounts.google.com/o/oauth2/auth",
	"token_uri": "https://oauth2.googleapis.com/token",
	"redirect_uris": ["urn:ietf:wg:oauth:2.0:oob"]
  }
}
```

⚠️ **SECURITY WARNING:**
- Keep this file SECRET
- Never commit it to GitHub
- It's already in `.gitignore` (check your git config)

---

### Step 5: Place credentials.json in Application

1. **Locate Application Directory**
   - After building the application, find the output folder:
   - `WpfApp1\bin\Debug\net10.0-windows\`

2. **Copy credentials.json**
   - Copy your downloaded `credentials.json` to this directory
   - The application will look for it at runtime

3. **Or Add to Project**
   - Add `credentials.json` to your Visual Studio project
   - Right-click → Properties → Copy to Output Directory: "Always"

---

## 🧪 Testing the Integration

### Test 1: Verify credentials.json Location

```csharp
// In CloudOrganizationView.xaml.cs
string credentialsPath = Path.Combine(AppContext.BaseDirectory, "credentials.json");
if (File.Exists(credentialsPath))
{
	Debug.WriteLine("✓ credentials.json found at: " + credentialsPath);
}
else
{
	Debug.WriteLine("✗ credentials.json NOT found at: " + credentialsPath);
}
```

### Test 2: Login Test

1. **Run the Application**
   - Build and run WpfApp1
   - Navigate to "Cloud Storage Integration" tab

2. **Click "Connect Google Drive"**
   - Your default browser will open
   - Google will ask for permission
   - Click "Allow"

3. **Verify Connection**
   - Look for: "✓ Connected" status
   - Email should display your Google account
   - "Last Sync" time should update

### Test 3: List Files

1. **After successful login**
   - Click "🔄 Refresh Files"
   - Wait for the file list to load
   - You should see your Google Drive files in the DataGrid

---

## 📊 Architecture Overview

```
┌─────────────────────────────────────────┐
│   Smart File Organizer Application      │
│   (WpfApp1.Views.CloudOrganizationView) │
└──────────────┬──────────────────────────┘
			   │
			   ▼
	┌──────────────────────┐
	│ GoogleOAuthService   │
	│ - Authenticate()     │
	│ - GetDriveService()  │
	│ - RevokeAccess()     │
	└──────────┬───────────┘
			   │
			   ▼
	┌──────────────────────┐
	│ Google OAuth 2.0     │
	│ (Browser Login)      │
	└──────────┬───────────┘
			   │
			   ▼
	┌──────────────────────┐
	│ CloudStorageService  │
	│ - ListFiles()        │
	│ - DownloadFile()     │
	│ - UploadFile()       │
	└──────────┬───────────┘
			   │
			   ▼
	┌──────────────────────┐
	│ Google Drive API     │
	│ (REST API)           │
	└──────────────────────┘
```

---

## 🔑 Key Components

### GoogleOAuthService
**Responsibility:** Handle OAuth authentication flow

```csharp
public class GoogleOAuthService
{
	public async Task<UserCredential> AuthenticateAsync()
	{
		// 1. Check if credentials.json exists
		// 2. Open browser for user login
		// 3. Store tokens for future use
		// 4. Return authenticated credential
	}

	public DriveService GetDriveService(UserCredential credential)
	{
		// Initialize and return a DriveService instance
	}

	public async Task RevokeAccessAsync()
	{
		// Revoke user's access
	}
}
```

### CloudStorageService
**Responsibility:** Interact with Google Drive API

```csharp
public class CloudStorageService
{
	public async Task<List<Google.Apis.Drive.v3.Data.File>> ListFilesAsync()
	{
		// List files from user's Google Drive
	}

	public CloudStorageAccount GetCurrentAccount()
	{
		// Get the currently connected account from database
	}

	public void SaveAccount(CloudStorageAccount account)
	{
		// Save account details to database
	}
}
```

### CloudStorageAccount (Model)
**Responsibility:** Store cloud account information

```csharp
public class CloudStorageAccount
{
	public int Id { get; set; }
	public string Provider { get; set; }        // "Google"
	public string UserEmail { get; set; }       // user@gmail.com
	public string AccessToken { get; set; }     // OAuth access token
	public string RefreshToken { get; set; }    // OAuth refresh token
	public DateTime TokenExpiresAt { get; set; } // Token expiry
	public bool IsConnected { get; set; }       // Connection status
	public DateTime ConnectedDate { get; set; } // When connected
	public DateTime? LastSyncDate { get; set; } // Last sync time
	public int TotalFilesSynced { get; set; }   // Files organized
}
```

---

## 🐛 Troubleshooting

### Issue: "credentials.json not found"

**Problem:** Application says "credentials.json not found at..."

**Solutions:**
1. Verify file exists in the correct directory
2. Check file name is exactly: `credentials.json` (case-sensitive)
3. Rebuild the project and run again
4. Check project file (.csproj) has:
   ```xml
   <ItemGroup>
	 <None Update="credentials.json">
	   <CopyToOutputDirectory>Always</CopyToOutputDirectory>
	 </None>
   </ItemGroup>
   ```

### Issue: "Authentication failed"

**Problem:** Login process fails or hangs

**Solutions:**
1. Check internet connection
2. Verify OAuth credentials in Google Cloud Console
3. Ensure Google Drive API is enabled
4. Check browser opens correctly
5. Try revoking access and re-authenticating

### Issue: "No files appear in list"

**Problem:** Login successful but file list is empty

**Solutions:**
1. Ensure you have files in your Google Drive
2. Files must not be in trash
3. Only non-folder files are listed
4. Click "Refresh Files" button
5. Check browser console (F12) for API errors

### Issue: "Token expired"

**Problem:** Getting "Invalid token" or "Token expired" errors

**Solutions:**
1. The app should automatically refresh tokens
2. If not, click "Disconnect" then "Connect Google Drive" again
3. Tokens are stored in: `%AppData%\FileOrganizer\google_oauth_tokens`

---

## 🔒 Security Considerations

1. **Never commit credentials.json to Git**
   - It's in `.gitignore` (verify)
   - Delete it if accidentally committed

2. **Tokens are stored locally**
   - Location: `%AppData%\FileOrganizer\google_oauth_tokens`
   - Only accessible by your Windows user account

3. **Use OAuth Scopes Minimally**
   - Current scope: `DriveService.Scope.Drive` (read/write all files)
   - More restrictive scopes available if needed

4. **Revoke Access When Needed**
   - Use "Disconnect" button to revoke
   - Removes tokens from local storage

---

## 📱 Feature Workflow

```
User Interaction Flow:
1. User clicks "Connect Google Drive"
   ↓
2. Browser opens (Google login)
   ↓
3. User authenticates and grants permission
   ↓
4. Browser closes, app receives tokens
   ↓
5. Tokens saved to database and local storage
   ↓
6. DriveService initialized with credentials
   ↓
7. User can now:
   - List files
   - Organize files
   - Track sync history
```

---

## ✅ Verification Checklist

- [ ] Google Cloud Project created
- [ ] Google Drive API enabled
- [ ] OAuth 2.0 credentials created
- [ ] credentials.json downloaded
- [ ] credentials.json placed in correct directory
- [ ] Application builds successfully
- [ ] Login works (browser opens)
- [ ] Files appear in list after login
- [ ] Can disconnect successfully
- [ ] Status updates correctly

---

## 📞 Support

For issues with:
- **Google Cloud Setup:** https://support.google.com/cloud/
- **Google Drive API:** https://developers.google.com/drive/api
- **OAuth 2.0:** https://developers.google.com/identity/protocols/oauth2

---

## 🎓 Learning Resources

- Google Drive API Documentation: https://developers.google.com/drive/api/v3/about-sdk
- OAuth 2.0 for Desktop Apps: https://developers.google.com/identity/protocols/oauth2/native-app
- .NET Google Client Library: https://github.com/googleapis/google-api-dotnet-client

