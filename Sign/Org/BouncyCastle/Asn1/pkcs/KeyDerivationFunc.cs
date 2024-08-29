using Sign.Org.BouncyCastle.Asn1.X509;

namespace Sign.Org.BouncyCastle.Asn1.pkcs
{
    public class KeyDerivationFunc : AlgorithmIdentifier
    {
        internal KeyDerivationFunc(Asn1Sequence seq)
            : base(seq)
        {
        }

        public KeyDerivationFunc(DerObjectIdentifier id, Asn1Encodable parameters)
            : base(id, parameters)
        {
        }
    }
}
