using Xunit;
using WpfApp1.Data;
using WpfApp1.Services;

namespace WpfApp1.Tests
{
    public class FileOrganizationServiceTests
    {
        private FileOrganizerContext GetTestContext()
        {
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<FileOrganizerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new FileOrganizerContext(options);
        }

        [Fact]
        public void MatchesRule_With_Valid_Pattern_Returns_True()
        {
            // Arrange
            var context = GetTestContext();
            var service = new FileOrganizationService(context);
            var rule = new WpfApp1.Models.FileOrganizationRule
            {
                RuleName = "PDF Rule",
                FilePattern = "*.pdf",
                DestinationFolder = "C:\\PDFs",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // Act
            var result = service.MatchesRule("document.pdf", rule);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void MatchesRule_With_Invalid_Pattern_Returns_False()
        {
            // Arrange
            var context = GetTestContext();
            var service = new FileOrganizationService(context);
            var rule = new WpfApp1.Models.FileOrganizationRule
            {
                RuleName = "PDF Rule",
                FilePattern = "*.pdf",
                DestinationFolder = "C:\\PDFs",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // Act
            var result = service.MatchesRule("image.jpg", rule);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void MatchesRule_With_Wildcard_Pattern_Matches_Correctly()
        {
            // Arrange
            var context = GetTestContext();
            var service = new FileOrganizationService(context);
            var rule = new WpfApp1.Models.FileOrganizationRule
            {
                RuleName = "Doc Rule",
                FilePattern = "*.doc*",
                DestinationFolder = "C:\\Docs",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // Act
            var resultDocx = service.MatchesRule("document.docx", rule);
            var resultDoc = service.MatchesRule("document.doc", rule);
            var resultTxt = service.MatchesRule("document.txt", rule);

            // Assert
            Assert.True(resultDocx);
            Assert.True(resultDoc);
            Assert.False(resultTxt);
        }

        [Fact]
        public void GetAllFilesInFolder_Returns_List()
        {
            // Arrange
            var context = GetTestContext();
            var service = new FileOrganizationService(context);
            string testFolder = Path.Combine(Path.GetTempPath(), "TestFolder_" + Guid.NewGuid().ToString());

            try
            {
                // Create test folder
                Directory.CreateDirectory(testFolder);
                File.WriteAllText(Path.Combine(testFolder, "file1.txt"), "content");
                File.WriteAllText(Path.Combine(testFolder, "file2.pdf"), "content");

                // Act
                var files = service.GetAllFilesInFolder(testFolder);

                // Assert
                Assert.NotNull(files);
                Assert.Equal(2, files.Count);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(testFolder))
                    Directory.Delete(testFolder, true);
            }
        }
    }
}
