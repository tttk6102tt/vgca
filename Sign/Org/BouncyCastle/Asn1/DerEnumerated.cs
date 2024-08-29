using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Asn1
{
    public class DerEnumerated : Asn1Object
    {
        private readonly byte[] bytes;

        public BigInteger Value => new BigInteger(bytes);

        public static DerEnumerated GetInstance(object obj)
        {
            if (obj == null || obj is DerEnumerated)
            {
                return (DerEnumerated)obj;
            }

            throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
        }

        public static DerEnumerated GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            Asn1Object @object = obj.GetObject();
            if (isExplicit || @object is DerEnumerated)
            {
                return GetInstance(@object);
            }

            return new DerEnumerated(((Asn1OctetString)@object).GetOctets());
        }

        public DerEnumerated(int val)
        {
            bytes = BigInteger.ValueOf(val).ToByteArray();
        }

        public DerEnumerated(BigInteger val)
        {
            bytes = val.ToByteArray();
        }

        public DerEnumerated(byte[] bytes)
        {
            this.bytes = bytes;
        }

        internal override void Encode(DerOutputStream derOut)
        {
            derOut.WriteEncoded(10, bytes);
        }

        protected override bool Asn1Equals(Asn1Object asn1Object)
        {
            DerEnumerated derEnumerated = asn1Object as DerEnumerated;
            if (derEnumerated == null)
            {
                return false;
            }

            return Arrays.AreEqual(bytes, derEnumerated.bytes);
        }

        protected override int Asn1GetHashCode()
        {
            return Arrays.GetHashCode(bytes);
        }
    }
}
