using ThunderPropagator.BuildingBlocks.Application.Attributes;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Attributes
{
    public
#if !DEBUG
        sealed
#endif
        class JsonSerializationAttributeTests
    {
        // Test class with the JsonSerializationAttribute applied
        [JsonSerialization(CamelCase = false)]
        private class TestClass
        {
        }

        // Test class without the JsonSerializationAttribute
        private class UnmarkedClass
        {
        }

        // Test to verify that the JsonSerializationAttribute is applied to classes
        [Fact]
        public void JsonSerializationAttribute_ShouldBeAppliedToClass()
        {
            // Arrange
            var classInfo = typeof(TestClass);

            // Act
            var attribute = classInfo.GetCustomAttribute(typeof(JsonSerializationAttribute), false);

            // Assert
            Assert.NotNull(attribute); // Check if the attribute is applied
        }

        // Test to verify that the JsonSerializationAttribute is not applied to unmarked classes
        [Fact]
        public void JsonSerializationAttribute_ShouldNotBeAppliedToUnmarkedClass()
        {
            // Arrange
            var classInfo = typeof(UnmarkedClass);

            // Act
            var attribute = classInfo.GetCustomAttribute(typeof(JsonSerializationAttribute), false);

            // Assert
            Assert.Null(attribute); // Check that the attribute is not applied
        }

        // Test to verify that the CamelCase property is correctly set when applying the attribute
        [Fact]
        public void JsonSerializationAttribute_CamelCase_ShouldBeSetCorrectly()
        {
            // Arrange
            var classInfo = typeof(TestClass);

            // Act
            var attribute = (JsonSerializationAttribute)classInfo.GetCustomAttribute(typeof(JsonSerializationAttribute), false)!;

            // Assert
            Assert.False(attribute.CamelCase); // Check if CamelCase was set to false
        }

        // Test to verify the default value of the CamelCase property
        [Fact]
        public void JsonSerializationAttribute_CamelCase_ShouldDefaultToTrue()
        {
            // Arrange
            var defaultAttribute = new JsonSerializationAttribute();

            // Assert
            Assert.True(defaultAttribute.CamelCase); // Check that the default is true
        }

        // Test to verify that JsonSerializationAttribute is restricted to classes
        [Fact]
        public void JsonSerializationAttribute_UsageIsRestrictedToClass()
        {
            // Arrange
            var attributeUsage = typeof(JsonSerializationAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            // Act
            var validTargets = attributeUsage!.ValidOn;

            // Assert
            Assert.True(validTargets.HasFlag(AttributeTargets.Class)); // Check that it can be applied to classes
            Assert.False(validTargets.HasFlag(AttributeTargets.Method)); // Verify it can't be applied to methods
            Assert.False(validTargets.HasFlag(AttributeTargets.Property)); // Verify it can't be applied to properties
        }
    }
}