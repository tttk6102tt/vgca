using Sign.itext.pdf;

namespace Sign.itext.text
{
    public class ListItem : Paragraph
    {
        protected Chunk symbol;

        private ListBody listBody;

        private ListLabel listLabel;

        public override int Type => 15;

        public virtual Chunk ListSymbol
        {
            get
            {
                return symbol;
            }
            set
            {
                if (symbol == null)
                {
                    symbol = value;
                    if (symbol.Font.IsStandardFont())
                    {
                        symbol.Font = font;
                    }
                }
            }
        }

        public virtual ListBody ListBody
        {
            get
            {
                if (listBody == null)
                {
                    listBody = new ListBody(this);
                }

                return listBody;
            }
        }

        public virtual ListLabel ListLabel
        {
            get
            {
                if (listLabel == null)
                {
                    listLabel = new ListLabel(this);
                }

                return listLabel;
            }
        }

        public ListItem()
        {
            Role = PdfName.LI;
        }

        public ListItem(float leading)
            : base(leading)
        {
            Role = PdfName.LI;
        }

        public ListItem(Chunk chunk)
            : base(chunk)
        {
            Role = PdfName.LI;
        }

        public ListItem(string str)
            : base(str)
        {
            Role = PdfName.LI;
        }

        public ListItem(string str, Font font)
            : base(str, font)
        {
            Role = PdfName.LI;
        }

        public ListItem(float leading, Chunk chunk)
            : base(leading, chunk)
        {
            Role = PdfName.LI;
        }

        public ListItem(float leading, string str)
            : base(leading, str)
        {
            Role = PdfName.LI;
        }

        public ListItem(float leading, string str, Font font)
            : base(leading, str, font)
        {
            Role = PdfName.LI;
        }

        public ListItem(Phrase phrase)
            : base(phrase)
        {
            Role = PdfName.LI;
        }

        public virtual void SetIndentationLeft(float indentation, bool autoindent)
        {
            if (autoindent)
            {
                IndentationLeft = ListSymbol.GetWidthPoint();
            }
            else
            {
                IndentationLeft = indentation;
            }
        }

        public virtual void AdjustListSymbolFont()
        {
            IList<Chunk> chunks = Chunks;
            if (chunks.Count != 0 && symbol != null)
            {
                symbol.Font = chunks[0].Font;
            }
        }
    }
}
