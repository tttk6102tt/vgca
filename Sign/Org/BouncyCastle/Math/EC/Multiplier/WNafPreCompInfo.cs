namespace Sign.Org.BouncyCastle.Math.EC.Multiplier
{
    internal class WNafPreCompInfo : PreCompInfo
    {
        private ECPoint[] preComp;

        private ECPoint twiceP;

        internal ECPoint[] GetPreComp()
        {
            return preComp;
        }

        internal void SetPreComp(ECPoint[] preComp)
        {
            this.preComp = preComp;
        }

        internal ECPoint GetTwiceP()
        {
            return twiceP;
        }

        internal void SetTwiceP(ECPoint twiceThis)
        {
            twiceP = twiceThis;
        }
    }
}
