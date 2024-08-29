namespace Sign.itext.text.pdf
{
    public interface IPdfPTableEventAfterSplit : IPdfPTableEventSplit, IPdfPTableEvent
    {
        void AfterSplitTable(PdfPTable table, PdfPRow startRow, int startIdx);
    }
}
