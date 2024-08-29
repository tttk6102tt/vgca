using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfPTableHeader : PdfPTableBody
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

        public PdfPTableHeader()
        {
            role = PdfName.THEAD;
        }
    }
}
