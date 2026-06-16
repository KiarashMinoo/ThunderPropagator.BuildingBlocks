using System.Reflection;
using JetBrains.Annotations;
using ThunderPropagator.BuildingBlocks.Application;
using ThunderPropagator.BuildingBlocks.Application.CorrelationId;

namespace ThunderPropagator.UnitTests;

[TestSubject(typeof(FeederMessage))]
public class FeederMessageTest
{
    [Fact]
    public void FeederMessage_CorrelationId_Must_Set_If_CorrelationId_Is_Null()
    {
        //Arrange
        var feederMessage = new TestFeederMessage();
        feederMessage.Id = Guid.NewGuid();

        //Act
        CorrelationIdSupportHelper.GenerateCorrelationId(feederMessage);

        //Assert
        Assert.NotNull(feederMessage.CorrelationId);
    }

    [Fact]
    public void Clone_Returns_New_Dictionary_Instance()
    {
        var message = new TestFeederMessage { Id = Guid.NewGuid() };

        var clone = ((ICloneable<IDictionary<string, object?>>)message).Clone();

        Assert.NotSame(message, clone);
    }

    [Fact]
    public void Clone_Mutations_Do_Not_Affect_Original()
    {
        var message = new TestFeederMessage { Id = Guid.NewGuid() };

        var clone = ((ICloneable<IDictionary<string, object?>>)message).Clone();
        clone["InjectedKey"] = "InjectedValue";

        Assert.False(((IDictionary<string, object?>)message).ContainsKey("InjectedKey"));
    }

    [Fact]
    public void SetValue_AfterDispose_ThrowsObjectDisposedException()
    {
        var message = new TestFeederMessage();
        message.Dispose();

        Assert.Throws<ObjectDisposedException>(() => message.Id = Guid.NewGuid());
    }

    [Fact]
    public void GetValue_AfterDispose_ThrowsObjectDisposedException()
    {
        var message = new TestFeederMessage();
        message.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = message.IdDirect);
    }

    [Fact]
    public void GetHashCode_ShouldRemainStable_WhenPayloadChanges()
    {
        var message = new TestFeederMessage();
        var hashCode = message.GetHashCode();

        message.Id = Guid.NewGuid();
        ((IDictionary<string, object?>)message).Add("Name", "value");

        Assert.Equal(hashCode, message.GetHashCode());
    }

    [Fact]
    public void Payload_ShouldSupportConcurrentReadsAndWrites()
    {
        var message = new TestFeederMessage();
        var payload = (IDictionary<string, object?>)message;

        Parallel.For(0, 1_000, index =>
        {
            var key = $"Key{index}";
            payload[key] = index;

            Assert.True(payload.TryGetValue(key, out var value));
            Assert.Equal(index, value);
        });

        Assert.Equal(1_000, payload.Count);
    }

    // --- Issue #133 security tests ---

    [Fact]
    public void Indexer_Setter_IsProtected_NotPubliclyAccessible()
    {
        var indexer = typeof(FeederMessage)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .First(p => p.GetIndexParameters().Length > 0);

        Assert.NotNull(indexer.SetMethod);
        Assert.False(indexer.SetMethod!.IsPublic);
        Assert.True(indexer.SetMethod!.IsFamily); // IsFamily == protected
    }

    [Fact]
    public void Reset_IsProtected_NotPubliclyAccessible()
    {
        var resetMethod = typeof(FeederMessage)
            .GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.NotNull(resetMethod);
        Assert.False(resetMethod!.IsPublic);
        Assert.True(resetMethod!.IsFamily); // IsFamily == protected
    }

    [Fact]
    public void IDictionaryIndexer_Write_StillAllowsPayloadMutation()
    {
        var message = new TestFeederMessage();
        var dict = (IDictionary<string, object?>)message;

        dict["SomeKey"] = "SomeValue";

        Assert.True(dict.TryGetValue("SomeKey", out var value));
        Assert.Equal("SomeValue", value);
    }

    [Fact]
    public void Reset_CanBeCalledBySubclass()
    {
        var message = new ResettableFeederMessage();
        message.Id = Guid.NewGuid();

        message.PublicReset();

        Assert.Null(message.Id == Guid.Empty ? (Guid?)null : message.Id);
    }

    private class ResettableFeederMessage : FeederMessage
    {
        public Guid? Id
        {
            get => GetValueOrNull<Guid>();
            set => SetValue(value);
        }

        public void PublicReset() => Reset();
    }

    private class TestFeederMessage : FeederMessage
    {
        public Guid Id
        {
            get => GetValueOrDefault(Guid.NewGuid());
            set => SetValue(value);
        }

        public Guid IdDirect => GetValue<Guid>();
    }
}
