using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Cms
{
    public class KeyTransRecipientInformation : RecipientInformation
    {
        private KeyTransRecipientInfo info;

        internal KeyTransRecipientInformation(KeyTransRecipientInfo info, CmsSecureReadable secureReadable)
            : base(info.KeyEncryptionAlgorithm, secureReadable)
        {
            this.info = info;
            rid = new RecipientID();
            RecipientIdentifier recipientIdentifier = info.RecipientIdentifier;
            try
            {
                if (recipientIdentifier.IsTagged)
                {
                    Asn1OctetString instance = Asn1OctetString.GetInstance(recipientIdentifier.ID);
                    rid.SubjectKeyIdentifier = instance.GetOctets();
                }
                else
                {
                    IssuerAndSerialNumber instance2 = IssuerAndSerialNumber.GetInstance(recipientIdentifier.ID);
                    rid.Issuer = instance2.Name;
                    rid.SerialNumber = instance2.SerialNumber.Value;
                }
            }
            catch (IOException)
            {
                throw new ArgumentException("invalid rid in KeyTransRecipientInformation");
            }
        }

        private string GetExchangeEncryptionAlgorithmName(DerObjectIdentifier oid)
        {
            if (PkcsObjectIdentifiers.RsaEncryption.Equals(oid))
            {
                return "RSA//PKCS1Padding";
            }

            return oid.Id;
        }

        internal KeyParameter UnwrapKey(ICipherParameters key)
        {
            byte[] octets = info.EncryptedKey.GetOctets();
            string exchangeEncryptionAlgorithmName = GetExchangeEncryptionAlgorithmName(keyEncAlg.ObjectID);
            try
            {
                IWrapper wrapper = WrapperUtilities.GetWrapper(exchangeEncryptionAlgorithmName);
                wrapper.Init(forWrapping: false, key);
                return ParameterUtilities.CreateKeyParameter(GetContentAlgorithmName(), wrapper.Unwrap(octets, 0, octets.Length));
            }
            catch (SecurityUtilityException e)
            {
                throw new CmsException("couldn't create cipher.", e);
            }
            catch (InvalidKeyException e2)
            {
                throw new CmsException("key invalid in message.", e2);
            }
            catch (DataLengthException e3)
            {
                throw new CmsException("illegal blocksize in message.", e3);
            }
            catch (InvalidCipherTextException e4)
            {
                throw new CmsException("bad padding in message.", e4);
            }
        }

        public override CmsTypedStream GetContentStream(ICipherParameters key)
        {
            KeyParameter sKey = UnwrapKey(key);
            return GetContentFromSessionKey(sKey);
        }
    }
}
