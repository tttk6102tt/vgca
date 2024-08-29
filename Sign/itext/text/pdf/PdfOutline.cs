using Sign.itext.pdf;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfOutline : PdfDictionary
    {
        private PdfIndirectReference reference;

        private int count;

        private PdfOutline parent;

        private PdfDestination destination;

        private PdfAction action;

        protected List<PdfOutline> kids = new List<PdfOutline>();

        protected PdfWriter writer;

        private string tag;

        private bool open;

        private BaseColor color;

        private int style;

        public virtual PdfIndirectReference IndirectReference
        {
            get
            {
                return reference;
            }
            set
            {
                reference = value;
            }
        }

        public virtual PdfOutline Parent => parent;

        public virtual PdfDestination PdfDestination => destination;

        internal int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }

        public virtual int Level
        {
            get
            {
                if (parent == null)
                {
                    return 0;
                }

                return parent.Level + 1;
            }
        }

        public virtual List<PdfOutline> Kids
        {
            get
            {
                return kids;
            }
            set
            {
                kids = value;
            }
        }

        public virtual string Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }

        public virtual string Title
        {
            get
            {
                return ((PdfString)Get(PdfName.TITLE)).ToString();
            }
            set
            {
                Put(PdfName.TITLE, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual bool Open
        {
            get
            {
                return open;
            }
            set
            {
                open = value;
            }
        }

        public virtual BaseColor Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public virtual int Style
        {
            get
            {
                return style;
            }
            set
            {
                style = value;
            }
        }

        internal PdfOutline(PdfWriter writer)
            : base(PdfDictionary.OUTLINES)
        {
            open = true;
            parent = null;
            this.writer = writer;
        }

        public PdfOutline(PdfOutline parent, PdfAction action, string title)
            : this(parent, action, title, open: true)
        {
        }

        public PdfOutline(PdfOutline parent, PdfAction action, string title, bool open)
        {
            this.action = action;
            InitOutline(parent, title, open);
        }

        public PdfOutline(PdfOutline parent, PdfDestination destination, string title)
            : this(parent, destination, title, open: true)
        {
        }

        public PdfOutline(PdfOutline parent, PdfDestination destination, string title, bool open)
        {
            this.destination = destination;
            InitOutline(parent, title, open);
        }

        public PdfOutline(PdfOutline parent, PdfAction action, PdfString title)
            : this(parent, action, title, open: true)
        {
        }

        public PdfOutline(PdfOutline parent, PdfAction action, PdfString title, bool open)
            : this(parent, action, title.ToString(), open)
        {
        }

        public PdfOutline(PdfOutline parent, PdfDestination destination, PdfString title)
            : this(parent, destination, title, open: true)
        {
        }

        public PdfOutline(PdfOutline parent, PdfDestination destination, PdfString title, bool open)
            : this(parent, destination, title.ToString(), open: true)
        {
        }

        public PdfOutline(PdfOutline parent, PdfAction action, Paragraph title)
            : this(parent, action, title, open: true)
        {
        }

        public PdfOutline(PdfOutline parent, PdfAction action, Paragraph title, bool open)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Chunk chunk in title.Chunks)
            {
                stringBuilder.Append(chunk.Content);
            }

            this.action = action;
            InitOutline(parent, stringBuilder.ToString(), open);
        }

        public PdfOutline(PdfOutline parent, PdfDestination destination, Paragraph title)
            : this(parent, destination, title, open: true)
        {
        }

        public PdfOutline(PdfOutline parent, PdfDestination destination, Paragraph title, bool open)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Chunk chunk in title.Chunks)
            {
                stringBuilder.Append(chunk.Content);
            }

            this.destination = destination;
            InitOutline(parent, stringBuilder.ToString(), open);
        }

        internal void InitOutline(PdfOutline parent, string title, bool open)
        {
            this.open = open;
            this.parent = parent;
            writer = parent.writer;
            Put(PdfName.TITLE, new PdfString(title, "UnicodeBig"));
            parent.AddKid(this);
            if (destination != null && !destination.HasPage())
            {
                SetDestinationPage(writer.CurrentPage);
            }
        }

        public virtual bool SetDestinationPage(PdfIndirectReference pageReference)
        {
            if (destination == null)
            {
                return false;
            }

            return destination.AddPage(pageReference);
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            if (color != null && !color.Equals(BaseColor.BLACK))
            {
                Put(PdfName.C, new PdfArray(new float[3]
                {
                    (float)color.R / 255f,
                    (float)color.G / 255f,
                    (float)color.B / 255f
                }));
            }

            int num = 0;
            if (((uint)style & (true ? 1u : 0u)) != 0)
            {
                num |= 2;
            }

            if (((uint)style & 2u) != 0)
            {
                num |= 1;
            }

            if (num != 0)
            {
                Put(PdfName.F, new PdfNumber(num));
            }

            if (parent != null)
            {
                Put(PdfName.PARENT, parent.IndirectReference);
            }

            if (destination != null && destination.HasPage())
            {
                Put(PdfName.DEST, destination);
            }

            if (action != null)
            {
                Put(PdfName.A, action);
            }

            if (count != 0)
            {
                Put(PdfName.COUNT, new PdfNumber(count));
            }

            base.ToPdf(writer, os);
        }

        public virtual void AddKid(PdfOutline outline)
        {
            kids.Add(outline);
        }
    }
}
