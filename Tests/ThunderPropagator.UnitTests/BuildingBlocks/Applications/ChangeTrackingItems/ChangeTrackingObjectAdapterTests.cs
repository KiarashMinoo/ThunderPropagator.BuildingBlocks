using ThunderPropagator.BuildingBlocks.Application.ChangeTrackingItems;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.ChangeTrackingItems
{
    public
#if !DEBUG
        sealed
#endif
        class ChangeTrackingObjectAdapterTests
    {
        // Test to verify that tracking can be started
        [Fact]
        public void BeginTracking_ShouldEnableTrackingAndClearCollection()
        {
            // Arrange
            var adapter = new ChangeTrackingObjectAdapter<string, string>();

            // Act
            var result = adapter.BeginTracking();

            // Assert
            Assert.True(result); // Should return true
            Assert.True(adapter.Enabled); // Tracking should be enabled
        }

        // Test to verify that tracking can be ended
        [Fact]
        public void EndTracking_ShouldDisableTrackingAndReturnCollection()
        {
            // Arrange
            var adapter = new ChangeTrackingObjectAdapter<string, string>();
            adapter.BeginTracking();

            // Act
            var items = adapter.EndTracking();

            // Assert
            Assert.False(adapter.Enabled); // Tracking should be disabled
            Assert.Empty(items); // Should return an empty collection
        }

        // Test to verify that changes can be reported when tracking is enabled
        [Fact]
        public void Report_ShouldAddChangeTrackingItemWhenEnabled()
        {
            // Arrange
            var adapter = new ChangeTrackingObjectAdapter<string, string>();
            adapter.BeginTracking();

            // Act
            var result = adapter.ReportAdded("key1", "value1");

            // Assert
            Assert.True(result); // Change should be reported successfully
        }

        // Test to verify that changes cannot be reported when tracking is disabled
        [Fact]
        public void Report_ShouldNotAddChangeTrackingItemWhenDisabled()
        {
            // Arrange
            var adapter = new ChangeTrackingObjectAdapter<string, string>();

            // Act
            var result = adapter.ReportAdded("key1", "value1");

            // Assert
            Assert.False(result); // Change should not be reported
        }

        // Test to verify that the Clear method empties the change tracking items
        [Fact]
        public void Clear_ShouldEmptyCollection()
        {
            // Arrange
            var adapter = new ChangeTrackingObjectAdapter<string, string>();
            adapter.BeginTracking();
            adapter.ReportAdded("key1", "value1");

            // Act
            adapter.Clear();

            // Assert
            var items = adapter.EndTracking(); // End tracking to get the collection
            Assert.Empty(items); // The collection should be empty
        }

        // Test to verify that reporting modified values works correctly
        [Fact]
        public void ReportModified_ShouldAddModifiedChangeTrackingItemWhenEnabled()
        {
            // Arrange
            var adapter = new ChangeTrackingObjectAdapter<string, string>();
            adapter.BeginTracking();

            // Act
            var result = adapter.ReportModified("key1", "oldValue", "newValue");

            // Assert
            Assert.True(result); // Change should be reported successfully
        }

        // Test to verify that reporting removed values works correctly
        [Fact]
        public void ReportRemoved_ShouldAddRemovedChangeTrackingItemWhenEnabled()
        {
            // Arrange
            var adapter = new ChangeTrackingObjectAdapter<string, string>();
            adapter.BeginTracking();

            // Act
            var result = adapter.ReportRemoved("key1", "oldValue");

            // Assert
            Assert.True(result); // Change should be reported successfully
        }
    }
}