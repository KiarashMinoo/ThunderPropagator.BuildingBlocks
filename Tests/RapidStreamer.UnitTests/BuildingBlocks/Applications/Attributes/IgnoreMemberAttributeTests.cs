using RapidStreamer.BuildingBlocks.Application.Attributes;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Attributes
{
    public
#if !DEBUG
        sealed
#endif
        class IgnoreMemberAttributeTests
    {
        // Test class with the IgnoreMemberAttribute applied
        private class TestClass
        {
            [IgnoreMember] public string TestProperty { get; set; } = null!;

            [IgnoreMember] public int TestField = 0;

            public string UnmarkedProperty { get; set; } = null!;
        }

        // Test to verify that the IgnoreMember attribute is applied to properties
        [Fact]
        public void IgnoreMemberAttribute_ShouldBeAppliedToProperties()
        {
            // Arrange
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));

            // Act
            var attribute = propertyInfo!.GetCustomAttribute(typeof(IgnoreMemberAttribute), false);

            // Assert
            Assert.NotNull(attribute); // Check if the attribute is applied
        }

        // Test to verify that the IgnoreMember attribute is applied to fields
        [Fact]
        public void IgnoreMemberAttribute_ShouldBeAppliedToFields()
        {
            // Arrange
            var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.TestField));

            // Act
            var attribute = fieldInfo!.GetCustomAttribute(typeof(IgnoreMemberAttribute), false);

            // Assert
            Assert.NotNull(attribute); // Check if the attribute is applied
        }

        // Test to verify that the IgnoreMember attribute is not applied to unmarked properties
        [Fact]
        public void IgnoreMemberAttribute_ShouldNotBeAppliedToUnmarkedProperties()
        {
            // Arrange
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.UnmarkedProperty));

            // Act
            var attribute = propertyInfo!.GetCustomAttribute(typeof(IgnoreMemberAttribute), false);

            // Assert
            Assert.Null(attribute); // Check that the attribute is not applied
        }

        // Test to verify that the IgnoreMember attribute can only be applied to properties or fields
        [Fact]
        public void IgnoreMemberAttribute_UsageIsRestrictedToPropertyAndField()
        {
            // Arrange
            var attributeUsage = typeof(IgnoreMemberAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            // Act
            var validTargets = attributeUsage!.ValidOn;

            // Assert
            Assert.True(validTargets.HasFlag(AttributeTargets.Property));
            Assert.True(validTargets.HasFlag(AttributeTargets.Field));
            Assert.False(validTargets.HasFlag(AttributeTargets.Method)); // Verify it can't be applied to methods
            Assert.False(validTargets.HasFlag(AttributeTargets.Class)); // Verify it can't be applied to classes
        }
    }
}