using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.api;
using Sign.itext.text.factories;

namespace Sign.itext.text
{
    public class List : ITextElementArray, IElement, IIndentable, IAccessibleElement
    {
        public const bool ORDERED = true;

        public const bool UNORDERED = false;

        public const bool NUMERICAL = false;

        public const bool ALPHABETICAL = true;

        public const bool UPPERCASE = false;

        public const bool LOWERCASE = true;

        protected List<IElement> list = new List<IElement>();

        protected bool numbered;

        protected bool lettered;

        protected bool lowercase;

        protected bool autoindent;

        protected bool alignindent;

        protected int first = 1;

        protected Chunk symbol = new Chunk("-");

        protected string preSymbol = "";

        protected string postSymbol = ". ";

        protected float indentationLeft;

        protected float indentationRight;

        protected float symbolIndent;

        protected PdfName role = PdfName.L;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id = new AccessibleElementId();

        public virtual int Type => 14;

        public virtual IList<Chunk> Chunks
        {
            get
            {
                List<Chunk> list = new List<Chunk>();
                foreach (IElement item in this.list)
                {
                    list.AddRange(item.Chunks);
                }

                return list;
            }
        }

        public virtual bool Numbered
        {
            get
            {
                return numbered;
            }
            set
            {
                numbered = value;
            }
        }

        public virtual bool Lettered
        {
            get
            {
                return lettered;
            }
            set
            {
                lettered = value;
            }
        }

        public virtual bool Lowercase
        {
            get
            {
                return lowercase;
            }
            set
            {
                lowercase = value;
            }
        }

        public virtual bool IsLowercase
        {
            get
            {
                return lowercase;
            }
            set
            {
                lowercase = value;
            }
        }

        public virtual bool Autoindent
        {
            get
            {
                return autoindent;
            }
            set
            {
                autoindent = value;
            }
        }

        public virtual bool Alignindent
        {
            get
            {
                return alignindent;
            }
            set
            {
                alignindent = value;
            }
        }

        public virtual int First
        {
            get
            {
                return first;
            }
            set
            {
                first = value;
            }
        }

        public virtual Chunk ListSymbol
        {
            set
            {
                symbol = value;
            }
        }

        public virtual float IndentationLeft
        {
            get
            {
                return indentationLeft;
            }
            set
            {
                indentationLeft = value;
            }
        }

        public virtual float IndentationRight
        {
            get
            {
                return indentationRight;
            }
            set
            {
                indentationRight = value;
            }
        }

        public virtual float SymbolIndent
        {
            get
            {
                return symbolIndent;
            }
            set
            {
                symbolIndent = value;
            }
        }

        public virtual List<IElement> Items => list;

        public virtual int Size => list.Count;

        public virtual float TotalLeading
        {
            get
            {
                if (list.Count < 1)
                {
                    return -1f;
                }

                return ((ListItem)list[0]).TotalLeading;
            }
        }

        public virtual Chunk Symbol
        {
            get
            {
                return symbol;
            }
            set
            {
                symbol = value;
            }
        }

        public virtual string PostSymbol
        {
            get
            {
                return postSymbol;
            }
            set
            {
                postSymbol = value;
            }
        }

        public virtual string PreSymbol
        {
            get
            {
                return preSymbol;
            }
            set
            {
                preSymbol = value;
            }
        }

        public virtual PdfName Role
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        public virtual AccessibleElementId ID
        {
            get
            {
                if (id == null)
                {
                    id = new AccessibleElementId();
                }

                return id;
            }
            set
            {
                id = value;
            }
        }

        public virtual bool IsInline => false;

        public List()
            : this(numbered: false, lettered: false)
        {
        }

        public List(float symbolIndent)
        {
            this.symbolIndent = symbolIndent;
        }

        public List(bool numbered)
            : this(numbered, lettered: false)
        {
        }

        public List(bool numbered, bool lettered)
        {
            this.numbered = numbered;
            this.lettered = lettered;
            autoindent = true;
            alignindent = true;
        }

        public List(bool numbered, float symbolIndent)
            : this(numbered, lettered: false, symbolIndent)
        {
        }

        public List(bool numbered, bool lettered, float symbolIndent)
        {
            this.numbered = numbered;
            this.lettered = lettered;
            this.symbolIndent = symbolIndent;
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                foreach (IElement item in list)
                {
                    listener.Add(item);
                }

                return true;
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        public virtual bool Add(string s)
        {
            if (s != null)
            {
                return Add(new ListItem(s));
            }

            return false;
        }

        public virtual bool Add(IElement o)
        {
            if (o is ListItem)
            {
                ListItem listItem = (ListItem)o;
                if (numbered || lettered)
                {
                    Chunk chunk = new Chunk(preSymbol, symbol.Font);
                    chunk.Attributes = symbol.Attributes;
                    int index = first + this.list.Count;
                    if (lettered)
                    {
                        chunk.Append(RomanAlphabetFactory.GetString(index, lowercase));
                    }
                    else
                    {
                        chunk.Append(index.ToString());
                    }

                    chunk.Append(postSymbol);
                    listItem.ListSymbol = chunk;
                }
                else
                {
                    listItem.ListSymbol = symbol;
                }

                listItem.SetIndentationLeft(symbolIndent, autoindent);
                listItem.IndentationRight = 0f;
                this.list.Add(listItem);
                return true;
            }

            if (o is List)
            {
                List list = (List)o;
                list.IndentationLeft += symbolIndent;
                first--;
                this.list.Add(list);
                return true;
            }

            return false;
        }

        public virtual void NormalizeIndentation()
        {
            float val = 0f;
            foreach (IElement item in list)
            {
                if (item is ListItem)
                {
                    val = Math.Max(val, ((ListItem)item).IndentationLeft);
                }
            }

            foreach (IElement item2 in list)
            {
                if (item2 is ListItem)
                {
                    ((ListItem)item2).IndentationLeft = val;
                }
            }
        }

        public virtual void SetListSymbol(string symbol)
        {
            this.symbol = new Chunk(symbol);
        }

        public virtual bool IsContent()
        {
            return true;
        }

        public virtual bool IsNestable()
        {
            return true;
        }

        public virtual bool IsEmpty()
        {
            return list.Count == 0;
        }

        public virtual string getPostSymbol()
        {
            return postSymbol;
        }

        public virtual ListItem GetFirstItem()
        {
            IElement element = ((list.Count > 0) ? list[0] : null);
            if (element != null)
            {
                if (element is ListItem)
                {
                    return (ListItem)element;
                }

                if (element is List)
                {
                    return ((List)element).GetFirstItem();
                }
            }

            return null;
        }

        public virtual ListItem GetLastItem()
        {
            IElement element = ((list.Count > 0) ? list[list.Count - 1] : null);
            if (element != null)
            {
                if (element is ListItem)
                {
                    return (ListItem)element;
                }

                if (element is List)
                {
                    return ((List)element).GetLastItem();
                }
            }

            return null;
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            if (accessibleAttributes != null)
            {
                accessibleAttributes.TryGetValue(key, out var value);
                return value;
            }

            return null;
        }

        public virtual void SetAccessibleAttribute(PdfName key, PdfObject value)
        {
            if (accessibleAttributes == null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            }

            accessibleAttributes[key] = value;
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleAttributes()
        {
            return accessibleAttributes;
        }
    }
}
