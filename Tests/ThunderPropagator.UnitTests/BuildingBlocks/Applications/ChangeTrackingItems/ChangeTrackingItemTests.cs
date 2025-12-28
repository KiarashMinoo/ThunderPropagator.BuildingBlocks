using ThunderPropagator.BuildingBlocks.Application.ChangeTrackingItems;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.ChangeTrackingItems
{
    public
#if !DEBUG
        sealed
#endif
        class ChangeTrackingItemTests
    {
        // Test to verify that the constructor correctly sets the ChangeType property
        [Fact]
        public void Constructor_ShouldSetChangeTypeCorrectly()
        {
            // Arrange
            var changeType = ChangeType.Modified;

            // Act
            var changeTrackingItem = new ChangeTrackingItem<string>(changeType, "old value", "new value");

            // Assert
            Assert.Equal(changeType, changeTrackingItem.ChangeType); // Verify ChangeType is set correctly
        }

        // Test to verify that the constructor correctly sets the PreviousValue property
        [Fact]
        public void Constructor_ShouldSetPreviousValueCorrectly()
        {
            // Arrange
            var previousValue = "old value";

            // Act
            var changeTrackingItem = new ChangeTrackingItem<string>(ChangeType.Modified, previousValue, "new value");

            // Assert
            Assert.Equal(previousValue, changeTrackingItem.PreviousValue); // Verify PreviousValue is set correctly
        }

        // Test to verify that the constructor correctly sets the NewValue property
        [Fact]
        public void Constructor_ShouldSetNewValueCorrectly()
        {
            // Arrange
            var newValue = "new value";

            // Act
            var changeTrackingItem = new ChangeTrackingItem<string>(ChangeType.Modified, "old value", newValue);

            // Assert
            Assert.Equal(newValue, changeTrackingItem.NewValue); // Verify NewValue is set correctly
        }

        // Test to verify that nullable values are handled correctly
        [Fact]
        public void Constructor_ShouldHandleNullableValues()
        {
            // Act
            var changeTrackingItem = new ChangeTrackingItem<int?>(ChangeType.Removed, null, 42);

            // Assert
            Assert.Null(changeTrackingItem.PreviousValue); // PreviousValue is null
            Assert.Equal(42, changeTrackingItem.NewValue); // NewValue is set to 42
        }

        // Test to verify that the class is not sealed in debug mode
#if DEBUG
        [Fact]
        public void ChangeTrackingItem_ShouldNotBeSealedInDebugMode()
        {
            // Act
            var type = typeof(ChangeTrackingItem<>);

            // Assert
            Assert.False(type.IsSealed); // The class should not be sealed in debug mode
        }
#endif

        // Test to verify that the class is sealed in non-debug mode
#if !DEBUG
    [Fact]
    public void ChangeTrackingItem_ShouldBeSealedInNonDebugMode()
    {
        // Act
        var type = typeof(ChangeTrackingItem<>);

        // Assert
        Assert.True(type.IsSealed);  // The class should be sealed in non-debug mode
    }
#endif
    }
}