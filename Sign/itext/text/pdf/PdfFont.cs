using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfFont : IComparable<PdfFont>
    {
        private BaseFont font;

        private float size;

        protected float hScale = 1f;

        internal float Size => size;

        internal BaseFont Font => font;

        internal static PdfFont DefaultFont => new PdfFont(BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false), 12f);

        internal float HorizontalScaling
        {
            get
            {
                return hScale;
            }
            set
            {
                hScale = value;
            }
        }

        internal PdfFont(BaseFont bf, float size)
        {
            this.size = size;
            font = bf;
        }

        public virtual int CompareTo(PdfFont pdfFont)
        {
            if (pdfFont == null)
            {
                return -1;
            }

            try
            {
                if (font != pdfFont.font)
                {
                    return 1;
                }

                if (Size != pdfFont.Size)
                {
                    return 2;
                }

                return 0;
            }
            catch (InvalidCastException)
            {
                return -2;
            }
        }

        internal float Width()
        {
            return Width(32);
        }

        internal float Width(int character)
        {
            return font.GetWidthPoint(character, size) * hScale;
        }

        internal float Width(string s)
        {
            return font.GetWidthPoint(s, size) * hScale;
        }
    }
}
