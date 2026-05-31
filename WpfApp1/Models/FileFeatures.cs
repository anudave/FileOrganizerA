namespace WpfApp1.Models
{
    /// <summary>
    /// Represents extracted features from a file for ML analysis
    /// </summary>
    public class FileFeatures
    {
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string NameWithoutExtension { get; set; }

        /// <summary>
        /// General file type category (Document, Image, Video, Audio, Archive, Executable, Other)
        /// </summary>
        public string FileTypeCategory { get; set; }

        /// <summary>
        /// Extracted keywords from filename
        /// </summary>
        public List<string> NameKeywords { get; set; } = new();

        /// <summary>
        /// Length of filename (can indicate type)
        /// </summary>
        public int FileNameLength { get; set; }

        /// <summary>
        /// Contains numbers in filename
        /// </summary>
        public bool ContainsNumbers { get; set; }

        /// <summary>
        /// Contains special characters
        /// </summary>
        public bool ContainsSpecialChars { get; set; }

        /// <summary>
        /// Common file naming patterns detected
        /// </summary>
        public List<string> DetectedPatterns { get; set; } = new();
    }
}
