using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Cms
{
    internal interface RecipientInfoGenerator
    {
        RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random);
    }
}
