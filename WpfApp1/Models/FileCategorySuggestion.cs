namespace WpfApp1.Models
{
    /// <summary>
    /// Represents an AI-generated suggestion for file categorization
    /// </summary>
    public class FileCategorySuggestion
    {
        public int Id { get; set; }

        /// <summary>
        /// Suggested file category (e.g., "Documents", "Images", "Archives")
        /// </summary>
        public string SuggestedCategory { get; set; }

        /// <summary>
        /// Recommended destination folder for organizing the file
        /// </summary>
        public string DestinationFolder { get; set; }

        /// <summary>
        /// Confidence score (0-100) indicating how certain the suggestion is
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// JSON array of reasons why this suggestion was made
        /// </summary>
        public string Reasons { get; set; }

        /// <summary>
        /// Number of times user accepted this suggestion type
        /// </summary>
        public int TimesAccepted { get; set; }

        /// <summary>
        /// Number of times user rejected this suggestion type
        /// </summary>
        public int TimesRejected { get; set; }

        /// <summary>
        /// When this suggestion was generated
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// File extension that triggered this suggestion
        /// </summary>
        public string FileExtension { get; set; }
    }
}
