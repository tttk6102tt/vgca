using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Cms
{
    public class KekRecipientInformation : RecipientInformation
    {
        private KekRecipientInfo info;

        internal KekRecipientInformation(KekRecipientInfo info, CmsSecureReadable secureReadable)
            : base(info.KeyEncryptionAlgorithm, secureReadable)
        {
            this.info = info;
            rid = new RecipientID();
            KekIdentifier kekID = info.KekID;
            rid.KeyIdentifier = kekID.KeyIdentifier.GetOctets();
        }

        public override CmsTypedStream GetContentStream(ICipherParameters key)
        {
            try
            {
                byte[] octets = info.EncryptedKey.GetOctets();
                IWrapper wrapper = WrapperUtilities.GetWrapper(keyEncAlg.ObjectID.Id);
                wrapper.Init(forWrapping: false, key);
                KeyParameter sKey = ParameterUtilities.CreateKeyParameter(GetContentAlgorithmName(), wrapper.Unwrap(octets, 0, octets.Length));
                return GetContentFromSessionKey(sKey);
            }
            catch (SecurityUtilityException e)
            {
                throw new CmsException("couldn't create cipher.", e);
            }
            catch (InvalidKeyException e2)
            {
                throw new CmsException("key invalid in message.", e2);
            }
        }
    }
}
