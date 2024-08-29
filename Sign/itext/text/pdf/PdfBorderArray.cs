namespace Sign.itext.text.pdf
{
    public class PdfBorderArray : PdfArray
    {
        public PdfBorderArray(float hRadius, float vRadius, float width)
            : this(hRadius, vRadius, width, null)
        {
        }

        public PdfBorderArray(float hRadius, float vRadius, float width, PdfDashPattern dash)
            : base(new PdfNumber(hRadius))
        {
            Add(new PdfNumber(vRadius));
            Add(new PdfNumber(width));
            if (dash != null)
            {
                Add(dash);
            }
        }
    }
}
