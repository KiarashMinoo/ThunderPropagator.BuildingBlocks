using RapidStreamer.BuildingBlocks.Application.Enums;

namespace RapidStreamer.UnitTests;

public class ReflectionTests
{
    [Fact]
    public void Reflection_Type_Name_Must_Equals()
    {
        // Arrange
        var type = typeof(TimeSpan);

        // Act
        var name = type.Name;

        // Assert
        Assert.Equal(nameof(TimeSpan), name);
    }

    [Fact]
    public void Enum_Must_Convert()
    {
        // Arrange
        var value = nameof(DataType.String);
        var type = typeof(DataType);

        // Act
        var success = Enum.TryParse(type, value, out var enumValue);
        var converted = Convert.ChangeType(enumValue, type);

        // Assert
        Assert.True(success);
        Assert.Equal(DataType.String, enumValue);
        Assert.True(converted is DataType);
    }
}