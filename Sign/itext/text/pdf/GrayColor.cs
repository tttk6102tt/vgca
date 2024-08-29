namespace Sign.itext.text.pdf
{
    public class GrayColor : ExtendedColor
    {
        private float cgray;

        public static readonly GrayColor GRAYBLACK = new GrayColor(0f);

        public static readonly GrayColor GRAYWHITE = new GrayColor(1f);

        public virtual float Gray => cgray;

        public GrayColor(int intGray)
            : this((float)intGray / 255f)
        {
        }

        public GrayColor(float floatGray)
            : base(1, floatGray, floatGray, floatGray)
        {
            cgray = ExtendedColor.Normalize(floatGray);
        }

        public override bool Equals(object obj)
        {
            if (obj is GrayColor)
            {
                return ((GrayColor)obj).cgray == cgray;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return cgray.GetHashCode();
        }
    }
}
