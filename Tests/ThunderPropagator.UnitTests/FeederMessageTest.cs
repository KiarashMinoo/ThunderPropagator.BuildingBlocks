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