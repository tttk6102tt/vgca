using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto.Parameters;

namespace Sign.Org.BouncyCastle.Cms
{
    internal interface CmsSecureReadable
    {
        AlgorithmIdentifier Algorithm { get; }

        object CryptoObject { get; }

        CmsReadable GetReadable(KeyParameter key);
    }
}
