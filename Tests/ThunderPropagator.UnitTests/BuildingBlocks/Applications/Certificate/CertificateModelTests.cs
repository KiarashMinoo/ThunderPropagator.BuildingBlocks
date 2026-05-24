using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Xunit;
using ThunderPropagator.BuildingBlocks.Application.Certificate;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Certificate
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

        [Fact]
        public void Setting_New_RawData_Disposes_Previous_Certificate()
        {
            var model = new CertificateModel
            {
                RawData = CreateCertificateBytes("first")
            };
            var previousCertificate = model.Certificate;

            model.RawData = CreateCertificateBytes("second");

            Assert.NotNull(previousCertificate);
            Assert.NotSame(previousCertificate, model.Certificate);
            Assert.Equal(IntPtr.Zero, previousCertificate.Handle);
        }

        private static byte[] CreateCertificateBytes(string commonName)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                $"CN={commonName}",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            using var certificate = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(1));

            return certificate.Export(X509ContentType.Cert);
        }
    }
}
