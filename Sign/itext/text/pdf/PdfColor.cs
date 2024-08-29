namespace Sign.itext.text.pdf
{
    internal class PdfColor : PdfArray
    {
        internal PdfColor(int red, int green, int blue)
            : base(new PdfNumber((double)(red & 0xFF) / 255.0))
        {
            Add(new PdfNumber((double)(green & 0xFF) / 255.0));
            Add(new PdfNumber((double)(blue & 0xFF) / 255.0));
        }

        internal PdfColor(BaseColor color)
            : this(color.R, color.G, color.B)
        {
        }
    }
}
