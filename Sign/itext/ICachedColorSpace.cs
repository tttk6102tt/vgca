using Sign.itext.pdf;
using Sign.itext.text.pdf;

namespace Sign.itext
{
    public interface ICachedColorSpace
    {
        PdfObject GetPdfObject(PdfWriter writer);

        new bool Equals(object obj);

        new int GetHashCode();
    }
}
