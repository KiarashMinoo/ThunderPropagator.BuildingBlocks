using RapidStreamer.BuildingBlocks.Application.Collections;
using RapidStreamer.BuildingBlocks.Application.Objects;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Collections
{
    public
#if !DEBUG
        sealed
#endif
        class BindingDictionaryTests
    {
        [Fact]
        public void Add_ShouldAddKeyValuePair()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();
            var key = "apple";
            var value = 1;

            // Act
            dict.Add(key, value);

            // Assert
            Assert.True(dict.ContainsKey(key));
            Assert.Equal(value, dict[key]);
        }

        [Fact]
        public void Remove_ShouldRemoveKeyValuePair()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();
            dict.Add("banana", 2);

            // Act
            var removed = dict.Remove("banana");

            // Assert
            Assert.True(removed);
            Assert.False(dict.ContainsKey("banana"));
        }

        [Fact]
        public void Clear_ShouldEmptyDictionary()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();
            dict.Add("cherry", 3);
            dict.Add("date", 4);

            // Act
            dict.Clear();

            // Assert
            Assert.Empty(dict);
        }

        [Fact]
        public void AddOrUpdate_ShouldAddValue_WhenKeyDoesNotExist()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();
            var key = "fig";
            var value = 5;

            // Act
            dict.AddOrUpdate(key, value);

            // Assert
            Assert.Equal(value, dict[key]);
        }

        [Fact]
        public void AddOrUpdate_ShouldUpdateValue_WhenKeyExists()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();
            var key = "grape";
            dict.Add(key, 6);
            var newValue = 7;

            // Act
            dict.AddOrUpdate(key, newValue);

            // Assert
            Assert.Equal(newValue, dict[key]);
        }

        [Fact]
        public void TryGetValue_ShouldReturnTrue_WhenKeyExists()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();
            var key = "honeydew";
            dict.Add(key, 8);

            // Act
            var result = dict.TryGetValue(key, out var value);

            // Assert
            Assert.True(result);
            Assert.Equal(8, value);
        }

        [Fact]
        public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();

            // Act
            var result = dict.TryGetValue("nonexistent", out var value);

            // Assert
            Assert.False(result);
            Assert.Equal(default, value); // Assuming int default is 
        }

        [Fact]
        public void ValueChangedEvent_ShouldTrigger_WhenValueIsUpdated()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();
            var isTriggered = false;
            dict.ValueChanged += (sender, key, value, changeType) =>
            {
                isTriggered = true;
                Assert.Equal("kiwi", key);
                Assert.Equal(9, value);
                Assert.Equal(NotifiableObject.NotifiableChangeType.Modified, changeType);
            };

            dict.Add("kiwi", 8);

            // Act
            dict["kiwi"] = 9;

            // Assert
            Assert.True(isTriggered);
        }

        [Fact]
        public void ClearedEvent_ShouldTrigger_WhenCleared()
        {
            // Arrange
            var dict = new BindingDictionary<string, int>();
            dict.Add("lemon", 10);
            var isTriggered = false;
            dict.Cleared += sender => isTriggered = true;

            // Act
            dict.Clear();

            // Assert
            Assert.True(isTriggered);
        }
    }
}