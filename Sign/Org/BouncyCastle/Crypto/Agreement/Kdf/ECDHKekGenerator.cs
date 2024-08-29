using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Crypto.Generators;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Crypto.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Agreement.Kdf
{
    public class ECDHKekGenerator : IDerivationFunction
    {
        private readonly IDerivationFunction kdf;

        private DerObjectIdentifier algorithm;

        private int keySize;

        private byte[] z;

        public virtual IDigest Digest => kdf.Digest;

        public ECDHKekGenerator(IDigest digest)
        {
            kdf = new Kdf2BytesGenerator(digest);
        }

        public virtual void Init(IDerivationParameters param)
        {
            DHKdfParameters dHKdfParameters = (DHKdfParameters)param;
            algorithm = dHKdfParameters.Algorithm;
            keySize = dHKdfParameters.KeySize;
            z = dHKdfParameters.GetZ();
        }

        public virtual int GenerateBytes(byte[] outBytes, int outOff, int len)
        {
            DerSequence derSequence = new DerSequence(new AlgorithmIdentifier(algorithm, DerNull.Instance), new DerTaggedObject(explicitly: true, 2, new DerOctetString(Pack.UInt32_To_BE((uint)keySize))));
            kdf.Init(new KdfParameters(z, derSequence.GetDerEncoded()));
            return kdf.GenerateBytes(outBytes, outOff, len);
        }
    }
}
