using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Cms
{
    internal class PasswordRecipientInfoGenerator : RecipientInfoGenerator
    {
        private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

        private AlgorithmIdentifier keyDerivationAlgorithm;

        private KeyParameter keyEncryptionKey;

        private string keyEncryptionKeyOID;

        internal AlgorithmIdentifier KeyDerivationAlgorithm
        {
            set
            {
                keyDerivationAlgorithm = value;
            }
        }

        internal KeyParameter KeyEncryptionKey
        {
            set
            {
                keyEncryptionKey = value;
            }
        }

        internal string KeyEncryptionKeyOID
        {
            set
            {
                keyEncryptionKeyOID = value;
            }
        }

        internal PasswordRecipientInfoGenerator()
        {
        }

        public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
        {
            byte[] key = contentEncryptionKey.GetKey();
            string rfc3211WrapperName = Helper.GetRfc3211WrapperName(keyEncryptionKeyOID);
            IWrapper wrapper = Helper.CreateWrapper(rfc3211WrapperName);
            byte[] array = new byte[rfc3211WrapperName.StartsWith("DESEDE") ? 8 : 16];
            random.NextBytes(array);
            ICipherParameters parameters = new ParametersWithIV(keyEncryptionKey, array);
            wrapper.Init(forWrapping: true, new ParametersWithRandom(parameters, random));
            Asn1OctetString encryptedKey = new DerOctetString(wrapper.Wrap(key, 0, key.Length));
            DerSequence parameters2 = new DerSequence(new DerObjectIdentifier(keyEncryptionKeyOID), new DerOctetString(array));
            AlgorithmIdentifier keyEncryptionAlgorithm = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdAlgPwriKek, parameters2);
            return new RecipientInfo(new PasswordRecipientInfo(keyDerivationAlgorithm, keyEncryptionAlgorithm, encryptedKey));
        }
    }
}
