namespace WpfApp1.Models
{
    public class DuplicateResolutionStrategy
    {
        public int Id { get; set; }
        public string StrategyName { get; set; } // Skip, Overwrite, Rename, KeepNewer
        public string Description { get; set; }
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
