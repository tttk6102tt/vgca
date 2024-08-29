using Sign.Org.BouncyCastle.Asn1.X9;

namespace Sign.Org.BouncyCastle.Math.EC
{
    public abstract class ECPointBase : ECPoint
    {
        protected internal abstract bool YTilde { get; }

        protected internal ECPointBase(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
            : base(curve, x, y, withCompression)
        {
        }

        public override byte[] GetEncoded(bool compressed)
        {
            if (base.IsInfinity)
            {
                return new byte[1];
            }

            int byteLength = X9IntegerConverter.GetByteLength(x);
            byte[] array = X9IntegerConverter.IntegerToBytes(base.X.ToBigInteger(), byteLength);
            byte[] array2;
            if (compressed)
            {
                array2 = new byte[1 + array.Length];
                array2[0] = (byte)(YTilde ? 3u : 2u);
            }
            else
            {
                byte[] array3 = X9IntegerConverter.IntegerToBytes(base.Y.ToBigInteger(), byteLength);
                array2 = new byte[1 + array.Length + array3.Length];
                array2[0] = 4;
                array3.CopyTo(array2, 1 + array.Length);
            }

            array.CopyTo(array2, 1);
            return array2;
        }

        public override ECPoint Multiply(BigInteger k)
        {
            if (k.SignValue < 0)
            {
                throw new ArgumentException("The multiplicator cannot be negative", "k");
            }

            if (base.IsInfinity)
            {
                return this;
            }

            if (k.SignValue == 0)
            {
                return curve.Infinity;
            }

            AssertECMultiplier();

            return multiplier.Multiply(this, k, preCompInfo);
        }
    }
}
