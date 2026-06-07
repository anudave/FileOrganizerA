namespace WpfApp1.Models
{
    public class ExclusionPattern
    {
        public int Id { get; set; }
        public string PatternName { get; set; }
        public string Pattern { get; set; } // e.g., "*.tmp", "*.cache", "Thumbs.db"
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
