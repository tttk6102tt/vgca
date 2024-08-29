using Sign.itext.pdf;

namespace Sign.itext.text.pdf.collection
{
    public class PdfTargetDictionary : PdfDictionary
    {
        public virtual string EmbeddedFileName
        {
            set
            {
                Put(PdfName.N, new PdfString(value, null));
            }
        }

        public virtual string FileAttachmentPagename
        {
            set
            {
                Put(PdfName.P, new PdfString(value, null));
            }
        }

        public virtual int FileAttachmentPage
        {
            set
            {
                Put(PdfName.P, new PdfNumber(value));
            }
        }

        public virtual string FileAttachmentName
        {
            set
            {
                Put(PdfName.A, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual int FileAttachmentIndex
        {
            set
            {
                Put(PdfName.A, new PdfNumber(value));
            }
        }

        public virtual PdfTargetDictionary AdditionalPath
        {
            set
            {
                Put(PdfName.T, value);
            }
        }

        public PdfTargetDictionary(PdfTargetDictionary nested)
        {
            Put(PdfName.R, PdfName.P);
            if (nested != null)
            {
                AdditionalPath = nested;
            }
        }

        public PdfTargetDictionary(bool child)
        {
            if (child)
            {
                Put(PdfName.R, PdfName.C);
            }
            else
            {
                Put(PdfName.R, PdfName.P);
            }
        }
    }
}
