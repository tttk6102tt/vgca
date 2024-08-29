namespace Sign.itext.pdf.draw
{
    public class DottedLineSeparator : LineSeparator
    {
        protected float gap = 5f;

        public override void Draw(
          PdfContentByte canvas,
          float llx,
          float lly,
          float urx,
          float ury,
          float y)
        {
            canvas.SaveState();
            canvas.SetLineWidth(this.lineWidth);
            canvas.SetLineCap(1);
            canvas.SetLineDash(0.0f, this.gap, this.gap / 2f);
            this.DrawLine(canvas, llx, urx, y);
            canvas.RestoreState();
        }

        public virtual float Gap
        {
            get => this.gap;
            set => this.gap = value;
        }
    }
}
