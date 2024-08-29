namespace Sign.Org.BouncyCastle.Math.EC
{
    public class FpCurve : ECCurve
    {
        private readonly BigInteger q;

        private readonly FpPoint infinity;

        public BigInteger Q => q;

        public override ECPoint Infinity => infinity;

        public override int FieldSize => q.BitLength;

        public FpCurve(BigInteger q, BigInteger a, BigInteger b)
        {
            this.q = q;
            base.a = FromBigInteger(a);
            base.b = FromBigInteger(b);
            infinity = new FpPoint(this, null, null);
        }

        public override ECFieldElement FromBigInteger(BigInteger x)
        {
            return new FpFieldElement(q, x);
        }

        public override ECPoint CreatePoint(BigInteger X1, BigInteger Y1, bool withCompression)
        {
            return new FpPoint(this, FromBigInteger(X1), FromBigInteger(Y1), withCompression);
        }

        protected override ECPoint DecompressPoint(int yTilde, BigInteger X1)
        {
            ECFieldElement eCFieldElement = FromBigInteger(X1);
            ECFieldElement eCFieldElement2 = eCFieldElement.Multiply(eCFieldElement.Square().Add(a)).Add(b).Sqrt();
            if (eCFieldElement2 == null)
            {
                throw new ArithmeticException("Invalid point compression");
            }

            BigInteger bigInteger = eCFieldElement2.ToBigInteger();
            if ((bigInteger.TestBit(0) ? 1 : 0) != yTilde)
            {
                eCFieldElement2 = FromBigInteger(q.Subtract(bigInteger));
            }

            return new FpPoint(this, eCFieldElement, eCFieldElement2, withCompression: true);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            FpCurve fpCurve = obj as FpCurve;
            if (fpCurve == null)
            {
                return false;
            }

            return Equals(fpCurve);
        }

        protected bool Equals(FpCurve other)
        {
            if (Equals((ECCurve)other))
            {
                return q.Equals(other.q);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ q.GetHashCode();
        }
    }
}
