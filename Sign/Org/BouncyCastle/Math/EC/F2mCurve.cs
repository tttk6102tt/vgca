using Sign.Org.BouncyCastle.Math.EC.Abc;

namespace Sign.Org.BouncyCastle.Math.EC
{
    public class F2mCurve : ECCurve
    {
        private readonly int m;

        private readonly int k1;

        private readonly int k2;

        private readonly int k3;

        private readonly BigInteger n;

        private readonly BigInteger h;

        private readonly F2mPoint infinity;

        private sbyte mu;

        private BigInteger[] si;

        public override ECPoint Infinity => infinity;

        public override int FieldSize => m;

        public bool IsKoblitz
        {
            get
            {
                if (n != null && h != null && (a.ToBigInteger().Equals(BigInteger.Zero) || a.ToBigInteger().Equals(BigInteger.One)))
                {
                    return b.ToBigInteger().Equals(BigInteger.One);
                }

                return false;
            }
        }

        public int M => m;

        public int K1 => k1;

        public int K2 => k2;

        public int K3 => k3;

        public BigInteger N => n;

        public BigInteger H => h;

        public F2mCurve(int m, int k, BigInteger a, BigInteger b)
            : this(m, k, 0, 0, a, b, null, null)
        {
        }

        public F2mCurve(int m, int k, BigInteger a, BigInteger b, BigInteger n, BigInteger h)
            : this(m, k, 0, 0, a, b, n, h)
        {
        }

        public F2mCurve(int m, int k1, int k2, int k3, BigInteger a, BigInteger b)
            : this(m, k1, k2, k3, a, b, null, null)
        {
        }

        public F2mCurve(int m, int k1, int k2, int k3, BigInteger a, BigInteger b, BigInteger n, BigInteger h)
        {
            this.m = m;
            this.k1 = k1;
            this.k2 = k2;
            this.k3 = k3;
            this.n = n;
            this.h = h;
            infinity = new F2mPoint(this, null, null);
            if (k1 == 0)
            {
                throw new ArgumentException("k1 must be > 0");
            }

            if (k2 == 0)
            {
                if (k3 != 0)
                {
                    throw new ArgumentException("k3 must be 0 if k2 == 0");
                }
            }
            else
            {
                if (k2 <= k1)
                {
                    throw new ArgumentException("k2 must be > k1");
                }

                if (k3 <= k2)
                {
                    throw new ArgumentException("k3 must be > k2");
                }
            }

            base.a = FromBigInteger(a);
            base.b = FromBigInteger(b);
        }

        public override ECFieldElement FromBigInteger(BigInteger x)
        {
            return new F2mFieldElement(m, k1, k2, k3, x);
        }

        internal sbyte GetMu()
        {
            if (mu == 0)
            {
                lock (this)
                {
                    if (mu == 0)
                    {
                        mu = Tnaf.GetMu(this);
                    }
                }
            }

            return mu;
        }

        internal BigInteger[] GetSi()
        {
            if (si == null)
            {
                lock (this)
                {
                    if (si == null)
                    {
                        si = Tnaf.GetSi(this);
                    }
                }
            }

            return si;
        }

        public override ECPoint CreatePoint(BigInteger X1, BigInteger Y1, bool withCompression)
        {
            return new F2mPoint(this, FromBigInteger(X1), FromBigInteger(Y1), withCompression);
        }

        protected override ECPoint DecompressPoint(int yTilde, BigInteger X1)
        {
            ECFieldElement eCFieldElement = FromBigInteger(X1);
            ECFieldElement eCFieldElement2 = null;
            if (eCFieldElement.ToBigInteger().SignValue == 0)
            {
                eCFieldElement2 = (F2mFieldElement)b;
                for (int i = 0; i < m - 1; i++)
                {
                    eCFieldElement2 = eCFieldElement2.Square();
                }
            }
            else
            {
                ECFieldElement beta = eCFieldElement.Add(a).Add(b.Multiply(eCFieldElement.Square().Invert()));
                ECFieldElement eCFieldElement3 = solveQuadradicEquation(beta);
                if (eCFieldElement3 == null)
                {
                    throw new ArithmeticException("Invalid point compression");
                }

                if ((eCFieldElement3.ToBigInteger().TestBit(0) ? 1 : 0) != yTilde)
                {
                    eCFieldElement3 = eCFieldElement3.Add(FromBigInteger(BigInteger.One));
                }

                eCFieldElement2 = eCFieldElement.Multiply(eCFieldElement3);
            }

            return new F2mPoint(this, eCFieldElement, eCFieldElement2, withCompression: true);
        }

        private ECFieldElement solveQuadradicEquation(ECFieldElement beta)
        {
            if (beta.ToBigInteger().SignValue == 0)
            {
                return FromBigInteger(BigInteger.Zero);
            }

            ECFieldElement eCFieldElement = null;
            ECFieldElement eCFieldElement2 = FromBigInteger(BigInteger.Zero);
            while (eCFieldElement2.ToBigInteger().SignValue == 0)
            {
                ECFieldElement eCFieldElement3 = FromBigInteger(new BigInteger(m, new Random()));
                eCFieldElement = FromBigInteger(BigInteger.Zero);
                ECFieldElement eCFieldElement4 = beta;
                for (int i = 1; i <= m - 1; i++)
                {
                    ECFieldElement eCFieldElement5 = eCFieldElement4.Square();
                    eCFieldElement = eCFieldElement.Square().Add(eCFieldElement5.Multiply(eCFieldElement3));
                    eCFieldElement4 = eCFieldElement5.Add(beta);
                }

                if (eCFieldElement4.ToBigInteger().SignValue != 0)
                {
                    return null;
                }

                eCFieldElement2 = eCFieldElement.Square().Add(eCFieldElement);
            }

            return eCFieldElement;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            F2mCurve f2mCurve = obj as F2mCurve;
            if (f2mCurve == null)
            {
                return false;
            }

            return Equals(f2mCurve);
        }

        protected bool Equals(F2mCurve other)
        {
            if (m == other.m && k1 == other.k1 && k2 == other.k2 && k3 == other.k3)
            {
                return Equals((ECCurve)other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ m ^ k1 ^ k2 ^ k3;
        }

        public bool IsTrinomial()
        {
            if (k2 == 0)
            {
                return k3 == 0;
            }

            return false;
        }
    }
}
