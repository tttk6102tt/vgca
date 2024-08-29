namespace Sign.itext.text.pdf.events
{
    public class PdfPageEventForwarder : IPdfPageEvent
    {
        protected List<IPdfPageEvent> events = new List<IPdfPageEvent>();

        public virtual void AddPageEvent(IPdfPageEvent eventa)
        {
            events.Add(eventa);
        }

        public virtual void OnOpenDocument(PdfWriter writer, Document document)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnOpenDocument(writer, document);
            }
        }

        public virtual void OnStartPage(PdfWriter writer, Document document)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnStartPage(writer, document);
            }
        }

        public virtual void OnEndPage(PdfWriter writer, Document document)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnEndPage(writer, document);
            }
        }

        public virtual void OnCloseDocument(PdfWriter writer, Document document)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnCloseDocument(writer, document);
            }
        }

        public virtual void OnParagraph(PdfWriter writer, Document document, float paragraphPosition)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnParagraph(writer, document, paragraphPosition);
            }
        }

        public virtual void OnParagraphEnd(PdfWriter writer, Document document, float paragraphPosition)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnParagraphEnd(writer, document, paragraphPosition);
            }
        }

        public virtual void OnChapter(PdfWriter writer, Document document, float paragraphPosition, Paragraph title)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnChapter(writer, document, paragraphPosition, title);
            }
        }

        public virtual void OnChapterEnd(PdfWriter writer, Document document, float position)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnChapterEnd(writer, document, position);
            }
        }

        public virtual void OnSection(PdfWriter writer, Document document, float paragraphPosition, int depth, Paragraph title)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnSection(writer, document, paragraphPosition, depth, title);
            }
        }

        public virtual void OnSectionEnd(PdfWriter writer, Document document, float position)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnSectionEnd(writer, document, position);
            }
        }

        public virtual void OnGenericTag(PdfWriter writer, Document document, Rectangle rect, string text)
        {
            foreach (IPdfPageEvent @event in events)
            {
                @event.OnGenericTag(writer, document, rect, text);
            }
        }
    }
}
