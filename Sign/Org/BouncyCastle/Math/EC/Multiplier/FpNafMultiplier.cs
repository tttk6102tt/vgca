namespace Sign.Org.BouncyCastle.Math.EC.Multiplier
{
    internal class FpNafMultiplier : ECMultiplier
    {
        public ECPoint Multiply(ECPoint p, BigInteger k, PreCompInfo preCompInfo)
        {
            BigInteger bigInteger = k.Multiply(BigInteger.Three);
            ECPoint eCPoint = p.Negate();
            ECPoint eCPoint2 = p;
            for (int num = bigInteger.BitLength - 2; num > 0; num--)
            {
                eCPoint2 = eCPoint2.Twice();
                bool flag = bigInteger.TestBit(num);
                bool flag2 = k.TestBit(num);
                if (flag != flag2)
                {
                    eCPoint2 = eCPoint2.Add(flag ? p : eCPoint);
                }
            }

            return eCPoint2;
        }
    }
}
