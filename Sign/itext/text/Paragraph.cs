using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.api;
using Sign.itext.text.pdf;

namespace Sign.itext.text
{
    public class Paragraph : Phrase, IIndentable, ISpaceable, IAccessibleElement
    {
        protected int alignment = -1;

        protected float indentationLeft;

        protected float indentationRight;

        private float firstLineIndent;

        protected float spacingBefore;

        protected float spacingAfter;

        private float extraParagraphSpace;

        protected bool keeptogether;

        protected PdfName role = PdfName.P;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id = new AccessibleElementId();

        public override int Type => 12;

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

        public virtual float SpacingBefore
        {
            get
            {
                return spacingBefore;
            }
            set
            {
                spacingBefore = value;
            }
        }

        public virtual float SpacingAfter
        {
            get
            {
                return spacingAfter;
            }
            set
            {
                spacingAfter = value;
            }
        }

        public virtual bool KeepTogether
        {
            get
            {
                return keeptogether;
            }
            set
            {
                keeptogether = value;
            }
        }

        public virtual float FirstLineIndent
        {
            get
            {
                return firstLineIndent;
            }
            set
            {
                firstLineIndent = value;
            }
        }

        public virtual float ExtraParagraphSpace
        {
            get
            {
                return extraParagraphSpace;
            }
            set
            {
                extraParagraphSpace = value;
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

        public Paragraph()
        {
        }

        public Paragraph(float leading)
            : base(leading)
        {
        }

        public Paragraph(Chunk chunk)
            : base(chunk)
        {
        }

        public Paragraph(float leading, Chunk chunk)
            : base(leading, chunk)
        {
        }

        public Paragraph(string str)
            : base(str)
        {
        }

        public Paragraph(string str, Font font)
            : base(str, font)
        {
        }

        public Paragraph(float leading, string str)
            : base(leading, str)
        {
        }

        public Paragraph(float leading, string str, Font font)
            : base(leading, str, font)
        {
        }

        public Paragraph(Phrase phrase)
            : base(phrase)
        {
            if (phrase is Paragraph)
            {
                Paragraph paragraph = (Paragraph)phrase;
                Alignment = paragraph.Alignment;
                IndentationLeft = paragraph.IndentationLeft;
                IndentationRight = paragraph.IndentationRight;
                FirstLineIndent = paragraph.FirstLineIndent;
                SpacingAfter = paragraph.SpacingAfter;
                SpacingBefore = paragraph.SpacingBefore;
                ExtraParagraphSpace = paragraph.ExtraParagraphSpace;
                Role = paragraph.role;
                id = paragraph.ID;
                if (paragraph.accessibleAttributes != null)
                {
                    accessibleAttributes = new Dictionary<PdfName, PdfObject>(paragraph.accessibleAttributes);
                }
            }
        }

        public virtual Paragraph CloneShallow(bool spacingBefore)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Font = Font;
            paragraph.Alignment = Alignment;
            paragraph.SetLeading(Leading, multipliedLeading);
            paragraph.IndentationLeft = IndentationLeft;
            paragraph.IndentationRight = IndentationRight;
            paragraph.FirstLineIndent = FirstLineIndent;
            paragraph.SpacingAfter = SpacingAfter;
            if (spacingBefore)
            {
                paragraph.SpacingBefore = SpacingBefore;
            }

            paragraph.ExtraParagraphSpace = ExtraParagraphSpace;
            paragraph.Role = Role;
            paragraph.id = ID;
            if (accessibleAttributes != null)
            {
                paragraph.accessibleAttributes = new Dictionary<PdfName, PdfObject>(accessibleAttributes);
            }

            paragraph.TabSettings = TabSettings;
            paragraph.KeepTogether = KeepTogether;
            return paragraph;
        }

        [Obsolete]
        public virtual Paragraph cloneShallow(bool spacingBefore)
        {
            return CloneShallow(spacingBefore);
        }

        public virtual IList<IElement> BreakUp()
        {
            IList<IElement> list = new List<IElement>();
            Paragraph paragraph = null;
            using (Enumerator enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IElement current = enumerator.Current;
                    if (current.Type == 14 || current.Type == 23 || current.Type == 12)
                    {
                        if (paragraph != null && paragraph.Count > 0)
                        {
                            paragraph.SpacingAfter = 0f;
                            list.Add(paragraph);
                            paragraph = CloneShallow(spacingBefore: false);
                        }

                        if (list.Count == 0)
                        {
                            switch (current.Type)
                            {
                                case 23:
                                    ((PdfPTable)current).SpacingBefore = SpacingBefore;
                                    break;
                                case 12:
                                    ((Paragraph)current).SpacingBefore = SpacingBefore;
                                    break;
                                case 14:
                                    {
                                        ListItem firstItem = ((List)current).GetFirstItem();
                                        if (firstItem != null)
                                        {
                                            firstItem.SpacingBefore = SpacingBefore;
                                        }

                                        break;
                                    }
                            }
                        }

                        list.Add(current);
                    }
                    else
                    {
                        if (paragraph == null)
                        {
                            paragraph = CloneShallow(list.Count == 0);
                        }

                        paragraph.Add(current);
                    }
                }
            }

            if (paragraph != null && paragraph.Count > 0)
            {
                list.Add(paragraph);
            }

            if (list.Count != 0)
            {
                IElement element = list[list.Count - 1];
                switch (element.Type)
                {
                    case 23:
                        ((PdfPTable)element).SpacingAfter = SpacingAfter;
                        break;
                    case 12:
                        ((Paragraph)element).SpacingAfter = SpacingAfter;
                        break;
                    case 14:
                        {
                            ListItem lastItem = ((List)element).GetLastItem();
                            if (lastItem != null)
                            {
                                lastItem.SpacingAfter = SpacingAfter;
                            }

                            break;
                        }
                }
            }

            return list;
        }

        [Obsolete]
        public IList<IElement> breakUp()
        {
            return BreakUp();
        }

        public override bool Add(IElement o)
        {
            if (o is List)
            {
                List list = (List)o;
                list.IndentationLeft += indentationLeft;
                list.IndentationRight = indentationRight;
                base.Add(list);
                return true;
            }

            if (o is Image)
            {
                base.AddSpecial((Image)o);
                return true;
            }

            if (o is Paragraph)
            {
                base.AddSpecial(o);
                return true;
            }

            base.Add(o);
            return true;
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
