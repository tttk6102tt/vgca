﻿using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.X509;

namespace Sign.Org.BouncyCastle.Cms
{
    internal class KeyTransRecipientInfoGenerator : RecipientInfoGenerator
    {
        private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

        private TbsCertificateStructure recipientTbsCert;

        private AsymmetricKeyParameter recipientPublicKey;

        private Asn1OctetString subjectKeyIdentifier;

        private SubjectPublicKeyInfo info;

        internal X509Certificate RecipientCert
        {
            set
            {
                recipientTbsCert = CmsUtilities.GetTbsCertificateStructure(value);
                recipientPublicKey = value.GetPublicKey();
                info = recipientTbsCert.SubjectPublicKeyInfo;
            }
        }

        internal AsymmetricKeyParameter RecipientPublicKey
        {
            set
            {
                recipientPublicKey = value;
                try
                {
                    info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(recipientPublicKey);
                }
                catch (IOException)
                {
                    throw new ArgumentException("can't extract key algorithm from this key");
                }
            }
        }

        internal Asn1OctetString SubjectKeyIdentifier
        {
            set
            {
                subjectKeyIdentifier = value;
            }
        }

        internal KeyTransRecipientInfoGenerator()
        {
        }

        public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
        {
            byte[] key = contentEncryptionKey.GetKey();
            AlgorithmIdentifier algorithmID = info.AlgorithmID;
            IWrapper wrapper = Helper.CreateWrapper(algorithmID.ObjectID.Id);
            wrapper.Init(forWrapping: true, new ParametersWithRandom(recipientPublicKey, random));
            byte[] str = wrapper.Wrap(key, 0, key.Length);
            RecipientIdentifier rid = ((recipientTbsCert == null) ? new RecipientIdentifier(subjectKeyIdentifier) : new RecipientIdentifier(new IssuerAndSerialNumber(recipientTbsCert.Issuer, recipientTbsCert.SerialNumber.Value)));
            return new RecipientInfo(new KeyTransRecipientInfo(rid, algorithmID, new DerOctetString(str)));
        }
    }
}
