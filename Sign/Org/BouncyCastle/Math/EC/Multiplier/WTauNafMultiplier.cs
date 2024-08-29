using Sign.Org.BouncyCastle.Math.EC.Abc;

namespace Sign.Org.BouncyCastle.Math.EC.Multiplier
{
    internal class WTauNafMultiplier : ECMultiplier
    {
        public ECPoint Multiply(ECPoint point, BigInteger k, PreCompInfo preCompInfo)
        {
            if (!(point is F2mPoint))
            {
                throw new ArgumentException("Only F2mPoint can be used in WTauNafMultiplier");
            }

            F2mPoint f2mPoint = (F2mPoint)point;
            F2mCurve obj = (F2mCurve)f2mPoint.Curve;
            int m = obj.M;
            sbyte a = (sbyte)obj.A.ToBigInteger().IntValue;
            sbyte mu = obj.GetMu();
            BigInteger[] si = obj.GetSi();
            ZTauElement lambda = Tnaf.PartModReduction(k, m, a, si, mu, 10);
            return MultiplyWTnaf(f2mPoint, lambda, preCompInfo, a, mu);
        }

        private F2mPoint MultiplyWTnaf(F2mPoint p, ZTauElement lambda, PreCompInfo preCompInfo, sbyte a, sbyte mu)
        {
            ZTauElement[] alpha = ((a != 0) ? Tnaf.Alpha1 : Tnaf.Alpha0);
            BigInteger tw = Tnaf.GetTw(mu, 4);
            sbyte[] u = Tnaf.TauAdicWNaf(mu, lambda, 4, BigInteger.ValueOf(16L), tw, alpha);
            return MultiplyFromWTnaf(p, u, preCompInfo);
        }

        private static F2mPoint MultiplyFromWTnaf(F2mPoint p, sbyte[] u, PreCompInfo preCompInfo)
        {
            sbyte a = (sbyte)((F2mCurve)p.Curve).A.ToBigInteger().IntValue;
            F2mPoint[] preComp;
            if (preCompInfo == null || !(preCompInfo is WTauNafPreCompInfo))
            {
                preComp = Tnaf.GetPreComp(p, a);
                p.SetPreCompInfo(new WTauNafPreCompInfo(preComp));
            }
            else
            {
                preComp = ((WTauNafPreCompInfo)preCompInfo).GetPreComp();
            }

            F2mPoint f2mPoint = (F2mPoint)p.Curve.Infinity;
            for (int num = u.Length - 1; num >= 0; num--)
            {
                f2mPoint = Tnaf.Tau(f2mPoint);
                if (u[num] != 0)
                {
                    f2mPoint = ((u[num] <= 0) ? f2mPoint.SubtractSimple(preComp[-u[num]]) : f2mPoint.AddSimple(preComp[u[num]]));
                }
            }

            return f2mPoint;
        }
    }
}
