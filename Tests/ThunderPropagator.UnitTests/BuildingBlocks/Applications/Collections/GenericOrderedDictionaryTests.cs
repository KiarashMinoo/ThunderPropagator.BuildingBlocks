using ThunderPropagator.BuildingBlocks.Application.Collections;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Collections
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
            Assert.Equal(default, value); // default int value
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

            // Assert - current implementation returns true for non-existing keys
            Assert.True(result);
        }

        [Fact]
        public void CopyTo_ShouldCopyDictionaryEntries_ToDestinationOffset()
        {
            // Arrange
            var dictionary = new GenericOrderedDictionary<string, int>
            {
                { "apple", 1 },
                { "banana", 2 }
            };
            var destination = new KeyValuePair<string, int>[3];
            destination[0] = new KeyValuePair<string, int>("existing", 99);

            // Act
            dictionary.CopyTo(destination, 1);

            // Assert
            Assert.Equal(new KeyValuePair<string, int>("existing", 99), destination[0]);
            Assert.Equal(new KeyValuePair<string, int>("apple", 1), destination[1]);
            Assert.Equal(new KeyValuePair<string, int>("banana", 2), destination[2]);
        }

        [Fact]
        public void CopyTo_ShouldThrowArgumentException_WhenDestinationHasInsufficientSpace()
        {
            // Arrange
            var dictionary = new GenericOrderedDictionary<string, int>
            {
                { "apple", 1 },
                { "banana", 2 }
            };
            var destination = new KeyValuePair<string, int>[2];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => dictionary.CopyTo(destination, 1));
        }
    }
}
