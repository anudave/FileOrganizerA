using System;

namespace WpfApp1.Models
{
    public class CloudStorageAccount
    {
        public int Id { get; set; }
        public string Provider { get; set; } // "GoogleDrive", "OneDrive", etc.
        public string UserEmail { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
        public bool IsConnected { get; set; }
        public DateTime ConnectedDate { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public int TotalFilesSynced { get; set; }
    }

    public class CloudOrganizationLog
    {
        public int Id { get; set; }
        public int CloudStorageAccountId { get; set; }
        public string FileName { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string Status { get; set; } // Success, Failed, Skipped
        public string Message { get; set; }
        public DateTime ProcessedDate { get; set; }
        public long FileSizeBytes { get; set; }
    }
}
