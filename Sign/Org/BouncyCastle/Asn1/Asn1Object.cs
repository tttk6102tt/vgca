namespace Sign.Org.BouncyCastle.Asn1
{
    public abstract class Asn1Object : Asn1Encodable
    {
        public static Asn1Object FromByteArray(byte[] data)
        {
            try
            {
                return new Asn1InputStream(data).ReadObject();
            }
            catch (InvalidCastException)
            {
                throw new IOException("cannot recognise object in stream");
            }
        }

        public static Asn1Object FromStream(Stream inStr)
        {
            try
            {
                return new Asn1InputStream(inStr).ReadObject();
            }
            catch (InvalidCastException)
            {
                throw new IOException("cannot recognise object in stream");
            }
        }

        public sealed override Asn1Object ToAsn1Object()
        {
            return this;
        }

        internal abstract void Encode(DerOutputStream derOut);

        protected abstract bool Asn1Equals(Asn1Object asn1Object);

        protected abstract int Asn1GetHashCode();

        internal bool CallAsn1Equals(Asn1Object obj)
        {
            return Asn1Equals(obj);
        }

        internal int CallAsn1GetHashCode()
        {
            return Asn1GetHashCode();
        }
    }
}
