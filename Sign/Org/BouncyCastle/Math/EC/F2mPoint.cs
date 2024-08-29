using Sign.Org.BouncyCastle.Math.EC.Multiplier;

namespace Sign.Org.BouncyCastle.Math.EC
{
    public class F2mPoint : ECPointBase
    {
        protected internal override bool YTilde
        {
            get
            {
                if (base.X.ToBigInteger().SignValue != 0)
                {
                    return base.Y.Multiply(base.X.Invert()).ToBigInteger().TestBit(0);
                }

                return false;
            }
        }

        public F2mPoint(ECCurve curve, ECFieldElement x, ECFieldElement y)
            : this(curve, x, y, withCompression: false)
        {
        }

        public F2mPoint(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
            : base(curve, x, y, withCompression)
        {
            if ((x != null && y == null) || (x == null && y != null))
            {
                throw new ArgumentException("Exactly one of the field elements is null");
            }

            if (x != null)
            {
                F2mFieldElement.CheckFieldElements(base.x, base.y);
                F2mFieldElement.CheckFieldElements(base.x, base.curve.A);
            }
        }

        [Obsolete("Use ECCurve.Infinity property")]
        public F2mPoint(ECCurve curve)
            : this(curve, null, null)
        {
        }

        private static void CheckPoints(ECPoint a, ECPoint b)
        {
            if (!a.curve.Equals(b.curve))
            {
                throw new ArgumentException("Only points on the same curve can be added or subtracted");
            }
        }

        public override ECPoint Add(ECPoint b)
        {
            CheckPoints(this, b);
            return AddSimple((F2mPoint)b);
        }

        internal F2mPoint AddSimple(F2mPoint b)
        {
            if (base.IsInfinity)
            {
                return b;
            }

            if (b.IsInfinity)
            {
                return this;
            }

            F2mFieldElement f2mFieldElement = (F2mFieldElement)b.X;
            F2mFieldElement f2mFieldElement2 = (F2mFieldElement)b.Y;
            if (x.Equals(f2mFieldElement))
            {
                if (y.Equals(f2mFieldElement2))
                {
                    return (F2mPoint)Twice();
                }

                return (F2mPoint)curve.Infinity;
            }

            ECFieldElement b2 = x.Add(f2mFieldElement);
            F2mFieldElement f2mFieldElement3 = (F2mFieldElement)y.Add(f2mFieldElement2).Divide(b2);
            F2mFieldElement b3 = (F2mFieldElement)f2mFieldElement3.Square().Add(f2mFieldElement3).Add(b2)
                .Add(curve.A);
            F2mFieldElement f2mFieldElement4 = (F2mFieldElement)f2mFieldElement3.Multiply(x.Add(b3)).Add(b3).Add(y);
            return new F2mPoint(curve, b3, f2mFieldElement4, withCompression);
        }

        public override ECPoint Subtract(ECPoint b)
        {
            CheckPoints(this, b);
            return SubtractSimple((F2mPoint)b);
        }

        internal F2mPoint SubtractSimple(F2mPoint b)
        {
            if (b.IsInfinity)
            {
                return this;
            }

            return AddSimple((F2mPoint)b.Negate());
        }

        public override ECPoint Twice()
        {
            if (base.IsInfinity)
            {
                return this;
            }

            if (x.ToBigInteger().SignValue == 0)
            {
                return curve.Infinity;
            }

            F2mFieldElement f2mFieldElement = (F2mFieldElement)x.Add(y.Divide(x));
            F2mFieldElement f2mFieldElement2 = (F2mFieldElement)f2mFieldElement.Square().Add(f2mFieldElement).Add(curve.A);
            ECFieldElement b = curve.FromBigInteger(BigInteger.One);
            F2mFieldElement f2mFieldElement3 = (F2mFieldElement)x.Square().Add(f2mFieldElement2.Multiply(f2mFieldElement.Add(b)));
            return new F2mPoint(curve, f2mFieldElement2, f2mFieldElement3, withCompression);
        }

        public override ECPoint Negate()
        {
            return new F2mPoint(curve, x, x.Add(y), withCompression);
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
                    if (((F2mCurve)curve).IsKoblitz)
                    {
                        multiplier = new WTauNafMultiplier();
                    }
                    else
                    {
                        multiplier = new WNafMultiplier();
                    }
                }
            }
        }
    }
}
