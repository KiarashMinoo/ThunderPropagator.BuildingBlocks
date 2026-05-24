using System.Security.Cryptography.X509Certificates;

namespace ThunderPropagator.BuildingBlocks.Application.Certificate;

public class CertificateModel
{
    private string? _path;
    private byte[]? _rawData;
    private string? _passphrase;
    private X509KeyStorageFlags? _keyStorageFlags;

    public string? Path
    {
        get => _path;
        set
        {
            _path = value;
            GenerateCertificate();
        }
    }

    public byte[]? RawData
    {
        get => _rawData;
        set
        {
            _rawData = value;
            GenerateCertificate();
        }
    }

    public string? Passphrase
    {
        get => _passphrase;
        set
        {
            _passphrase = value;
            GenerateCertificate();
        }
    }

    public X509KeyStorageFlags? KeyStorageFlags
    {
        get => _keyStorageFlags;
        set
        {
            _keyStorageFlags = value;
            GenerateCertificate();
        }
    }

    public X509Certificate2? Certificate { get; private set; }

    private void GenerateCertificate()
    {
        var certificate = CreateCertificate();
        var oldCertificate = Certificate;

        Certificate = certificate;
        oldCertificate?.Dispose();
    }

    private X509Certificate2? CreateCertificate()
    {
        if (!string.IsNullOrWhiteSpace(Path))
        {
#if NET9_0_OR_GREATER
            return !string.IsNullOrWhiteSpace(Passphrase)
                ? KeyStorageFlags != null
                    ? X509CertificateLoader.LoadPkcs12FromFile(Path, Passphrase, KeyStorageFlags.Value)
                    : X509CertificateLoader.LoadPkcs12FromFile(Path, Passphrase)
                : X509CertificateLoader.LoadCertificateFromFile(Path);
#else
            return !string.IsNullOrWhiteSpace(Passphrase)
                ? KeyStorageFlags != null
                    ? new X509Certificate2(Path, Passphrase, KeyStorageFlags.Value)
                    : new X509Certificate2(Path, Passphrase)
                : new X509Certificate2(Path);
#endif
        }

        if (RawData is { Length: > 0 })
        {
#if NET9_0_OR_GREATER
            return !string.IsNullOrWhiteSpace(Passphrase)
                ? KeyStorageFlags != null
                    ? X509CertificateLoader.LoadPkcs12(RawData, Passphrase, KeyStorageFlags.Value)
                    : X509CertificateLoader.LoadPkcs12(RawData, Passphrase)
                : X509CertificateLoader.LoadCertificate(RawData);
#else
            return !string.IsNullOrWhiteSpace(Passphrase)
                ? KeyStorageFlags != null
                    ? new X509Certificate2(RawData, Passphrase, KeyStorageFlags.Value)
                    : new X509Certificate2(RawData, Passphrase)
                : new X509Certificate2(RawData);

#endif
        }

        return null;
    }
}
