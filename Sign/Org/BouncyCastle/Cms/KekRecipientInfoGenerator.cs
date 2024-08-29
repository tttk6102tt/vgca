using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Asn1.Kisa;
using Sign.Org.BouncyCastle.Asn1.Nist;
using Sign.Org.BouncyCastle.Asn1.Ntt;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Cms
{
    internal class KekRecipientInfoGenerator : RecipientInfoGenerator
    {
        private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

        private KeyParameter keyEncryptionKey;

        private string keyEncryptionKeyOID;

        private KekIdentifier kekIdentifier;

        private AlgorithmIdentifier keyEncryptionAlgorithm;

        internal KekIdentifier KekIdentifier
        {
            set
            {
                kekIdentifier = value;
            }
        }

        internal KeyParameter KeyEncryptionKey
        {
            set
            {
                keyEncryptionKey = value;
                keyEncryptionAlgorithm = DetermineKeyEncAlg(keyEncryptionKeyOID, keyEncryptionKey);
            }
        }

        internal string KeyEncryptionKeyOID
        {
            set
            {
                keyEncryptionKeyOID = value;
            }
        }

        internal KekRecipientInfoGenerator()
        {
        }

        public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
        {
            byte[] key = contentEncryptionKey.GetKey();
            IWrapper wrapper = Helper.CreateWrapper(keyEncryptionAlgorithm.ObjectID.Id);
            wrapper.Init(forWrapping: true, new ParametersWithRandom(keyEncryptionKey, random));
            Asn1OctetString encryptedKey = new DerOctetString(wrapper.Wrap(key, 0, key.Length));
            return new RecipientInfo(new KekRecipientInfo(kekIdentifier, keyEncryptionAlgorithm, encryptedKey));
        }

        private static AlgorithmIdentifier DetermineKeyEncAlg(string algorithm, KeyParameter key)
        {
            if (algorithm.StartsWith("DES"))
            {
                return new AlgorithmIdentifier(PkcsObjectIdentifiers.IdAlgCms3DesWrap, DerNull.Instance);
            }

            if (algorithm.StartsWith("RC2"))
            {
                return new AlgorithmIdentifier(PkcsObjectIdentifiers.IdAlgCmsRC2Wrap, new DerInteger(58));
            }

            if (algorithm.StartsWith("AES"))
            {
                return new AlgorithmIdentifier((key.GetKey().Length * 8) switch
                {
                    128 => NistObjectIdentifiers.IdAes128Wrap,
                    192 => NistObjectIdentifiers.IdAes192Wrap,
                    256 => NistObjectIdentifiers.IdAes256Wrap,
                    _ => throw new ArgumentException("illegal keysize in AES"),
                });
            }

            if (algorithm.StartsWith("SEED"))
            {
                return new AlgorithmIdentifier(KisaObjectIdentifiers.IdNpkiAppCmsSeedWrap);
            }

            if (algorithm.StartsWith("CAMELLIA"))
            {
                return new AlgorithmIdentifier((key.GetKey().Length * 8) switch
                {
                    128 => NttObjectIdentifiers.IdCamellia128Wrap,
                    192 => NttObjectIdentifiers.IdCamellia192Wrap,
                    256 => NttObjectIdentifiers.IdCamellia256Wrap,
                    _ => throw new ArgumentException("illegal keysize in Camellia"),
                });
            }

            throw new ArgumentException("unknown algorithm");
        }
    }
}
