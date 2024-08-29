using Sign.itext.pdf;

namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfViewerPreferences
    {
        int ViewerPreferences { set; }

        void AddViewerPreference(PdfName key, PdfObject value);
    }
}
