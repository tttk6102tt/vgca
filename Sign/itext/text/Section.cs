using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.api;
using System.Text;

namespace Sign.itext.text
{
    public class Section : List<IElement>, ITextElementArray, IElement, ILargeElement, IIndentable, IAccessibleElement
    {
        public const int NUMBERSTYLE_DOTTED = 0;

        public const int NUMBERSTYLE_DOTTED_WITHOUT_FINAL_DOT = 1;

        protected Paragraph title;

        protected int numberDepth;

        protected int numberStyle;

        protected float indentationLeft;

        protected float indentationRight;

        protected float indentation;

        protected int subsections;

        protected internal List<int> numbers;

        protected bool complete = true;

        protected bool addedCompletely;

        protected bool notAddedYet = true;

        protected bool bookmarkOpen = true;

        protected bool triggerNewPage;

        protected string bookmarkTitle;

        public virtual int Type => 13;

        public virtual IList<Chunk> Chunks
        {
            get
            {
                List<Chunk> list = new List<Chunk>();
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    IElement current = enumerator.Current;
                    list.AddRange(current.Chunks);
                }

                return list;
            }
        }

        public virtual Paragraph Title
        {
            get
            {
                return ConstructTitle(title, numbers, numberDepth, numberStyle);
            }
            set
            {
                title = value;
            }
        }

        public virtual int NumberStyle
        {
            get
            {
                return numberStyle;
            }
            set
            {
                numberStyle = value;
            }
        }

        public virtual int NumberDepth
        {
            get
            {
                return numberDepth;
            }
            set
            {
                numberDepth = value;
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

        public virtual float Indentation
        {
            get
            {
                return indentation;
            }
            set
            {
                indentation = value;
            }
        }

        public virtual int Depth => numbers.Count;

        public virtual bool BookmarkOpen
        {
            get
            {
                return bookmarkOpen;
            }
            set
            {
                bookmarkOpen = value;
            }
        }

        public virtual string BookmarkTitle
        {
            set
            {
                bookmarkTitle = value;
            }
        }

        public virtual bool TriggerNewPage
        {
            get
            {
                if (triggerNewPage)
                {
                    return notAddedYet;
                }

                return false;
            }
            set
            {
                triggerNewPage = value;
            }
        }

        public virtual bool NotAddedYet
        {
            get
            {
                return notAddedYet;
            }
            set
            {
                notAddedYet = value;
            }
        }

        protected virtual bool AddedCompletely
        {
            get
            {
                return addedCompletely;
            }
            set
            {
                addedCompletely = value;
            }
        }

        public virtual bool ElementComplete
        {
            get
            {
                return complete;
            }
            set
            {
                complete = value;
            }
        }

        public virtual PdfName Role
        {
            get
            {
                return title.Role;
            }
            set
            {
                title.Role = value;
            }
        }

        public virtual AccessibleElementId ID
        {
            get
            {
                return title.ID;
            }
            set
            {
                title.ID = value;
            }
        }

        public virtual bool IsInline => false;

        protected internal Section()
        {
            title = new Paragraph();
            numberDepth = 1;
            title.Role = new PdfName("H" + numberDepth);
        }

        protected internal Section(Paragraph title, int numberDepth)
        {
            this.numberDepth = numberDepth;
            this.title = title;
            if (title != null)
            {
                title.Role = new PdfName("H" + numberDepth);
            }
        }

        private void SetNumbers(int number, List<int> numbers)
        {
            this.numbers = new List<int>();
            this.numbers.Add(number);
            this.numbers.AddRange(numbers);
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        IElement current = enumerator.Current;
                        listener.Add(current);
                    }
                }

                return true;
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

        public virtual void Add(int index, IElement element)
        {
            if (AddedCompletely)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("this.largeelement.has.already.been.added.to.the.document"));
            }

