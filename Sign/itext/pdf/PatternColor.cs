using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PatternColor : ExtendedColor
    {
        private PdfPatternPainter painter;

        public virtual PdfPatternPainter Painter => painter;

        public PatternColor(PdfPatternPainter painter)
            : base(4, 0.5f, 0.5f, 0.5f)
        {
            this.painter = painter;
        }

        public override bool Equals(object obj)
        {
            if (obj is PatternColor)
            {
                return ((PatternColor)obj).painter.Equals(painter);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return painter.GetHashCode();
        }
    }
}
