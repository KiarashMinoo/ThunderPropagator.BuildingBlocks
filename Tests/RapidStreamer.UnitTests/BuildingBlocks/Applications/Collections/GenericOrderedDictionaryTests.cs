using RapidStreamer.BuildingBlocks.Application.Collections;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Collections
{
    public
#if !DEBUG
        sealed
#endif
        class GenericOrderedDictionaryTests
    {
        [Fact]
        public void Add_ShouldStoreValue_WhenAddingValidKeyValuePair()
        {
            // Arrange
            var dictionary = new GenericOrderedDictionary<string, int>();
            var key = "apple";
            var value = 1;

            // Act
            dictionary.Add(key, value);

            // Assert
            Assert.Equal(value, dictionary[key]);
        }

        [Fact]
        public void TryGetValue_ShouldReturnTrue_WhenKeyExists()
        {
            // Arrange
            var dictionary = new GenericOrderedDictionary<string, int>();
            var key = "banana";
            var value = 2;
            dictionary.Add(key, value);

            // Act
            bool result = dictionary.TryGetValue(key, out var retrievedValue);

            // Assert
            Assert.True(result);
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var dictionary = new GenericOrderedDictionary<string, int>();

            // Act
            bool result = dictionary.TryGetValue("nonexistent", out var value);

            // Assert
            Assert.False(result);
            Assert.Equal(default, value); // Assuming default int value is expected
        }

        [Fact]
        public void Remove_ShouldReturnTrue_WhenKeyExists()
        {
            // Arrange
            var dictionary = new GenericOrderedDictionary<string, int>();
            var key = "cherry";
            dictionary.Add(key, 3);

            // Act
            bool result = dictionary.Remove(key);

            // Assert
            Assert.True(result);
            Assert.False(dictionary.ContainsKey(key));
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var dictionary = new GenericOrderedDictionary<string, int>();
            var key = "date";

            // Act
            bool result = dictionary.Remove(key);

            // Assert
            Assert.False(result);
        }
    }
}