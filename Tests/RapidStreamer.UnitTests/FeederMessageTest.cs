using JetBrains.Annotations;
using RapidStreamer.BuildingBlocks.Application;
using RapidStreamer.BuildingBlocks.Application.CorrelationId;

namespace RapidStreamer.UnitTests;

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

    private class TestFeederMessage : FeederMessage
    {
        public Guid Id
        {
            get => GetValueOrDefault(Guid.NewGuid());
            set => SetValue(value);
        }
    }
}