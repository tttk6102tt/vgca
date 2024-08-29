namespace Sign.itext.pdf.draw
{
    public interface IDrawInterface
    {
        void Draw(PdfContentByte canvas, float llx, float lly, float urx, float ury, float y);
    }
}
