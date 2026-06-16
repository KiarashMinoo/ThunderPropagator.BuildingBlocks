using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
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

            ((INotifyPropertyChanged)configuration).PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            configuration.Name = "primary";

            Assert.Equal("primary", configuration.Name);
            Assert.Equal(["Name"], changedProperties);
        }

        [Fact]
        public void Set_ShouldNotRaisePropertyChanged_WhenValueDoesNotChange()
        {
            var configuration = new TestServiceConfiguration { Name = "primary" };
            var changedProperties = new List<string?>();

            ((INotifyPropertyChanged)configuration).PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

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

        // --- Issue #132 security tests: mass-assignment prevention ---

        [Fact]
        public void ReadJson_KnownPropertyKeys_AreDeserializedCorrectly()
        {
            const string json = "{\"name\":\"primary\",\"region\":\"us-east-1\"}";

            var config = JsonConvert.DeserializeObject<TestServiceConfiguration>(json);

            Assert.NotNull(config);
            Assert.Equal("primary", config.Name);
            Assert.Equal("us-east-1", config.Region);
        }

        [Fact]
        public void ReadJson_InjectedUnknownKey_IsNotPresentInProperties()
        {
            const string json = "{\"name\":\"primary\",\"injectedKey\":\"malicious\"}";

            var config = JsonConvert.DeserializeObject<TestServiceConfiguration>(json);

            Assert.NotNull(config);
            Assert.Equal("primary", config.Name);
            var enumeratedKeys = config.Select(kv => kv.Key).ToList();
            Assert.DoesNotContain("InjectedKey", enumeratedKeys);
            Assert.DoesNotContain("injectedKey", enumeratedKeys);
        }

        [Fact]
        public void ReadJson_MultipleInjectedKeys_NoneArePopulated()
        {
            const string json = "{\"name\":\"primary\",\"password\":\"secret\",\"__proto__\":\"polluted\",\"connectionString\":\"evil\"}";

            var config = JsonConvert.DeserializeObject<TestServiceConfiguration>(json);

            Assert.NotNull(config);
            Assert.Equal("primary", config.Name);
            var enumeratedKeys = config.Select(kv => kv.Key).ToList();
            Assert.DoesNotContain("Password", enumeratedKeys);
            Assert.DoesNotContain("ConnectionString", enumeratedKeys);
            Assert.Single(enumeratedKeys);
        }

        [Fact]
        public void ReadJson_EmptyJson_ProducesEmptyConfiguration()
        {
            const string json = "{}";

            var config = JsonConvert.DeserializeObject<TestServiceConfiguration>(json);

            Assert.NotNull(config);
            Assert.Null(config.Name);
            Assert.Null(config.Region);
        }

        [Fact]
        public void ReadJson_ConfigurationWithNoProperties_AcceptsAllKeys()
        {
            // A raw-bag subclass with no declared properties has no surface to protect,
            // so all incoming keys must pass through unchanged.
            const string json = "{\"anyKey\":\"value1\",\"anotherKey\":\"value2\"}";

            var config = JsonConvert.DeserializeObject<EmptyServiceConfiguration>(json);

            Assert.NotNull(config);
            var keys = config.Select(kv => kv.Key).ToList();
            Assert.Contains("AnyKey", keys);
            Assert.Contains("AnotherKey", keys);
        }

        // --- Issue #134 security tests ---

        [Fact]
        public void PropertyChanged_IsNotDirectlyAccessibleOnConcreteType()
        {
            // Explicit interface implementation — no public "PropertyChanged" event
            // should be visible directly on the concrete type.
            var publicEvent = typeof(TestServiceConfiguration).GetEvent(
                "PropertyChanged", BindingFlags.Public | BindingFlags.Instance);

            Assert.Null(publicEvent);
        }

        [Fact]
        public void PropertyChanging_IsNotDirectlyAccessibleOnConcreteType()
        {
            var publicEvent = typeof(TestServiceConfiguration).GetEvent(
                "PropertyChanging", BindingFlags.Public | BindingFlags.Instance);

            Assert.Null(publicEvent);
        }

        [Fact]
        public void PropertyChanged_ViaInterface_StillFiresOnSet()
        {
            var configuration = new TestServiceConfiguration();
            var changedProperties = new List<string?>();
            ((INotifyPropertyChanged)configuration).PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            configuration.Name = "interface-subscribed";

            Assert.Equal(["Name"], changedProperties);
        }

        [Fact]
        public void PropertyChanging_ViaInterface_StillFiresOnSet()
        {
            var configuration = new TestServiceConfiguration();
            var changingProperties = new List<string?>();
            ((INotifyPropertyChanging)configuration).PropertyChanging += (_, args) => changingProperties.Add(args.PropertyName);

            configuration.Name = "interface-subscribed";

            Assert.Equal(["Name"], changingProperties);
        }

        [Fact]
        public void CreateNew_WithUnknownKeys_FiltersThemOut()
        {
            var properties = new Dictionary<string, string>
            {
                ["Name"] = "primary",
                ["InjectedKey"] = "malicious",
                ["Password"] = "secret"
            };

            var config = ServiceConfiguration.CreateNew<TestServiceConfiguration>(properties);

            Assert.Equal("primary", config.Name);
            var keys = config.Select(kv => kv.Key).ToList();
            Assert.DoesNotContain("InjectedKey", keys);
            Assert.DoesNotContain("Password", keys);
            Assert.Single(keys);
        }

        [Fact]
        public void CreateNew_WithKnownKeys_PopulatesCorrectly()
        {
            var properties = new Dictionary<string, string>
            {
                ["Name"] = "primary",
                ["Region"] = "us-east-1"
            };

            var config = ServiceConfiguration.CreateNew<TestServiceConfiguration>(properties);

            Assert.Equal("primary", config.Name);
            Assert.Equal("us-east-1", config.Region);
        }

        [Fact]
        public void CreateNew_WithNoPropertiesSubclass_AcceptsAllKeys()
        {
            var properties = new Dictionary<string, string>
            {
                ["AnyKey"] = "value1",
                ["AnotherKey"] = "value2"
            };

            var config = ServiceConfiguration.CreateNew<EmptyServiceConfiguration>(properties);

            var keys = config.Select(kv => kv.Key).ToList();
            Assert.Contains("AnyKey", keys);
            Assert.Contains("AnotherKey", keys);
        }

        private class EmptyServiceConfiguration : ServiceConfiguration { }

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
