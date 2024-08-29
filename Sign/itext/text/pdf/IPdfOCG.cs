using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public interface IPdfOCG
    {
        PdfIndirectReference Ref { get; }

        PdfObject PdfObject { get; }
    }
}
