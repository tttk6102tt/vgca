using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Math.EC
{
    public class FpFieldElement : ECFieldElement
    {
        private readonly BigInteger q;

        private readonly BigInteger x;

        public override string FieldName => "Fp";

        public override int FieldSize => q.BitLength;

        public BigInteger Q => q;

        public FpFieldElement(BigInteger q, BigInteger x)
        {
            if (x.CompareTo(q) >= 0)
            {
                throw new ArgumentException("x value too large in field element");
            }

            this.q = q;
            this.x = x;
        }

        public override BigInteger ToBigInteger()
        {
            return x;
        }

        public override ECFieldElement Add(ECFieldElement b)
        {
            return new FpFieldElement(q, x.Add(b.ToBigInteger()).Mod(q));
        }

        public override ECFieldElement Subtract(ECFieldElement b)
        {
            return new FpFieldElement(q, x.Subtract(b.ToBigInteger()).Mod(q));
        }

        public override ECFieldElement Multiply(ECFieldElement b)
        {
            return new FpFieldElement(q, x.Multiply(b.ToBigInteger()).Mod(q));
        }

        public override ECFieldElement Divide(ECFieldElement b)
        {
            return new FpFieldElement(q, x.Multiply(b.ToBigInteger().ModInverse(q)).Mod(q));
        }

        public override ECFieldElement Negate()
        {
            return new FpFieldElement(q, x.Negate().Mod(q));
        }

        public override ECFieldElement Square()
        {
            return new FpFieldElement(q, x.Multiply(x).Mod(q));
        }

        public override ECFieldElement Invert()
        {
            return new FpFieldElement(q, x.ModInverse(q));
        }

        public override ECFieldElement Sqrt()
        {
            if (!q.TestBit(0))
            {
                throw Platform.CreateNotImplementedException("even value of q");
            }

            if (q.TestBit(1))
            {
                ECFieldElement eCFieldElement = new FpFieldElement(q, x.ModPow(q.ShiftRight(2).Add(BigInteger.One), q));
                if (!Equals(eCFieldElement.Square()))
                {
                    return null;
                }

                return eCFieldElement;
            }

            BigInteger bigInteger = q.Subtract(BigInteger.One);
            BigInteger e = bigInteger.ShiftRight(1);
            if (!x.ModPow(e, q).Equals(BigInteger.One))
            {
                return null;
            }

            BigInteger k = bigInteger.ShiftRight(2).ShiftLeft(1).Add(BigInteger.One);
            BigInteger bigInteger2 = x;
            BigInteger bigInteger3 = bigInteger2.ShiftLeft(2).Mod(q);
            BigInteger bigInteger5;
            do
            {
                Random random = new Random();
                BigInteger bigInteger4;
                do
                {
                    bigInteger4 = new BigInteger(q.BitLength, random);
                }
                while (bigInteger4.CompareTo(q) >= 0 || !bigInteger4.Multiply(bigInteger4).Subtract(bigInteger3).ModPow(e, q)
                    .Equals(bigInteger));
                BigInteger[] array = fastLucasSequence(q, bigInteger4, bigInteger2, k);
                bigInteger5 = array[0];
                BigInteger bigInteger6 = array[1];
                if (bigInteger6.Multiply(bigInteger6).Mod(q).Equals(bigInteger3))
                {
                    if (bigInteger6.TestBit(0))
                    {
                        bigInteger6 = bigInteger6.Add(q);
                    }

                    bigInteger6 = bigInteger6.ShiftRight(1);
                    return new FpFieldElement(q, bigInteger6);
                }
            }
            while (bigInteger5.Equals(BigInteger.One) || bigInteger5.Equals(bigInteger));
            return null;
        }

        private static BigInteger[] fastLucasSequence(BigInteger p, BigInteger P, BigInteger Q, BigInteger k)
        {
            int bitLength = k.BitLength;
            int lowestSetBit = k.GetLowestSetBit();
            BigInteger bigInteger = BigInteger.One;
            BigInteger bigInteger2 = BigInteger.Two;
            BigInteger bigInteger3 = P;
            BigInteger bigInteger4 = BigInteger.One;
            BigInteger bigInteger5 = BigInteger.One;
            for (int num = bitLength - 1; num >= lowestSetBit + 1; num--)
            {
                bigInteger4 = bigInteger4.Multiply(bigInteger5).Mod(p);
                if (k.TestBit(num))
                {
                    bigInteger5 = bigInteger4.Multiply(Q).Mod(p);
                    bigInteger = bigInteger.Multiply(bigInteger3).Mod(p);
                    bigInteger2 = bigInteger3.Multiply(bigInteger2).Subtract(P.Multiply(bigInteger4)).Mod(p);
                    bigInteger3 = bigInteger3.Multiply(bigInteger3).Subtract(bigInteger5.ShiftLeft(1)).Mod(p);
                }
                else
                {
                    bigInteger5 = bigInteger4;
                    bigInteger = bigInteger.Multiply(bigInteger2).Subtract(bigInteger4).Mod(p);
                    bigInteger3 = bigInteger3.Multiply(bigInteger2).Subtract(P.Multiply(bigInteger4)).Mod(p);
                    bigInteger2 = bigInteger2.Multiply(bigInteger2).Subtract(bigInteger4.ShiftLeft(1)).Mod(p);
                }
            }

            bigInteger4 = bigInteger4.Multiply(bigInteger5).Mod(p);
            bigInteger5 = bigInteger4.Multiply(Q).Mod(p);
            bigInteger = bigInteger.Multiply(bigInteger2).Subtract(bigInteger4).Mod(p);
            bigInteger2 = bigInteger3.Multiply(bigInteger2).Subtract(P.Multiply(bigInteger4)).Mod(p);
            bigInteger4 = bigInteger4.Multiply(bigInteger5).Mod(p);
            for (int i = 1; i <= lowestSetBit; i++)
            {
                bigInteger = bigInteger.Multiply(bigInteger2).Mod(p);
                bigInteger2 = bigInteger2.Multiply(bigInteger2).Subtract(bigInteger4.ShiftLeft(1)).Mod(p);
                bigInteger4 = bigInteger4.Multiply(bigInteger4).Mod(p);
            }

            return new BigInteger[2] { bigInteger, bigInteger2 };
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            FpFieldElement fpFieldElement = obj as FpFieldElement;
            if (fpFieldElement == null)
            {
                return false;
            }

            return Equals(fpFieldElement);
        }

        protected bool Equals(FpFieldElement other)
        {
            if (q.Equals(other.q))
            {
                return Equals((ECFieldElement)other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return q.GetHashCode() ^ base.GetHashCode();
        }
    }
}
