namespace Sign.itext.text.pdf
{
    public interface IPdfPageEvent
    {
        void OnOpenDocument(PdfWriter writer, Document document);

        void OnStartPage(PdfWriter writer, Document document);

        void OnEndPage(PdfWriter writer, Document document);

        void OnCloseDocument(PdfWriter writer, Document document);

        void OnParagraph(PdfWriter writer, Document document, float paragraphPosition);

        void OnParagraphEnd(PdfWriter writer, Document document, float paragraphPosition);

        void OnChapter(PdfWriter writer, Document document, float paragraphPosition, Paragraph title);

        void OnChapterEnd(PdfWriter writer, Document document, float paragraphPosition);

        void OnSection(PdfWriter writer, Document document, float paragraphPosition, int depth, Paragraph title);

        void OnSectionEnd(PdfWriter writer, Document document, float paragraphPosition);

        void OnGenericTag(PdfWriter writer, Document document, Rectangle rect, string text);
    }
}
