using NetArchTest.Rules;
using Xunit;

namespace RapidStreamer.ArchTests
{
    public class ArchitectureTests
    {
        [Fact]
        public void Application_Should_Not_Depend_On_Infrastructure()
        {
            var result = Types.InAssembly(typeof(RapidStreamer.BuildingBlocks.Application.ServiceConfiguration).Assembly)
                .That().ResideInNamespace("RapidStreamer.BuildingBlocks.Application")
                .ShouldNot().HaveDependencyOn("RapidStreamer.BuildingBlocks.Infrastructure").GetResult();

            Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames));
        }
    }
}