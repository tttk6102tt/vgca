using Sign.itext.pdf;

namespace Sign.itext.text.pdf.interfaces
{
    public interface IPdfStructureElement
    {
        PdfObject GetAttribute(PdfName name);

        void SetAttribute(PdfName name, PdfObject obj);
    }
}
