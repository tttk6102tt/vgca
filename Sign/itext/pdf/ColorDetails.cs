using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class ColorDetails
    {
        private PdfIndirectReference indirectReference;

        private PdfName colorSpaceName;

        private ICachedColorSpace colorSpace;

        public virtual PdfIndirectReference IndirectReference => indirectReference;

        internal virtual PdfName ColorSpaceName => colorSpaceName;

        internal ColorDetails(PdfName colorName, PdfIndirectReference indirectReference, ICachedColorSpace scolor)
        {
            colorSpaceName = colorName;
            this.indirectReference = indirectReference;
            colorSpace = scolor;
        }

        public virtual PdfObject GetPdfObject(PdfWriter writer)
        {
            return colorSpace.GetPdfObject(writer);
        }
    }
}
