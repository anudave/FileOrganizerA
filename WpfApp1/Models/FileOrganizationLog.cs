namespace WpfApp1.Models
{
    public class FileOrganizationLog
    {
        public int Id { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public string Status { get; set; } // "Success", "Failed", "Pending"
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedDate { get; set; } = DateTime.Now;
        public long FileSizeBytes { get; set; }
    }
}
