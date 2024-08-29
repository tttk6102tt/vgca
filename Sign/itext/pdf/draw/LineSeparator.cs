namespace Sign.itext.pdf.draw
{
    public class LineSeparator : VerticalPositionMark
    {
        protected float lineWidth = 1f;

        protected float percentage = 100f;

        protected BaseColor lineColor;

        protected int alignment = 6;

        public virtual float LineWidth
        {
            get
            {
                return lineWidth;
            }
            set
            {
                lineWidth = value;
            }
        }

        public virtual float Percentage
        {
            get
            {
                return percentage;
            }
            set
            {
                percentage = value;
            }
        }

        public virtual BaseColor LineColor
        {
            get
            {
                return lineColor;
            }
            set
            {
                lineColor = value;
            }
        }

        public virtual int Alignment
        {
            get
            {
                return alignment;
            }
            set
            {
                alignment = value;
            }
        }

        public LineSeparator(float lineWidth, float percentage, BaseColor lineColor, int align, float offset)
        {
            this.lineWidth = lineWidth;
            this.percentage = percentage;
            this.lineColor = lineColor;
            alignment = align;
            base.offset = offset;
        }

        public LineSeparator(Font font)
        {
            lineWidth = 71f / (339f * (float)Math.PI) * font.Size;
            offset = -0.333333343f * font.Size;
            percentage = 100f;
            lineColor = font.Color;
        }

        public LineSeparator()
        {
        }

        public override void Draw(PdfContentByte canvas, float llx, float lly, float urx, float ury, float y)
        {
            canvas.SaveState();
            DrawLine(canvas, llx, urx, y);
            canvas.RestoreState();
        }

        public virtual void DrawLine(PdfContentByte canvas, float leftX, float rightX, float y)
        {
            float num = ((!(Percentage < 0f)) ? ((rightX - leftX) * Percentage / 100f) : (0f - Percentage));
            float num2 = Alignment switch
            {
                0 => 0f,
                2 => rightX - leftX - num,
                _ => (rightX - leftX - num) / 2f,
            };
            canvas.SetLineWidth(LineWidth);
            if (LineColor != null)
            {
                canvas.SetColorStroke(LineColor);
            }

            canvas.MoveTo(num2 + leftX, y + offset);
            canvas.LineTo(num2 + num + leftX, y + offset);
            canvas.Stroke();
        }
    }
}
