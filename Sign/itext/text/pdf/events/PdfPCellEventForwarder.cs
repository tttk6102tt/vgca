using Sign.itext.pdf;

namespace Sign.itext.text.pdf.events
{
    public class PdfPCellEventForwarder : IPdfPCellEvent
    {
        protected List<IPdfPCellEvent> events = new List<IPdfPCellEvent>();

        public virtual void AddCellEvent(IPdfPCellEvent eventa)
        {
            events.Add(eventa);
        }

        public virtual void CellLayout(PdfPCell cell, Rectangle position, PdfContentByte[] canvases)
        {
            foreach (IPdfPCellEvent @event in events)
            {
                @event.CellLayout(cell, position, canvases);
            }
        }
    }
}
