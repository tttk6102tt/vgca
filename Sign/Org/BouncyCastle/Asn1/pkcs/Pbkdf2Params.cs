using Sign.Org.BouncyCastle.Math;

namespace Sign.Org.BouncyCastle.Asn1.pkcs
{
    public class Pbkdf2Params : Asn1Encodable
    {
        private readonly Asn1OctetString octStr;

        private readonly DerInteger iterationCount;

        private readonly DerInteger keyLength;

        public BigInteger IterationCount => iterationCount.Value;

        public BigInteger KeyLength
        {
            get
            {
                if (keyLength != null)
                {
                    return keyLength.Value;
                }

                return null;
            }
        }

        public static Pbkdf2Params GetInstance(object obj)
        {
            if (obj == null || obj is Pbkdf2Params)
            {
                return (Pbkdf2Params)obj;
            }

            if (obj is Asn1Sequence)
            {
                return new Pbkdf2Params((Asn1Sequence)obj);
            }

            throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
        }

        public Pbkdf2Params(Asn1Sequence seq)
        {
            if (seq.Count < 2 || seq.Count > 3)
            {
                throw new ArgumentException("Wrong number of elements in sequence", "seq");
            }

            octStr = (Asn1OctetString)seq[0];
            iterationCount = (DerInteger)seq[1];
            if (seq.Count > 2)
            {
                keyLength = (DerInteger)seq[2];
            }
        }

        public Pbkdf2Params(byte[] salt, int iterationCount)
        {
            octStr = new DerOctetString(salt);
            this.iterationCount = new DerInteger(iterationCount);
        }

        public Pbkdf2Params(byte[] salt, int iterationCount, int keyLength)
            : this(salt, iterationCount)
        {
            this.keyLength = new DerInteger(keyLength);
        }

        public byte[] GetSalt()
        {
            return octStr.GetOctets();
        }

        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(octStr, iterationCount);
            if (keyLength != null)
            {
                asn1EncodableVector.Add(keyLength);
            }

            return new DerSequence(asn1EncodableVector);
        }
    }
}
