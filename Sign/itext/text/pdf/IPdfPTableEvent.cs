using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public interface IPdfPTableEvent
    {
        void TableLayout(PdfPTable table, float[][] widths, float[] heights, int headerRows, int rowStart, PdfContentByte[] canvases);
    }
}
