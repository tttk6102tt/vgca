namespace Sign.itext.pdf
{
    public class PdfNull : PdfObject
    {
        public static PdfNull PDFNULL = new PdfNull();

        public PdfNull()
            : base(8, "null")
        {
        }

        public override string ToString()
        {
            return "null";
        }
    }
}
