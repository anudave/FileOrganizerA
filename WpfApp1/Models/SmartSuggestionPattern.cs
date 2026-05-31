namespace WpfApp1.Models
{
    /// <summary>
    /// Represents a learned pattern from file organization rules
    /// Used by ML engine to make intelligent suggestions
    /// </summary>
    public class SmartSuggestionPattern
    {
        public int Id { get; set; }

        /// <summary>
        /// File extension pattern (e.g., ".pdf", ".jpg", ".xlsx")
        /// </summary>
        public string FilePattern { get; set; }

        /// <summary>
        /// Detected category for this pattern (e.g., "Documents", "Images")
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Destination folder most commonly used for this pattern
        /// </summary>
        public string CommonDestinationFolder { get; set; }

        /// <summary>
        /// How many times this pattern has been used
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// Accuracy rating (0-1) based on user feedback
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// When this pattern was last learned/updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// Confidence in this pattern (0-100)
        /// </summary>
        public double Confidence { get; set; }
    }
}
