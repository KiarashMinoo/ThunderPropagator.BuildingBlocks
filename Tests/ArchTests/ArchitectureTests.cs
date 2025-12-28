using NetArchTest.Rules;
using Xunit;

namespace ThunderPropagator.ArchTests
{
    public class ArchitectureTests
    {
        [Fact]
        public void Application_Should_Not_Depend_On_Infrastructure()
        {
            var result = Types.InAssembly(typeof(ThunderPropagator.BuildingBlocks.Application.ServiceConfiguration).Assembly)
                .That().ResideInNamespace("ThunderPropagator.BuildingBlocks.Application")
                .ShouldNot().HaveDependencyOn("ThunderPropagator.BuildingBlocks.Infrastructure").GetResult();

            Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames));
        }
    }
}