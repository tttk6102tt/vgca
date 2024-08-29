using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Signers
{
    public class Gost3410DigestSigner : ISigner
    {
        private readonly IDigest digest;

        private readonly IDsa dsaSigner;

        private bool forSigning;

        public string AlgorithmName => digest.AlgorithmName + "with" + dsaSigner.AlgorithmName;

        public Gost3410DigestSigner(IDsa signer, IDigest digest)
        {
            dsaSigner = signer;
            this.digest = digest;
        }

        public void Init(bool forSigning, ICipherParameters parameters)
        {
            this.forSigning = forSigning;
            AsymmetricKeyParameter asymmetricKeyParameter = ((!(parameters is ParametersWithRandom)) ? ((AsymmetricKeyParameter)parameters) : ((AsymmetricKeyParameter)((ParametersWithRandom)parameters).Parameters));
            if (forSigning && !asymmetricKeyParameter.IsPrivate)
            {
                throw new InvalidKeyException("Signing Requires Private Key.");
            }

            if (!forSigning && asymmetricKeyParameter.IsPrivate)
            {
                throw new InvalidKeyException("Verification Requires Public Key.");
            }

            Reset();
            dsaSigner.Init(forSigning, parameters);
        }

        public void Update(byte input)
        {
            digest.Update(input);
        }

        public void BlockUpdate(byte[] input, int inOff, int length)
        {
            digest.BlockUpdate(input, inOff, length);
        }

        public byte[] GenerateSignature()
        {
            if (!forSigning)
            {
                throw new InvalidOperationException("GOST3410DigestSigner not initialised for signature generation.");
            }

            byte[] array = new byte[digest.GetDigestSize()];
            digest.DoFinal(array, 0);
            try
            {
                BigInteger[] array2 = dsaSigner.GenerateSignature(array);
                byte[] array3 = new byte[64];
                byte[] array4 = array2[0].ToByteArrayUnsigned();
                byte[] array5 = array2[1].ToByteArrayUnsigned();
                array5.CopyTo(array3, 32 - array5.Length);
                array4.CopyTo(array3, 64 - array4.Length);
                return array3;
            }
            catch (Exception ex)
            {
                throw new SignatureException(ex.Message, ex);
            }
        }

        public bool VerifySignature(byte[] signature)
        {
            if (forSigning)
            {
                throw new InvalidOperationException("DSADigestSigner not initialised for verification");
            }

            byte[] array = new byte[digest.GetDigestSize()];
            digest.DoFinal(array, 0);
            BigInteger r;
            BigInteger s;
            try
            {
                r = new BigInteger(1, signature, 32, 32);
                s = new BigInteger(1, signature, 0, 32);
            }
            catch (Exception exception)
            {
                throw new SignatureException("error decoding signature bytes.", exception);
            }

            return dsaSigner.VerifySignature(array, r, s);
        }

        public void Reset()
        {
            digest.Reset();
        }
    }
}
