using Sign.itext.text.pdf;

namespace Sign.itext.text.api
{
    public interface IWriterOperation
    {
        void Write(PdfWriter writer, Document doc);
    }
}
