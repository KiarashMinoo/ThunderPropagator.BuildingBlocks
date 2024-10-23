using RapidStreamer.BuildingBlocks.Application.ChangeTrackingItems;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.ChangeTrackingItems
{
    public
#if !DEBUG
        sealed
#endif
        class ChangeTrackingItemCollectionTests
    {
        // Test to verify that the collection starts empty
        [Fact]
        public void Constructor_ShouldInitializeEmptyCollection()
        {
            // Act
            var collection = new ChangeTrackingItemCollection<string, string>();

            // Assert
            Assert.Empty(collection); // The collection should be empty on initialization
        }

        // Test to verify that items can be added to the collection
        [Fact]
        public void Add_ShouldAddItemToCollection()
        {
            // Arrange
            var collection = new ChangeTrackingItemCollection<string, string>();
            var changeTrackingItem = new ChangeTrackingItem<string>(ChangeType.Added, null, "new value");

            // Act
            var result = collection.Add("key1", changeTrackingItem);

            // Assert
            Assert.True(result); // Item should be successfully added
            Assert.Single(collection); // Collection should have one item
        }

        // Test to verify that existing items are not updated unless forced
        [Fact]
        public void Add_ShouldNotUpdateExistingItemUnlessForced()
        {
            // Arrange
            var collection = new ChangeTrackingItemCollection<string, string>();
            var changeTrackingItem1 = new ChangeTrackingItem<string>(ChangeType.Added, null, "value1");
            var changeTrackingItem2 = new ChangeTrackingItem<string>(ChangeType.Modified, "value1", "value2");

            collection.Add("key1", changeTrackingItem1);

            // Act
            var resultWithoutForce = collection.Add("key1", changeTrackingItem2);
            var resultWithForce = collection.Add("key1", changeTrackingItem2, true);

            // Assert
            Assert.False(resultWithoutForce); // Item should not be updated without force
            Assert.True(resultWithForce); // Item should be updated with force
            Assert.Single(collection); // Collection should still have one item
        }

        // Test to verify that the collection can be cleared
        [Fact]
        public void Clear_ShouldEmptyCollection()
        {
            // Arrange
            var collection = new ChangeTrackingItemCollection<string, string>();
            var changeTrackingItem = new ChangeTrackingItem<string>(ChangeType.Added, null, "new value");

            collection.Add("key1", changeTrackingItem);

            // Act
            collection.Clear();

            // Assert
            Assert.Empty(collection); // Collection should be empty after clearing
        }

        // Test to verify that ToDictionary returns correct items filtered by ChangeType
        [Fact]
        public void ToDictionary_ShouldReturnItemsFilteredByChangeType()
        {
            // Arrange
            var collection = new ChangeTrackingItemCollection<string, string>();
            collection.Add("key1", new ChangeTrackingItem<string>(ChangeType.Added, null, "value1"));
            collection.Add("key2", new ChangeTrackingItem<string>(ChangeType.Modified, "value1", "value2"));

            // Act
            var addedItems = collection.ToDictionary(ChangeType.Added);

            // Assert
            Assert.Single(addedItems); // Only one item should be added
            Assert.Equal("value1", addedItems["key1"]); // Verify the added item
        }

        // Test to verify that GetItems retrieves items by ChangeType
        [Fact]
        public void GetItems_ShouldReturnCorrectItemsByChangeType()
        {
            // Arrange
            var collection = new ChangeTrackingItemCollection<string, string>();
            collection.Add("key1", new ChangeTrackingItem<string>(ChangeType.Added, null, "value1"));
            collection.Add("key2", new ChangeTrackingItem<string>(ChangeType.Modified, "value1", "value2"));

            // Act
            var addedItems = collection.GetAddedItems();
            var modifiedItems = collection.GetModifiedItems();

            // Assert
            Assert.Single(addedItems); // One added item
            Assert.Single(modifiedItems); // One modified item
        }

        // Test to verify that the collection can be enumerated
        [Fact]
        public void GetEnumerator_ShouldEnumerateItems()
        {
            // Arrange
            var collection = new ChangeTrackingItemCollection<string, string>();
            collection.Add("key1", new ChangeTrackingItem<string>(ChangeType.Added, null, "value1"));
            collection.Add("key2", new ChangeTrackingItem<string>(ChangeType.Modified, "value1", "value2"));

            // Act & Assert
            foreach (var item in collection)
            {
                Assert.NotNull(item.Key); // Check that the key is not null
                Assert.NotNull(item.Value); // Check that the value is not null
            }
        }
    }
}