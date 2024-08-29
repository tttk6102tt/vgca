using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfLayer : PdfDictionary, IPdfOCG
    {
        protected PdfIndirectReference refi;

        protected List<PdfLayer> children;

        protected PdfLayer parent;

        protected string title;

        private bool on = true;

        private bool onPanel = true;

        internal string Title => title;

        public virtual PdfLayer Parent => parent;

        public virtual List<PdfLayer> Children => children;

        public virtual PdfIndirectReference Ref
        {
            get
            {
                return refi;
            }
            set
            {
                refi = value;
            }
        }

        public virtual string Name
        {
            set
            {
                Put(PdfName.NAME, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual PdfObject PdfObject => this;

        public virtual bool On
        {
            get
            {
                return on;
            }
            set
            {
                on = value;
            }
        }

        private PdfDictionary Usage
        {
            get
            {
                PdfDictionary pdfDictionary = GetAsDict(PdfName.USAGE);
                if (pdfDictionary == null)
                {
                    pdfDictionary = new PdfDictionary();
                    Put(PdfName.USAGE, pdfDictionary);
                }

                return pdfDictionary;
            }
        }

        public virtual bool Export
        {
            set
            {
                PdfDictionary usage = Usage;
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.EXPORTSTATE, value ? PdfName.ON : PdfName.OFF);
                usage.Put(PdfName.EXPORT, pdfDictionary);
            }
        }

        public virtual bool View
        {
            set
            {
                PdfDictionary usage = Usage;
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.VIEWSTATE, value ? PdfName.ON : PdfName.OFF);
                usage.Put(PdfName.VIEW, pdfDictionary);
            }
        }

        public virtual string PageElement
        {
            set
            {
                PdfDictionary usage = Usage;
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.SUBTYPE, new PdfName(value));
                usage.Put(PdfName.PAGEELEMENT, pdfDictionary);
            }
        }

        public virtual bool OnPanel
        {
            get
            {
                return onPanel;
            }
            set
            {
                onPanel = value;
            }
        }

        internal PdfLayer(string title)
        {
            this.title = title;
        }

        public static PdfLayer CreateTitle(string title, PdfWriter writer)
        {
            if (title == null)
            {
                throw new ArgumentNullException(MessageLocalization.GetComposedMessage("title.cannot.be.null"));
            }

            PdfLayer pdfLayer = new PdfLayer(title);
            writer.RegisterLayer(pdfLayer);
            return pdfLayer;
        }

        public PdfLayer(string name, PdfWriter writer)
            : base(PdfName.OCG)
        {
            Name = name;
            if (writer is PdfStamperImp)
            {
                refi = writer.AddToBody(this).IndirectReference;
            }
            else
            {
                refi = writer.PdfIndirectReference;
            }

            writer.RegisterLayer(this);
        }

        public virtual void AddChild(PdfLayer child)
        {
            if (child.parent != null)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.layer.1.already.has.a.parent", child.GetAsString(PdfName.NAME).ToUnicodeString()));
            }

            child.parent = this;
            if (children == null)
            {
                children = new List<PdfLayer>();
            }

            children.Add(child);
        }

        public virtual void SetCreatorInfo(string creator, string subtype)
        {
            PdfDictionary usage = Usage;
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.CREATOR, new PdfString(creator, "UnicodeBig"));
            pdfDictionary.Put(PdfName.SUBTYPE, new PdfName(subtype));
            usage.Put(PdfName.CREATORINFO, pdfDictionary);
        }

        public virtual void SetLanguage(string lang, bool preferred)
        {
            PdfDictionary usage = Usage;
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.LANG, new PdfString(lang, "UnicodeBig"));
            if (preferred)
            {
                pdfDictionary.Put(PdfName.PREFERRED, PdfName.ON);
            }

            usage.Put(PdfName.LANGUAGE, pdfDictionary);
        }

        public virtual void SetZoom(float min, float max)
        {
            if (!(min <= 0f) || !(max < 0f))
            {
                PdfDictionary usage = Usage;
                PdfDictionary pdfDictionary = new PdfDictionary();
                if (min > 0f)
                {
                    pdfDictionary.Put(PdfName.MIN_LOWER_CASE, new PdfNumber(min));
                }

                if (max >= 0f)
                {
                    pdfDictionary.Put(PdfName.MAX_LOWER_CASE, new PdfNumber(max));
                }

                usage.Put(PdfName.ZOOM, pdfDictionary);
            }
        }

        public virtual void SetPrint(string subtype, bool printstate)
        {
            PdfDictionary usage = Usage;
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.SUBTYPE, new PdfName(subtype));
            pdfDictionary.Put(PdfName.PRINTSTATE, printstate ? PdfName.ON : PdfName.OFF);
            usage.Put(PdfName.PRINT, pdfDictionary);
        }

        public virtual void SetUser(string type, string[] names)
        {
            PdfDictionary usage = Usage;
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.TYPE, new PdfName(type));
            PdfArray pdfArray = new PdfArray();
            foreach (string value in names)
            {
                pdfArray.Add(new PdfString(value, "UnicodeBig"));
            }

            usage.Put(PdfName.NAME, pdfArray);
            usage.Put(PdfName.USER, pdfDictionary);
        }
    }
}
