namespace WpfApp1.Models
{
    /// <summary>
    /// Result of AI suggestion analysis for a file
    /// </summary>
    public class SuggestionResult
    {
        public string FileName { get; set; }
        public List<FileCategorySuggestion> Suggestions { get; set; } = new();
        public string TopSuggestionReason { get; set; }
        public bool HasConfidentSuggestion { get; set; }
    }
}
