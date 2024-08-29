namespace Sign.Org.BouncyCastle.Math.EC
{
    public abstract class ECCurve
    {
        internal ECFieldElement a;

        internal ECFieldElement b;

        public abstract int FieldSize { get; }

        public abstract ECPoint Infinity { get; }

        public ECFieldElement A => a;

        public ECFieldElement B => b;

        public abstract ECFieldElement FromBigInteger(BigInteger x);

        public abstract ECPoint CreatePoint(BigInteger x, BigInteger y, bool withCompression);

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            ECCurve eCCurve = obj as ECCurve;
            if (eCCurve == null)
            {
                return false;
            }

            return Equals(eCCurve);
        }

        protected bool Equals(ECCurve other)
        {
            if (a.Equals(other.a))
            {
                return b.Equals(other.b);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ b.GetHashCode();
        }

        protected abstract ECPoint DecompressPoint(int yTilde, BigInteger X1);

        public virtual ECPoint DecodePoint(byte[] encoded)
        {
            ECPoint eCPoint = null;
            int num = (FieldSize + 7) / 8;
            switch (encoded[0])
            {
                case 0:
                    if (encoded.Length != 1)
                    {
                        throw new ArgumentException("Incorrect length for infinity encoding", "encoded");
                    }

                    return Infinity;
                case 2:
                case 3:
                    {
                        if (encoded.Length != num + 1)
                        {
                            throw new ArgumentException("Incorrect length for compressed encoding", "encoded");
                        }

                        int yTilde = encoded[0] & 1;
                        BigInteger x2 = new BigInteger(1, encoded, 1, num);
                        return DecompressPoint(yTilde, x2);
                    }
                case 4:
                case 6:
                case 7:
                    {
                        if (encoded.Length != 2 * num + 1)
                        {
                            throw new ArgumentException("Incorrect length for uncompressed/hybrid encoding", "encoded");
                        }

                        BigInteger x = new BigInteger(1, encoded, 1, num);
                        BigInteger y = new BigInteger(1, encoded, 1 + num, num);
                        return CreatePoint(x, y, withCompression: false);
                    }
                default:
                    throw new FormatException("Invalid point encoding " + encoded[0]);
            }
        }
    }
}
