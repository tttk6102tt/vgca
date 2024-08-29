namespace Sign.itext.text.pdf
{
    public class ShadingColor : ExtendedColor
    {
        private PdfShadingPattern shadingPattern;

        public virtual PdfShadingPattern PdfShadingPattern => shadingPattern;

        public ShadingColor(PdfShadingPattern shadingPattern)
            : base(5, 0.5f, 0.5f, 0.5f)
        {
            this.shadingPattern = shadingPattern;
        }

        public override bool Equals(object obj)
        {
            if (obj is ShadingColor)
            {
                return ((ShadingColor)obj).shadingPattern.Equals(shadingPattern);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return shadingPattern.GetHashCode();
        }
    }
}
