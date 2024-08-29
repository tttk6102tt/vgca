using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public interface IPdfSpecialColorSpace
    {
        ColorDetails[] GetColorantDetails(PdfWriter writer);
    }
}
