using Sign.itext.pdf;

namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfDocumentActions
    {
        void SetOpenAction(string name);

        void SetOpenAction(PdfAction action);

        void SetAdditionalAction(PdfName actionType, PdfAction action);
    }
}
