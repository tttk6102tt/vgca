using Sign.itext.pdf;

namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfPageActions
    {
        int Duration { set; }

        PdfTransition Transition { set; }

        void SetPageAction(PdfName actionType, PdfAction action);
    }
}
