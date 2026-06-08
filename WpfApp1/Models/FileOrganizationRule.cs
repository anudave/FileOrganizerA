namespace WpfApp1.Models
{
    public class FileOrganizationRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public string FilePattern { get; set; } // e.g., "*.pdf|*.doc|*.docx"
        public string Category { get; set; } // e.g., "Documents", "Images", "Videos" (nullable for backward compatibility)
        public string DestinationFolder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
