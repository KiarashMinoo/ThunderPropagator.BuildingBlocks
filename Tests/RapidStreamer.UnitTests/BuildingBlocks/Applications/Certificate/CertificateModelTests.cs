using System.Security.Cryptography.X509Certificates;
using Xunit;
using RapidStreamer.BuildingBlocks.Application.Certificate;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Certificate
{
    public class CertificateModelTests
    {
        [Fact]
        public void Default_Certificate_Is_Null()
        {
            var model = new CertificateModel();
            Assert.Null(model.Certificate);
        }

        [Fact]
        public void Setting_Empty_RawData_Leaves_Certificate_Null()
        {
            var model = new CertificateModel();
            model.RawData = new byte[0];
            Assert.Null(model.Certificate);
        }

        [Fact]
        public void NotSettingPathOrRawData_Leaves_Certificate_Null()
        {
            var model = new CertificateModel();
            model.Passphrase = "secret";
            model.KeyStorageFlags = X509KeyStorageFlags.DefaultKeySet;
            Assert.Null(model.Certificate);
        }
    }
}