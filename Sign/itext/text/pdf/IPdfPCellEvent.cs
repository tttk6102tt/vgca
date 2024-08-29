using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public interface IPdfPCellEvent
    {
        void CellLayout(PdfPCell cell, Rectangle position, PdfContentByte[] canvases);
    }
}
