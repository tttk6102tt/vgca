using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class LabColor : ExtendedColor
    {
        private PdfLabColor labColorSpace;

        private float l;

        private float a;

        private float b;

        public virtual PdfLabColor LabColorSpace => labColorSpace;

        public virtual float L => l;

        public new virtual float A => a;

        public new virtual float B => b;

        public LabColor(PdfLabColor labColorSpace, float l, float a, float b)
            : base(7)
        {
            this.labColorSpace = labColorSpace;
            this.l = l;
            this.a = a;
            this.b = b;
            BaseColor baseColor = labColorSpace.Lab2Rgb(l, a, b);
            SetValue(baseColor.R, baseColor.G, baseColor.B, 255);
        }

        public virtual BaseColor ToRgb()
        {
            return labColorSpace.Lab2Rgb(l, a, b);
        }

        internal virtual CMYKColor ToCmyk()
        {
            return labColorSpace.Lab2Cmyk(l, a, b);
        }
    }
}
