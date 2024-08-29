namespace Sign.itext.text.pdf
{
    public class PdfDashPattern : PdfArray
    {
        private float dash = -1f;

        private float gap = -1f;

        private float phase = -1f;

        public PdfDashPattern()
        {
        }

        public PdfDashPattern(float dash)
            : base(new PdfNumber(dash))
        {
            this.dash = dash;
        }

        public PdfDashPattern(float dash, float gap)
            : base(new PdfNumber(dash))
        {
            Add(new PdfNumber(gap));
            this.dash = dash;
            this.gap = gap;
        }

        public PdfDashPattern(float dash, float gap, float phase)
            : base(new PdfNumber(dash))
        {
            Add(new PdfNumber(gap));
            this.dash = dash;
            this.gap = gap;
            this.phase = phase;
        }

        public virtual void Add(float n)
        {
            Add(new PdfNumber(n));
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            os.WriteByte(91);
            if (dash >= 0f)
            {
                new PdfNumber(dash).ToPdf(writer, os);
                if (gap >= 0f)
                {
                    os.WriteByte(32);
                    new PdfNumber(gap).ToPdf(writer, os);
                }
            }

            os.WriteByte(93);
            if (phase >= 0f)
            {
                os.WriteByte(32);
                new PdfNumber(phase).ToPdf(writer, os);
            }
        }
    }
}
