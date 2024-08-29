using Sign.itext.text.api;

namespace Sign.itext.text
{
    public class MarkedSection : MarkedObject, IIndentable
    {
        protected MarkedObject title;

        public virtual MarkedObject Title
        {
            get
            {
                return new MarkedObject(Section.ConstructTitle((Paragraph)title.element, ((Section)element).numbers, ((Section)element).NumberDepth, ((Section)element).NumberStyle))
                {
                    markupAttributes = title.MarkupAttributes
                };
            }
            set
            {
                if (value.element is Paragraph)
                {
                    title = value;
                }
            }
        }

        public virtual int NumberDepth
        {
            set
            {
                ((Section)element).NumberDepth = value;
            }
        }

        public virtual float IndentationLeft
        {
            get
            {
                return ((Section)element).IndentationLeft;
            }
            set
            {
                ((Section)element).IndentationLeft = value;
            }
        }

        public virtual float IndentationRight
        {
            get
            {
                return ((Section)element).IndentationRight;
            }
            set
            {
                ((Section)element).IndentationRight = value;
            }
        }

        public virtual float Indentation
        {
            set
            {
                ((Section)element).Indentation = value;
            }
        }

        public virtual bool BookmarkOpen
        {
            set
            {
                ((Section)element).BookmarkOpen = value;
            }
        }

        public virtual bool TriggerNewPage
        {
            set
            {
                ((Section)element).TriggerNewPage = value;
            }
        }

        public virtual string BookmarkTitle
        {
            set
            {
                ((Section)element).BookmarkTitle = value;
            }
        }

        public MarkedSection(Section section)
        {
            if (section.Title != null)
            {
                title = new MarkedObject(section.Title);
                section.Title = null;
            }

            element = section;
        }

        public virtual void Add(int index, IElement o)
        {
            ((Section)element).Add(index, o);
        }

        public virtual bool Add(IElement o)
        {
            return ((Section)element).Add(o);
        }

        public override bool Process(IElementListener listener)
        {
            try
            {
                foreach (IElement item in (Section)element)
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

        public virtual bool AddAll<T>(ICollection<T> collection) where T : IElement
        {
            return ((Section)element).AddAll(collection);
        }

        public virtual MarkedSection AddSection(float indentation, int numberDepth)
        {
            MarkedSection markedSection = ((Section)element).AddMarkedSection();
            markedSection.Indentation = indentation;
            markedSection.NumberDepth = numberDepth;
            return markedSection;
        }

        public virtual MarkedSection AddSection(float indentation)
        {
            MarkedSection markedSection = ((Section)element).AddMarkedSection();
            markedSection.Indentation = indentation;
            return markedSection;
        }

        public virtual MarkedSection AddSection(int numberDepth)
        {
            MarkedSection markedSection = ((Section)element).AddMarkedSection();
            markedSection.NumberDepth = numberDepth;
            return markedSection;
        }

        public virtual MarkedSection AddSection()
        {
            return ((Section)element).AddMarkedSection();
        }

        public virtual void NewPage()
        {
            ((Section)element).NewPage();
        }
    }
}
