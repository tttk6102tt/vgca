using Sign.Org.BouncyCastle.Asn1.X509;

namespace Sign.Org.BouncyCastle.Asn1.pkcs
{
    public class EncryptionScheme : AlgorithmIdentifier
    {
        public Asn1Object Asn1Object => base.Parameters.ToAsn1Object();

        public EncryptionScheme(DerObjectIdentifier objectID, Asn1Encodable parameters)
            : base(objectID, parameters)
        {
        }

        internal EncryptionScheme(Asn1Sequence seq)
            : this((DerObjectIdentifier)seq[0], seq[1])
        {
        }

        public new static EncryptionScheme GetInstance(object obj)
        {
            if (obj is EncryptionScheme)
            {
                return (EncryptionScheme)obj;
            }

            if (obj is Asn1Sequence)
            {
                return new EncryptionScheme((Asn1Sequence)obj);
            }

            throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
        }

        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(ObjectID, base.Parameters);
        }
    }
}
