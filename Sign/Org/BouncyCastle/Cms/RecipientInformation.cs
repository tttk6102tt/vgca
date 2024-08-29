using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Cms
{
    public abstract class RecipientInformation
    {
        internal RecipientID rid = new RecipientID();

        internal AlgorithmIdentifier keyEncAlg;

        internal CmsSecureReadable secureReadable;

        private byte[] resultMac;

        public RecipientID RecipientID => rid;

        public AlgorithmIdentifier KeyEncryptionAlgorithmID => keyEncAlg;

        public string KeyEncryptionAlgOid => keyEncAlg.ObjectID.Id;

        public Asn1Object KeyEncryptionAlgParams => keyEncAlg.Parameters?.ToAsn1Object();

        internal RecipientInformation(AlgorithmIdentifier keyEncAlg, CmsSecureReadable secureReadable)
        {
            this.keyEncAlg = keyEncAlg;
            this.secureReadable = secureReadable;
        }

        internal string GetContentAlgorithmName()
        {
            return secureReadable.Algorithm.ObjectID.Id;
        }

        internal CmsTypedStream GetContentFromSessionKey(KeyParameter sKey)
        {
            CmsReadable readable = secureReadable.GetReadable(sKey);
            try
            {
                return new CmsTypedStream(readable.GetInputStream());
            }
            catch (IOException e)
            {
                throw new CmsException("error getting .", e);
            }
        }

        public byte[] GetContent(ICipherParameters key)
        {
            try
            {
                return CmsUtilities.StreamToByteArray(GetContentStream(key).ContentStream);
            }
            catch (IOException ex)
            {
                throw new Exception("unable to parse internal stream: " + ex);
            }
        }

        public byte[] GetMac()
        {
            if (resultMac == null)
            {
                object cryptoObject = secureReadable.CryptoObject;
                if (cryptoObject is IMac)
                {
                    resultMac = MacUtilities.DoFinal((IMac)cryptoObject);
                }
            }

            return Arrays.Clone(resultMac);
        }

        public abstract CmsTypedStream GetContentStream(ICipherParameters key);
    }
}
