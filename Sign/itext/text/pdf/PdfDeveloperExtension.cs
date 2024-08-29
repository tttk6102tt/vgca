using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfDeveloperExtension
    {
        public static readonly PdfDeveloperExtension ADOBE_1_7_EXTENSIONLEVEL3 = new PdfDeveloperExtension(PdfName.ADBE, PdfWriter.PDF_VERSION_1_7, 3);

        public static readonly PdfDeveloperExtension ESIC_1_7_EXTENSIONLEVEL2 = new PdfDeveloperExtension(PdfName.ESIC, PdfWriter.PDF_VERSION_1_7, 2);

        public static readonly PdfDeveloperExtension ESIC_1_7_EXTENSIONLEVEL5 = new PdfDeveloperExtension(PdfName.ESIC, PdfWriter.PDF_VERSION_1_7, 5);

        protected PdfName prefix;

        protected PdfName baseversion;

        protected int extensionLevel;

        public virtual PdfName Prefix => prefix;

        public virtual PdfName Baseversion => baseversion;

        public virtual int ExtensionLevel => extensionLevel;

        public PdfDeveloperExtension(PdfName prefix, PdfName baseversion, int extensionLevel)
        {
            this.prefix = prefix;
            this.baseversion = baseversion;
            this.extensionLevel = extensionLevel;
        }

        public virtual PdfDictionary GetDeveloperExtensions()
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.BASEVERSION, baseversion);
            pdfDictionary.Put(PdfName.EXTENSIONLEVEL, new PdfNumber(extensionLevel));
            return pdfDictionary;
        }
    }
}
