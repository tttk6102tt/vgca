using Sign.Org.BouncyCastle.Asn1.X509;
using Sign.Org.BouncyCastle.Math;
using System.Collections;

namespace Sign.Org.BouncyCastle.Asn1.pkcs
{
    public class PrivateKeyInfo : Asn1Encodable
    {
        private readonly Asn1Object privKey;

        private readonly AlgorithmIdentifier algID;

        private readonly Asn1Set attributes;

        public AlgorithmIdentifier AlgorithmID => algID;

        public Asn1Object PrivateKey => privKey;

        public Asn1Set Attributes => attributes;

        public static PrivateKeyInfo GetInstance(object obj)
        {
            if (obj is PrivateKeyInfo)
            {
                return (PrivateKeyInfo)obj;
            }

            if (obj != null)
            {
                return new PrivateKeyInfo(Asn1Sequence.GetInstance(obj));
            }

            return null;
        }

        public PrivateKeyInfo(AlgorithmIdentifier algID, Asn1Object privateKey)
            : this(algID, privateKey, null)
        {
        }

        public PrivateKeyInfo(AlgorithmIdentifier algID, Asn1Object privateKey, Asn1Set attributes)
        {
            privKey = privateKey;
            this.algID = algID;
            this.attributes = attributes;
        }

        private PrivateKeyInfo(Asn1Sequence seq)
        {
            IEnumerator enumerator = seq.GetEnumerator();
            enumerator.MoveNext();
            BigInteger value = ((DerInteger)enumerator.Current).Value;
            if (value.IntValue != 0)
            {
                throw new ArgumentException("wrong version for private key info: " + value.IntValue);
            }

            enumerator.MoveNext();
            algID = AlgorithmIdentifier.GetInstance(enumerator.Current);
            try
            {
                enumerator.MoveNext();
                Asn1OctetString asn1OctetString = (Asn1OctetString)enumerator.Current;
                privKey = Asn1Object.FromByteArray(asn1OctetString.GetOctets());
            }
            catch (IOException)
            {
                throw new ArgumentException("Error recoverying private key from sequence");
            }

            if (enumerator.MoveNext())
            {
                attributes = Asn1Set.GetInstance((Asn1TaggedObject)enumerator.Current, explicitly: false);
            }
        }

        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new DerInteger(0), algID, new DerOctetString(privKey));
            if (attributes != null)
            {
                asn1EncodableVector.Add(new DerTaggedObject(explicitly: false, 0, attributes));
            }

            return new DerSequence(asn1EncodableVector);
        }
    }
}
