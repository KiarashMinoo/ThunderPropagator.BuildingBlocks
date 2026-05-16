using ThunderPropagator.BuildingBlocks.Application;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.UnitTests
{
    public class ExceptionInfoTests
    {
        [Fact]
        public void ExceptionInfo_SingleException_CapturesTypeMessageSource()
        {
            var ex = new ArgumentException("bad arg");

            var info = (ExceptionInfo)(Exception)ex;

            Assert.Equal("System.ArgumentException", info.Type);
            Assert.Equal("bad arg", info.Message);
            Assert.Null(info.InnerException);
        }

        [Fact]
        public void ExceptionInfo_OneInnerException_CapturesBothLevels()
        {
            var inner = new InvalidOperationException("inner");
            var outer = new Exception("outer", inner);

            var info = (ExceptionInfo)(Exception)outer;

            Assert.Equal("outer", info.Message);
            Assert.NotNull(info.InnerException);
            Assert.Equal("System.InvalidOperationException", info.InnerException!.Type);
            Assert.Equal("inner", info.InnerException.Message);
            Assert.Null(info.InnerException.InnerException);
        }

        [Fact]
        public void ExceptionInfo_DeepChain_CapturesAllNodes()
        {
            // 11 exceptions: outer + 10 inner levels
            Exception chain = new Exception("depth-10");
            for (var i = 9; i >= 0; i--)
                chain = new Exception($"depth-{i}", chain);

            var info = (ExceptionInfo)(Exception)chain;

            var count = 0;
            var current = info;
            while (current is not null)
            {
                count++;
                current = current.InnerException;
            }

            Assert.Equal(11, count);
        }

        [Fact]
        public void ExceptionInfo_ChainBeyondMaxDepth_IsTruncatedAtOuterPlusTenInner()
        {
            // 21 exceptions — only outer + 10 inner levels captured
            Exception chain = new Exception("leaf");
            for (var i = 0; i < 20; i++)
                chain = new Exception($"level-{i}", chain);

            var info = (ExceptionInfo)(Exception)chain;

            var count = 0;
            var current = info;
            while (current is not null)
            {
                count++;
                current = current.InnerException;
            }

            Assert.Equal(11, count);
        }

        [Fact]
        public void ExceptionInfo_RoundTripsViaJson()
        {
            var inner = new InvalidOperationException("inner message");
            var outer = new Exception("outer message", inner);

            var json = outer.ToJson();
            var info = json.FromJson<ExceptionInfo>();

            Assert.NotNull(info);
            Assert.Equal("outer message", info!.Message);
            Assert.NotNull(info.InnerException);
            Assert.Equal("inner message", info.InnerException!.Message);
        }
    }
}