            try
            {
                if (element.IsNestable())
                {
                    Insert(index, element);
                    return;
                }

                throw new Exception(element.Type.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("insertion.of.illegal.element.1", ex.Message));
            }
        }

        public new bool Add(IElement element)
        {
            try
            {
                if (element.Type == 13)
                {
                    Section section = (Section)element;
                    section.SetNumbers(++subsections, numbers);
                    base.Add((IElement)section);
                    return true;
                }

                if (element is MarkedSection && ((MarkedObject)element).element.Type == 13)
                {
                    MarkedSection markedSection = (MarkedSection)element;
                    ((Section)markedSection.element).SetNumbers(++subsections, numbers);
                    base.Add((IElement)markedSection);
                    return true;
                }

                if (element.IsNestable())
                {
                    base.Add(element);
                    return true;
                }

                throw new InvalidCastException(MessageLocalization.GetComposedMessage("you.can.t.add.a.1.to.a.section", element.Type.ToString()));
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException(MessageLocalization.GetComposedMessage("insertion.of.illegal.element.1", ex.Message));
            }
        }

        public virtual bool AddAll<T>(ICollection<T> collection) where T : IElement
        {
            if (collection.Count == 0)
            {
                return false;
            }

            foreach (T item in collection)
            {
                IElement element = item;
                Add(element);
            }

            return true;
        }

        public virtual Section AddSection(float indentation, Paragraph title, int numberDepth)
        {
            if (AddedCompletely)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("this.largeelement.has.already.been.added.to.the.document"));
            }

            Section section = new Section(title, numberDepth);
            section.Indentation = indentation;
            Add(section);
            return section;
        }

        public virtual Section AddSection(float indentation, Paragraph title)
        {
            return AddSection(indentation, title, numberDepth + 1);
        }

        public virtual Section AddSection(Paragraph title, int numberDepth)
        {
            return AddSection(0f, title, numberDepth);
        }

        public virtual MarkedSection AddMarkedSection()
        {
            MarkedSection markedSection = new MarkedSection(new Section(null, numberDepth + 1));
            Add(markedSection);
            return markedSection;
        }

        public virtual Section AddSection(Paragraph title)
        {
            return AddSection(0f, title, numberDepth + 1);
        }

        public virtual Section AddSection(float indentation, string title, int numberDepth)
        {
            return AddSection(indentation, new Paragraph(title), numberDepth);
        }

        public virtual Section AddSection(string title, int numberDepth)
        {
            return AddSection(new Paragraph(title), numberDepth);
        }

        public virtual Section AddSection(float indentation, string title)
        {
            return AddSection(indentation, new Paragraph(title));
        }

        public virtual Section AddSection(string title)
        {
            return AddSection(new Paragraph(title));
        }

        public static Paragraph ConstructTitle(Paragraph title, List<int> numbers, int numberDepth, int numberStyle)
        {
            if (title == null)
            {
                return null;
            }

            int num = Math.Min(numbers.Count, numberDepth);
            if (num < 1)
            {
                return title;
            }

            StringBuilder stringBuilder = new StringBuilder(" ");
            for (int i = 0; i < num; i++)
            {
                stringBuilder.Insert(0, ".");
                stringBuilder.Insert(0, numbers[i]);
            }

            if (numberStyle == 1)
            {
                stringBuilder.Remove(stringBuilder.Length - 2, 1);
            }

            Paragraph paragraph = new Paragraph(title);
            paragraph.Insert(0, new Chunk(stringBuilder.ToString(), title.Font));
            return paragraph;
        }

        public virtual bool IsChapter()
        {
            return Type == 16;
        }

        public virtual bool IsSection()
        {
            return Type == 13;
        }

        public virtual Paragraph GetBookmarkTitle()
        {
            if (bookmarkTitle == null)
            {
                return Title;
            }

            return new Paragraph(bookmarkTitle);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public virtual void SetChapterNumber(int number)
        {
            numbers[numbers.Count - 1] = number;
            using Enumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                IElement current = enumerator.Current;
                if (current is Section)
                {
                    ((Section)current).SetChapterNumber(number);
                }
            }
        }

        public virtual void FlushContent()
        {
            NotAddedYet = false;
            title = null;
            int num;
            for (num = 0; num < base.Count; num++)
            {
                IElement element = base[num];
                if (element is Section)
                {
                    Section section = (Section)element;
                    if (!section.ElementComplete && base.Count == 1)
                    {
                        section.FlushContent();
                        break;
                    }

                    section.AddedCompletely = true;
                }

                RemoveAt(num);
                num--;
            }
        }

        public virtual void NewPage()
        {
            Add(Chunk.NEXTPAGE);
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            return title.GetAccessibleAttribute(key);
        }

        public virtual void SetAccessibleAttribute(PdfName key, PdfObject value)
        {
            title.SetAccessibleAttribute(key, value);
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleAttributes()
        {
            return title.GetAccessibleAttributes();
        }
    }
}
