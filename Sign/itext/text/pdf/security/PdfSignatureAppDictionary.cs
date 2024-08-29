using Sign.itext.pdf;

namespace Sign.itext.text.pdf.security
{
    internal class PdfSignatureAppDictionary : PdfDictionary
    {
        public virtual string SignatureCreator
        {
            set
            {
                Put(PdfName.NAME, new PdfName(value));
            }
        }
    }
}
