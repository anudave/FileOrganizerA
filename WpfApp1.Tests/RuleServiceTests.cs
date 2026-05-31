using Xunit;
using WpfApp1.Data;
using WpfApp1.Services;
using WpfApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace WpfApp1.Tests
{
    public class RuleServiceTests
    {
        private FileOrganizerContext GetTestContext()
        {
            var options = new DbContextOptionsBuilder<FileOrganizerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new FileOrganizerContext(options);
        }

        [Fact]
        public void CreateRule_Should_Add_New_Rule()
        {
            // Arrange
            var context = GetTestContext();
            var service = new RuleManagementService(context);

            // Act
            var rule = service.CreateRule("PDF Files", "*.pdf", "C:\\Documents\\PDFs", true);

            // Assert
            Assert.NotNull(rule);
            Assert.Equal("PDF Files", rule.RuleName);
            Assert.Equal("*.pdf", rule.FilePattern);
            Assert.Equal("C:\\Documents\\PDFs", rule.DestinationFolder);
            Assert.True(rule.IsActive);
        }

        [Fact]
        public void GetAllRules_Should_Return_All_Rules()
        {
            // Arrange
            var context = GetTestContext();
            var service = new RuleManagementService(context);

            service.CreateRule("Rule 1", "*.pdf", "C:\\Path1", true);
            service.CreateRule("Rule 2", "*.jpg", "C:\\Path2", true);

            // Act
            var rules = service.GetAllRules();

            // Assert
            Assert.NotNull(rules);
            Assert.Equal(2, rules.Count);
        }

        [Fact]
        public void UpdateRule_Should_Modify_Existing_Rule()
        {
            // Arrange
            var context = GetTestContext();
            var service = new RuleManagementService(context);
            var rule = service.CreateRule("Original", "*.pdf", "C:\\Original", true);

            // Act
            var updated = service.UpdateRule(rule.Id, "Updated", "*.docx", "C:\\Updated", false);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("Updated", updated.RuleName);
            Assert.Equal("*.docx", updated.FilePattern);
            Assert.False(updated.IsActive);
        }

        [Fact]
        public void DeleteRule_Should_Remove_Rule()
        {
            // Arrange
            var context = GetTestContext();
            var service = new RuleManagementService(context);
            var rule = service.CreateRule("ToDelete", "*.pdf", "C:\\Path", true);

            // Act
            var result = service.DeleteRule(rule.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Rule_Should_Have_CreatedDate()
        {
            // Arrange
            var context = GetTestContext();
            var service = new RuleManagementService(context);
            var before = DateTime.Now;

            // Act
            var rule = service.CreateRule("DateTest", "*.pdf", "C:\\Path", true);
            var after = DateTime.Now;

            // Assert
            Assert.True(rule.CreatedDate >= before);
            Assert.True(rule.CreatedDate <= after.AddSeconds(1));
        }
    }
}
