namespace Sign.itext.text.pdf
{
    public class SpotColor : ExtendedColor
    {
        private PdfSpotColor spot;

        private float tint;

        public virtual PdfSpotColor PdfSpotColor => spot;

        public virtual float Tint => tint;

        public SpotColor(PdfSpotColor spot, float tint)
            : base(3, ((float)spot.AlternativeCS.R / 255f - 1f) * tint + 1f, ((float)spot.AlternativeCS.G / 255f - 1f) * tint + 1f, ((float)spot.AlternativeCS.B / 255f - 1f) * tint + 1f)
        {
            this.spot = spot;
            this.tint = tint;
        }

        public override bool Equals(object obj)
        {
            if (obj is SpotColor && ((SpotColor)obj).spot.Equals(spot))
            {
                return ((SpotColor)obj).tint == tint;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return spot.GetHashCode() ^ tint.GetHashCode();
        }
    }
}
