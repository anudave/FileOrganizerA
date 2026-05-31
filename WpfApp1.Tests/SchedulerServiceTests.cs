using Xunit;
using WpfApp1.Data;
using WpfApp1.Services;

namespace WpfApp1.Tests
{
    public class SchedulerServiceTests
    {
        private FileOrganizerContext GetTestContext()
        {
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<FileOrganizerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new FileOrganizerContext(options);
        }

        [Fact]
        public void StartScheduler_Should_Not_Throw()
        {
            // Arrange
            var context = GetTestContext();
            var fileService = new FileOrganizationService(context);
            var service = new SchedulerService(context, fileService);

            // Act & Assert
            service.StartScheduler();
            service.StopScheduler();
            // If we get here without exception, test passes
            Assert.True(true);
        }

        [Fact]
        public void StopScheduler_Should_Not_Throw()
        {
            // Arrange
            var context = GetTestContext();
            var fileService = new FileOrganizationService(context);
            var service = new SchedulerService(context, fileService);

            // Act & Assert
            service.StartScheduler();
            service.StopScheduler();
            Assert.True(true);
        }

        [Fact]
        public void Scheduler_Should_Initialize_Successfully()
        {
            // Arrange
            var context = GetTestContext();
            var fileService = new FileOrganizationService(context);

            // Act
            var service = new SchedulerService(context, fileService);

            // Assert
            Assert.NotNull(service);
        }
    }
}
