using ThunderPropagator.BuildingBlocks.Application;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications
{
    public class ServiceConfigurationTests
    {
        [Fact]
        public void Set_ShouldRaisePropertyChanged_WhenValueChanges()
        {
            var configuration = new TestServiceConfiguration();
            var changedProperties = new List<string?>();

            configuration.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            configuration.Name = "primary";

            Assert.Equal("primary", configuration.Name);
            Assert.Equal(["Name"], changedProperties);
        }

        [Fact]
        public void Set_ShouldNotRaisePropertyChanged_WhenValueDoesNotChange()
        {
            var configuration = new TestServiceConfiguration { Name = "primary" };
            var changedProperties = new List<string?>();

            configuration.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            configuration.Name = "primary";

            Assert.Empty(changedProperties);
        }

        private class TestServiceConfiguration : ServiceConfiguration
        {
            public string? Name
            {
                get => Get<string>();
                set => Set(value);
            }
        }
    }
}
