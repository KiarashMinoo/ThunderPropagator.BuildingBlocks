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

        [Fact]
        public void Equals_ShouldReturnTrue_WhenAllPropertiesMatch()
        {
            var a = new TestServiceConfiguration { Name = "primary", Region = "us-east-1" };
            var b = new TestServiceConfiguration { Name = "primary", Region = "us-east-1" };

            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenSinglePropertyDiffers()
        {
            var a = new TestServiceConfiguration { Name = "primary", Region = "us-east-1" };
            var b = new TestServiceConfiguration { Name = "primary", Region = "eu-west-1" };

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenAllPropertiesDiffer()
        {
            var a = new TestServiceConfiguration { Name = "primary", Region = "us-east-1" };
            var b = new TestServiceConfiguration { Name = "secondary", Region = "eu-west-1" };

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenOtherHasFewerProperties()
        {
            var a = new TestServiceConfiguration { Name = "primary", Region = "us-east-1" };
            var b = new TestServiceConfiguration { Name = "primary" };

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenOtherIsNull()
        {
            var a = new TestServiceConfiguration { Name = "primary" };

            Assert.False(a.Equals(null));
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenSameReference()
        {
            var a = new TestServiceConfiguration { Name = "primary" };

            Assert.True(a.Equals(a));
        }

        private class TestServiceConfiguration : ServiceConfiguration
        {
            public string? Name
            {
                get => Get<string>();
                set => Set(value);
            }

            public string? Region
            {
                get => Get<string>();
                set => Set(value);
            }
        }
    }
}
