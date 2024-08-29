using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfPTableFooter : PdfPTableBody
    {
        public override PdfName Role
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        public PdfPTableFooter()
        {
            role = PdfName.TFOOT;
        }
    }
}
