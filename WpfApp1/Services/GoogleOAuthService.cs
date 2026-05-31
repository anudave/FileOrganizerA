using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    /// <summary>
    /// Handles Google OAuth 2.0 authentication and Drive service initialization
    /// </summary>
    public class GoogleOAuthService
    {
        private readonly string _credentialsPath;
        private readonly string _tokenStorePath;
        private UserCredential _userCredential;

        public GoogleOAuthService()
        {
            // Path to credentials.json (downloaded from Google Cloud Console)
            _credentialsPath = Path.Combine(AppContext.BaseDirectory, "credentials.json");

            // Path to store user tokens
            _tokenStorePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FileOrganizer",
                "google_oauth_tokens"
            );
        }

        /// <summary>
        /// Authenticate user with Google and open browser login dialog
        /// </summary>
        public async Task<UserCredential> AuthenticateAsync()
        {
            try
            {
                // Check if credentials.json exists
                if (!File.Exists(_credentialsPath))
                {
                    throw new FileNotFoundException(
                        $"credentials.json not found at {_credentialsPath}. " +
                        "Please download it from Google Cloud Console and place it in the application directory.");
                }

                // Ensure token storage directory exists
                Directory.CreateDirectory(_tokenStorePath);

                // Load credentials from file
                using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

                    // Use GoogleWebAuthorizationBroker for browser-based OAuth flow
                    _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        secrets,
                        new[] { DriveService.Scope.Drive },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(_tokenStorePath, true)
                    );
                }

                return _userCredential;
            }
            catch (Exception ex)
            {
                throw new Exception($"Authentication failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get authenticated DriveService instance
        /// </summary>
        public DriveService GetDriveService(UserCredential credential)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential), "User must be authenticated first");

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "FileOrganizer"
            });
        }

        /// <summary>
        /// Revoke user's Google Drive access
        /// </summary>
        public async Task RevokeAccessAsync()
        {
            try
            {
                if (_userCredential != null)
                {
                    await _userCredential.RevokeTokenAsync(CancellationToken.None);
                }

                // Delete stored tokens
                if (Directory.Exists(_tokenStorePath))
                {
                    Directory.Delete(_tokenStorePath, true);
                }

                _userCredential = null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to revoke access: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if user is already authenticated (tokens cached)
        /// </summary>
        public bool IsAuthenticated()
        {
            return Directory.Exists(_tokenStorePath) && 
                   Directory.GetFiles(_tokenStorePath, "*.json").Length > 0;
        }

        /// <summary>
        /// Get cached credential without browser flow
        /// </summary>
        public async Task<UserCredential> GetCachedCredentialAsync()
        {
            try
            {
                if (!File.Exists(_credentialsPath))
                {
                    return null;
                }

                Directory.CreateDirectory(_tokenStorePath);

                using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

                    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        secrets,
                        new[] { DriveService.Scope.Drive },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(_tokenStorePath, true)
                    );

                    return credential;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
