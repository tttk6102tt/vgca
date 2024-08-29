namespace Sign.Org.BouncyCastle.Math.EC
{
    public class ECAlgorithms
    {
        public static ECPoint SumOfTwoMultiplies(ECPoint P, BigInteger a, ECPoint Q, BigInteger b)
        {
            ECCurve curve = P.Curve;
            if (!curve.Equals(Q.Curve))
            {
                throw new ArgumentException("P and Q must be on same curve");
            }

            if (curve is F2mCurve && ((F2mCurve)curve).IsKoblitz)
            {
                return P.Multiply(a).Add(Q.Multiply(b));
            }

            return ImplShamirsTrick(P, a, Q, b);
        }

        public static ECPoint ShamirsTrick(ECPoint P, BigInteger k, ECPoint Q, BigInteger l)
        {
            if (!P.Curve.Equals(Q.Curve))
            {
                throw new ArgumentException("P and Q must be on same curve");
            }

            return ImplShamirsTrick(P, k, Q, l);
        }

        private static ECPoint ImplShamirsTrick(ECPoint P, BigInteger k, ECPoint Q, BigInteger l)
        {
            int num = System.Math.Max(k.BitLength, l.BitLength);
            ECPoint b = P.Add(Q);
            ECPoint eCPoint = P.Curve.Infinity;
            for (int num2 = num - 1; num2 >= 0; num2--)
            {
                eCPoint = eCPoint.Twice();
                if (k.TestBit(num2))
                {
                    eCPoint = ((!l.TestBit(num2)) ? eCPoint.Add(P) : eCPoint.Add(b));
                }
                else if (l.TestBit(num2))
                {
                    eCPoint = eCPoint.Add(Q);
                }
            }

            return eCPoint;
        }
    }
}
