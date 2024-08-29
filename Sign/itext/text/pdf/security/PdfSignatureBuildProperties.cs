using Sign.itext.pdf;

namespace Sign.itext.text.pdf.security
{
    internal class PdfSignatureBuildProperties : PdfDictionary
    {
        public virtual string SignatureCreator
        {
            set
            {
                GetPdfSignatureAppProperty().SignatureCreator = value;
            }
        }

        private PdfSignatureAppDictionary GetPdfSignatureAppProperty()
        {
            PdfSignatureAppDictionary pdfSignatureAppDictionary = (PdfSignatureAppDictionary)GetAsDict(PdfName.APP);
            if (pdfSignatureAppDictionary == null)
            {
                pdfSignatureAppDictionary = new PdfSignatureAppDictionary();
                Put(PdfName.APP, pdfSignatureAppDictionary);
            }

            return pdfSignatureAppDictionary;
        }
    }
}
