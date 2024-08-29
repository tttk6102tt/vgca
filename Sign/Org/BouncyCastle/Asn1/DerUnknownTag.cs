using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Asn1
{
    public class DerUnknownTag : Asn1Object
    {
        private readonly bool isConstructed;

        private readonly int tag;

        private readonly byte[] data;

        public bool IsConstructed => isConstructed;

        public int Tag => tag;

        public DerUnknownTag(int tag, byte[] data)
            : this(isConstructed: false, tag, data)
        {
        }

        public DerUnknownTag(bool isConstructed, int tag, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this.isConstructed = isConstructed;
            this.tag = tag;
            this.data = data;
        }

        public byte[] GetData()
        {
            return data;
        }

        internal override void Encode(DerOutputStream derOut)
        {
            derOut.WriteEncoded(isConstructed ? 32 : 0, tag, data);
        }

        protected override bool Asn1Equals(Asn1Object asn1Object)
        {
            DerUnknownTag derUnknownTag = asn1Object as DerUnknownTag;
            if (derUnknownTag == null)
            {
                return false;
            }

            if (isConstructed == derUnknownTag.isConstructed && tag == derUnknownTag.tag)
            {
                return Arrays.AreEqual(data, derUnknownTag.data);
            }

            return false;
        }

        protected override int Asn1GetHashCode()
        {
            bool flag = isConstructed;
            int hashCode = flag.GetHashCode();
            int num = tag;
            return hashCode ^ num.GetHashCode() ^ Arrays.GetHashCode(data);
        }
    }
}
