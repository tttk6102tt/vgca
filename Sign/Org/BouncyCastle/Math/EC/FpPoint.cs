using Sign.Org.BouncyCastle.Math.EC.Multiplier;

namespace Sign.Org.BouncyCastle.Math.EC
{
    public class FpPoint : ECPointBase
    {
        protected internal override bool YTilde => base.Y.ToBigInteger().TestBit(0);

        public FpPoint(ECCurve curve, ECFieldElement x, ECFieldElement y)
            : this(curve, x, y, withCompression: false)
        {
        }

        public FpPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
            : base(curve, x, y, withCompression)
        {
            if (x == null != (y == null))
            {
                throw new ArgumentException("Exactly one of the field elements is null");
            }
        }

        public override ECPoint Add(ECPoint b)
        {
            if (base.IsInfinity)
            {
                return b;
            }

            if (b.IsInfinity)
            {
                return this;
            }

            if (x.Equals(b.x))
            {
                if (y.Equals(b.y))
                {
                    return Twice();
                }

                return curve.Infinity;
            }

            ECFieldElement eCFieldElement = b.y.Subtract(y).Divide(b.x.Subtract(x));
            ECFieldElement b2 = eCFieldElement.Square().Subtract(x).Subtract(b.x);
            ECFieldElement eCFieldElement2 = eCFieldElement.Multiply(x.Subtract(b2)).Subtract(y);
            return new FpPoint(curve, b2, eCFieldElement2, withCompression);
        }

        public override ECPoint Twice()
        {
            if (base.IsInfinity)
            {
                return this;
            }

            if (y.ToBigInteger().SignValue == 0)
            {
                return curve.Infinity;
            }

            ECFieldElement b = curve.FromBigInteger(BigInteger.Two);
            ECFieldElement b2 = curve.FromBigInteger(BigInteger.Three);
            ECFieldElement eCFieldElement = x.Square().Multiply(b2).Add(curve.a)
                .Divide(y.Multiply(b));
            ECFieldElement b3 = eCFieldElement.Square().Subtract(x.Multiply(b));
            ECFieldElement eCFieldElement2 = eCFieldElement.Multiply(x.Subtract(b3)).Subtract(y);
            return new FpPoint(curve, b3, eCFieldElement2, withCompression);
        }

        public override ECPoint Subtract(ECPoint b)
        {
            if (b.IsInfinity)
            {
                return this;
            }

            return Add(b.Negate());
        }

        public override ECPoint Negate()
        {
            return new FpPoint(curve, x, y.Negate(), withCompression);
        }

        internal override void AssertECMultiplier()
        {
            if (multiplier != null)
            {
                return;
            }

            lock (this)
            {
                if (multiplier == null)
                {
                    multiplier = new WNafMultiplier();
                }
            }
        }
    }
}
