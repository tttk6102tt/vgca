using Sign.Org.BouncyCastle.Math.EC.Multiplier;

namespace Sign.Org.BouncyCastle.Math.EC
{
    public abstract class ECPoint
    {
        internal readonly ECCurve curve;

        internal readonly ECFieldElement x;

        internal readonly ECFieldElement y;

        internal readonly bool withCompression;

        internal ECMultiplier multiplier;

        internal PreCompInfo preCompInfo;

        public ECCurve Curve => curve;

        public ECFieldElement X => x;

        public ECFieldElement Y => y;

        public bool IsInfinity
        {
            get
            {
                if (x == null)
                {
                    return y == null;
                }

                return false;
            }
        }

        public bool IsCompressed => withCompression;

        protected internal ECPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            this.curve = curve;
            this.x = x;
            this.y = y;
            this.withCompression = withCompression;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            ECPoint eCPoint = obj as ECPoint;
            if (eCPoint == null)
            {
                return false;
            }

            if (IsInfinity)
            {
                return eCPoint.IsInfinity;
            }

            if (x.Equals(eCPoint.x))
            {
                return y.Equals(eCPoint.y);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (IsInfinity)
            {
                return 0;
            }

            return x.GetHashCode() ^ y.GetHashCode();
        }

        internal void SetPreCompInfo(PreCompInfo preCompInfo)
        {
            this.preCompInfo = preCompInfo;
        }

        public virtual byte[] GetEncoded()
        {
            return GetEncoded(withCompression);
        }

        public abstract byte[] GetEncoded(bool compressed);

        public abstract ECPoint Add(ECPoint b);

        public abstract ECPoint Subtract(ECPoint b);

        public abstract ECPoint Negate();

        public abstract ECPoint Twice();

        public abstract ECPoint Multiply(BigInteger b);

        internal virtual void AssertECMultiplier()
        {
            if (multiplier != null)
            {
                return;
            }

            lock (this)
            {
                if (multiplier == null)
                {
                    multiplier = new FpNafMultiplier();
                }
            }
        }
    }
}
