using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class CMYKColor : ExtendedColor
    {
        private float ccyan;

        private float cmagenta;

        private float cyellow;

        private float cblack;

        public virtual float Cyan => ccyan;

        public virtual float Magenta => cmagenta;

        public virtual float Yellow => cyellow;

        public virtual float Black => cblack;

        public CMYKColor(int intCyan, int intMagenta, int intYellow, int intBlack)
            : this((float)intCyan / 255f, (float)intMagenta / 255f, (float)intYellow / 255f, (float)intBlack / 255f)
        {
        }

        public CMYKColor(float floatCyan, float floatMagenta, float floatYellow, float floatBlack)
            : base(2, 1f - floatCyan - floatBlack, 1f - floatMagenta - floatBlack, 1f - floatYellow - floatBlack)
        {
            ccyan = ExtendedColor.Normalize(floatCyan);
            cmagenta = ExtendedColor.Normalize(floatMagenta);
            cyellow = ExtendedColor.Normalize(floatYellow);
            cblack = ExtendedColor.Normalize(floatBlack);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CMYKColor))
            {
                return false;
            }

            CMYKColor cMYKColor = (CMYKColor)obj;
            if (ccyan == cMYKColor.ccyan && cmagenta == cMYKColor.cmagenta && cyellow == cMYKColor.cyellow)
            {
                return cblack == cMYKColor.cblack;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ccyan.GetHashCode() ^ cmagenta.GetHashCode() ^ cyellow.GetHashCode() ^ cblack.GetHashCode();
        }
    }
}
