namespace Sign.itext.text.pdf
{
    public interface IPdfPTableEventSplit : IPdfPTableEvent
    {
        void SplitTable(PdfPTable table);
    }
}
