namespace Sign.Org.BouncyCastle.Math.EC.Multiplier
{
    internal class WTauNafPreCompInfo : PreCompInfo
    {
        private readonly F2mPoint[] preComp;

        internal WTauNafPreCompInfo(F2mPoint[] preComp)
        {
            this.preComp = preComp;
        }

        internal F2mPoint[] GetPreComp()
        {
            return preComp;
        }
    }
}
