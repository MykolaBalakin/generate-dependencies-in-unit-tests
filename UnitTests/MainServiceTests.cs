using Moq;
using Service;
using Xunit;

namespace UnitTests
{
    public class MainServiceTests
    {
        [Fact]
        public void TheTest()
        {
            var importantDependency = new Mock<IImportantDependency>();

            var service = ServiceBuilder
                .For<MainService>()
                .AddDependency(importantDependency.Object)
                .Build();

            service.DoWork(useNotImportantDependency: false);

            importantDependency.Verify(mock => mock.GetData(), Times.Once);
        }
    }
}