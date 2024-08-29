using Sign.itext.pdf;

namespace Sign.itext.text.pdf.events
{
    public class PdfPTableEventForwarder : IPdfPTableEventAfterSplit, IPdfPTableEventSplit, IPdfPTableEvent
    {
        protected List<IPdfPTableEvent> events = new List<IPdfPTableEvent>();

        public virtual void AddTableEvent(IPdfPTableEvent eventa)
        {
            events.Add(eventa);
        }

        public virtual void TableLayout(PdfPTable table, float[][] widths, float[] heights, int headerRows, int rowStart, PdfContentByte[] canvases)
        {
            foreach (IPdfPTableEvent @event in events)
            {
                @event.TableLayout(table, widths, heights, headerRows, rowStart, canvases);
            }
        }

        public virtual void SplitTable(PdfPTable table)
        {
            foreach (IPdfPTableEvent @event in events)
            {
                if (@event is IPdfPTableEventSplit)
                {
                    ((IPdfPTableEventSplit)@event).SplitTable(table);
                }
            }
        }

        public virtual void AfterSplitTable(PdfPTable table, PdfPRow startRow, int startIdx)
        {
            foreach (IPdfPTableEvent @event in events)
            {
                if (@event is IPdfPTableEventAfterSplit)
                {
                    ((IPdfPTableEventAfterSplit)@event).AfterSplitTable(table, startRow, startIdx);
                }
            }
        }
    }
}
