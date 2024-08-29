using Sign.itext.pdf;

namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfVersion
    {
        char PdfVersion { set; }

        void SetAtLeastPdfVersion(char version);

        void SetPdfVersion(PdfName version);

        void AddDeveloperExtension(PdfDeveloperExtension de);
    }
}
