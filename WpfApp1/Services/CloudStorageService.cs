using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class CloudStorageService
    {
        private readonly FileOrganizerContext _dbContext;
        private readonly FileOrganizationService _organizationService;
        private DriveService _driveService;
        private CloudStorageAccount _currentAccount;

        public CloudStorageService(FileOrganizerContext dbContext, FileOrganizationService organizationService)
        {
            _dbContext = dbContext;
            _organizationService = organizationService;
        }

        /// <summary>
        /// Get Google Drive service instance
        /// </summary>
        public DriveService GetDriveService()
        {
            return _driveService;
        }

        /// <summary>
        /// Check if account is connected
        /// </summary>
        public bool IsConnected()
        {
            _currentAccount = _dbContext.CloudStorageAccounts.FirstOrDefault(a => a.IsConnected && a.Provider == "GoogleDrive");
            return _currentAccount != null && _driveService != null;
        }

        /// <summary>
        /// Get current connected account
        /// </summary>
        public CloudStorageAccount GetCurrentAccount()
        {
            return _currentAccount;
        }

        /// <summary>
        /// Initialize Drive Service with credentials
        /// </summary>
        public async Task InitializeDriveService(string accessToken)
        {
            try
            {
                var credential = GoogleCredential.FromAccessToken(accessToken);
                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "File Organizer App"
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing Drive service: {ex.Message}");
            }
        }

        /// <summary>
        /// List files from Google Drive
        /// </summary>
        public async Task<List<Google.Apis.Drive.v3.Data.File>> ListFilesAsync(string folderId = null)
        {
            try
            {
                var request = _driveService.Files.List();
                request.Spaces = "drive";
                request.Fields = "files(id, name, mimeType, size, createdTime, modifiedTime)";
                request.PageSize = 100;

                if (!string.IsNullOrEmpty(folderId))
                {
                    request.Q = $"'{folderId}' in parents and trashed=false";
                }
                else
                {
                    request.Q = "trashed=false and mimeType='application/vnd.google-apps.folder'";
                }

                var result = await request.ExecuteAsync();
                return result.Files?.ToList() ?? new List<Google.Apis.Drive.v3.Data.File>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing files: {ex.Message}");
            }
        }

        /// <summary>
        /// Download file from Google Drive
        /// </summary>
        public async Task<string> DownloadFileAsync(string fileId, string fileName, string localPath)
        {
            try
            {
                var request = _driveService.Files.Get(fileId);
                var filePath = Path.Combine(localPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await request.DownloadAsync(stream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file: {ex.Message}");
            }
        }

        /// <summary>
        /// Upload file to Google Drive
        /// </summary>
        public async Task<string> UploadFileAsync(string filePath, string folderId = null)
        {
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = Path.GetFileName(filePath)
                };

                if (!string.IsNullOrEmpty(folderId))
                {
                    fileMetadata.Parents = new List<string> { folderId };
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var request = _driveService.Files.Create(fileMetadata, stream, "application/octet-stream");
                    var result = await request.UploadAsync();

                    if (result.Status == Google.Apis.Upload.UploadStatus.Completed)
                    {
                        return request.ResponseBody.Id;
                    }
                    else
                    {
                        throw new Exception($"Upload failed with status: {result.Status}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file: {ex.Message}");
            }
        }

        /// <summary>
        /// Organize cloud files using local rules
        /// </summary>
        public async Task<List<CloudOrganizationLog>> OrganizeCloudFilesAsync(List<string> fileIds, string tempPath)
        {
            var logs = new List<CloudOrganizationLog>();

            try
            {
                var files = _driveService.Files.List();
                files.Spaces = "drive";
                files.Fields = "files(id, name, mimeType, size)";

                foreach (var fileId in fileIds)
                {
                    try
                    {
                        var fileRequest = _driveService.Files.Get(fileId);
                        var file = await fileRequest.ExecuteAsync();

                        // Download file
                        var localPath = await DownloadFileAsync(fileId, file.Name, tempPath);

                        // Apply organization rules
                        var rules = _dbContext.FileOrganizationRules.Where(r => r.IsActive).ToList();
                        string destinationFolder = null;

                        foreach (var rule in rules)
                        {
                            if (_organizationService.MatchesRule(file.Name, rule))
                            {
                                destinationFolder = rule.DestinationFolder;
                                break;
                            }
                        }

                        var log = new CloudOrganizationLog
                        {
                            CloudStorageAccountId = _currentAccount.Id,
                            FileName = file.Name,
                            SourcePath = $"GoogleDrive/{fileId}",
                            DestinationPath = destinationFolder ?? "Unorganized",
                            Status = destinationFolder != null ? "Success" : "Skipped",
                            Message = destinationFolder != null ? "Organized" : "No matching rule",
                            ProcessedDate = DateTime.Now,
                            FileSizeBytes = file.Size ?? 0
                        };

                        logs.Add(log);

                        // Cleanup temp file
                        if (File.Exists(localPath))
                        {
                            File.Delete(localPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        logs.Add(new CloudOrganizationLog
                        {
                            CloudStorageAccountId = _currentAccount.Id,
                            FileName = fileId,
                            Status = "Failed",
                            Message = ex.Message,
                            ProcessedDate = DateTime.Now
                        });
                    }
                }

                // Save logs to database
                _dbContext.CloudOrganizationLogs.AddRange(logs);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error organizing cloud files: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Save cloud storage account credentials
        /// </summary>
        public void SaveAccount(CloudStorageAccount account)
        {
            var existing = _dbContext.CloudStorageAccounts.FirstOrDefault(a => a.UserEmail == account.UserEmail && a.Provider == "GoogleDrive");

            if (existing != null)
            {
                existing.AccessToken = account.AccessToken;
                existing.RefreshToken = account.RefreshToken;
                existing.TokenExpiresAt = account.TokenExpiresAt;
                existing.IsConnected = true;
                existing.LastSyncDate = DateTime.Now;
                _dbContext.CloudStorageAccounts.Update(existing);
            }
            else
            {
                account.Provider = "GoogleDrive";
                account.ConnectedDate = DateTime.Now;
                account.IsConnected = true;
                _dbContext.CloudStorageAccounts.Add(account);
            }

            _dbContext.SaveChanges();
            _currentAccount = account;
        }

        /// <summary>
        /// Get all connected accounts
        /// </summary>
        public List<CloudStorageAccount> GetAllAccounts()
        {
            return _dbContext.CloudStorageAccounts.Where(a => a.IsConnected).ToList();
        }

        /// <summary>
        /// Disconnect account
        /// </summary>
        public void DisconnectAccount(int accountId)
        {
            var account = _dbContext.CloudStorageAccounts.Find(accountId);
            if (account != null)
            {
                account.IsConnected = false;
                _dbContext.CloudStorageAccounts.Update(account);
                _dbContext.SaveChanges();
            }
        }
    }
}
