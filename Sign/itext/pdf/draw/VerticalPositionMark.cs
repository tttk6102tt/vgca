namespace Sign.itext.pdf.draw
{
    public class VerticalPositionMark : IDrawInterface, IElement
    {
        protected IDrawInterface drawInterface;

        protected float offset;

        public virtual int Type => 55;

        public virtual IList<Chunk> Chunks => new List<Chunk>
        {
            new Chunk(this, vertical: true)
        };

        public virtual IDrawInterface DrawInterface
        {
            get
            {
                return drawInterface;
            }
            set
            {
                drawInterface = value;
            }
        }

        public virtual float Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
            }
        }

        public VerticalPositionMark()
        {
        }

        public VerticalPositionMark(IDrawInterface drawInterface, float offset)
        {
            this.drawInterface = drawInterface;
            this.offset = offset;
        }

        public virtual void Draw(PdfContentByte canvas, float llx, float lly, float urx, float ury, float y)
        {
            if (drawInterface != null)
            {
                drawInterface.Draw(canvas, llx, lly, urx, ury, y + offset);
            }
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                return listener.Add(this);
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        public virtual bool IsContent()
        {
            return true;
        }

        public virtual bool IsNestable()
        {
            return false;
        }
    }
}
