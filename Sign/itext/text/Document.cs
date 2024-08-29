using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;

namespace Sign.itext.text
{
    public class Document : IDocListener, IElementListener, IDisposable, IAccessibleElement
    {
        public static bool Compress = true;

        public static float WmfFontCorrection = 0.86f;

        protected List<IDocListener> listeners = new List<IDocListener>();

        protected bool open;

        protected bool close;

        protected Rectangle pageSize;

        protected float marginLeft;

        protected float marginRight;

        protected float marginTop;

        protected float marginBottom;

        protected bool marginMirroring;

        protected bool marginMirroringTopBottom;

        protected string javaScript_onLoad;

        protected string javaScript_onUnLoad;

        protected string htmlStyleClass;

        protected int pageN;

        protected int chapternumber;

        protected PdfName role = PdfName.DOCUMENT;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id = new AccessibleElementId();

        public virtual int PageCount
        {
            set
            {
                pageN = value;
                foreach (IDocListener listener in listeners)
                {
                    listener.PageCount = value;
                }
            }
        }

        public virtual int PageNumber => pageN;

        public virtual float LeftMargin => marginLeft;

        public virtual float RightMargin => marginRight;

        public virtual float TopMargin => marginTop;

        public virtual float BottomMargin => marginBottom;

        public virtual float Left => pageSize.GetLeft(marginLeft);

        public virtual float Right => pageSize.GetRight(marginRight);

        public virtual float Top => pageSize.GetTop(marginTop);

        public virtual float Bottom => pageSize.GetBottom(marginBottom);

        public virtual Rectangle PageSize => pageSize;

        public virtual string JavaScript_onLoad
        {
            get
            {
                return javaScript_onLoad;
            }
            set
            {
                javaScript_onLoad = value;
            }
        }

        public virtual string JavaScript_onUnLoad
        {
            get
            {
                return javaScript_onUnLoad;
            }
            set
            {
                javaScript_onUnLoad = value;
            }
        }

        public virtual string HtmlStyleClass
        {
            get
            {
                return htmlStyleClass;
            }
            set
            {
                htmlStyleClass = value;
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
                return id;
            }
            set
            {
                id = value;
            }
        }

        public virtual bool IsInline => false;

        public Document()
            : this(Sign.itext.text.PageSize.A4)
        {
        }

        public Document(Rectangle pageSize)
            : this(pageSize, 36f, 36f, 36f, 36f)
        {
        }

        public Document(Rectangle pageSize, float marginLeft, float marginRight, float marginTop, float marginBottom)
        {
            this.pageSize = pageSize;
            this.marginLeft = marginLeft;
            this.marginRight = marginRight;
            this.marginTop = marginTop;
            this.marginBottom = marginBottom;
        }

        public virtual void AddDocListener(IDocListener listener)
        {
            listeners.Add(listener);
            if (!(listener is IAccessibleElement))
            {
                return;
            }

            IAccessibleElement accessibleElement = (IAccessibleElement)listener;
            accessibleElement.Role = role;
            accessibleElement.ID = id;
            if (accessibleAttributes == null)
            {
                return;
            }

            foreach (PdfName key in accessibleAttributes.Keys)
            {
                accessibleElement.SetAccessibleAttribute(key, accessibleAttributes[key]);
            }
        }

        public virtual void RemoveIDocListener(IDocListener listener)
        {
            listeners.Remove(listener);
        }

        public virtual bool Add(IElement element)
        {
            if (close)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.has.been.closed.you.can.t.add.any.elements"));
            }

            if (!open && element.IsContent())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.is.not.open.yet.you.can.only.add.meta.information"));
            }

            bool flag = false;
            if (element is ChapterAutoNumber)
            {
                chapternumber = ((ChapterAutoNumber)element).SetAutomaticNumber(chapternumber);
            }

            foreach (IDocListener listener in listeners)
            {
                flag |= listener.Add(element);
            }

            if (element is ILargeElement)
            {
                ILargeElement largeElement = (ILargeElement)element;
                if (!largeElement.ElementComplete)
                {
                    largeElement.FlushContent();
                }
            }

            return flag;
        }

        public virtual void Open()
        {
            if (!close)
            {
                open = true;
            }

            foreach (IDocListener listener in listeners)
            {
                listener.SetPageSize(pageSize);
                listener.SetMargins(marginLeft, marginRight, marginTop, marginBottom);
                listener.Open();
            }
        }

        public virtual void OpenDocument()
        {
            Open();
        }

        public virtual bool SetPageSize(Rectangle pageSize)
        {
            this.pageSize = pageSize;
            foreach (IDocListener listener in listeners)
            {
                listener.SetPageSize(pageSize);
            }

            return true;
        }

        public virtual bool SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom)
        {
            this.marginLeft = marginLeft;
            this.marginRight = marginRight;
            this.marginTop = marginTop;
            this.marginBottom = marginBottom;
            foreach (IDocListener listener in listeners)
            {
                listener.SetMargins(marginLeft, marginRight, marginTop, marginBottom);
            }

            return true;
        }

        public virtual bool NewPage()
        {
            if (!open || close)
            {
                return false;
            }

            foreach (IDocListener listener in listeners)
            {
                listener.NewPage();
            }

            return true;
        }

        public virtual void ResetPageCount()
        {
            pageN = 0;
            foreach (IDocListener listener in listeners)
            {
                listener.ResetPageCount();
            }
        }

        public virtual void Close()
        {
            if (!close)
            {
                open = false;
                close = true;
            }

            foreach (IDocListener listener in listeners)
            {
                listener.Close();
            }
        }

        public virtual void CloseDocument()
        {
            Close();
        }

        public virtual bool AddHeader(string name, string content)
        {
            return Add(new Header(name, content));
        }

        public virtual bool AddTitle(string title)
        {
            return Add(new Meta(1, title));
        }

        public virtual bool AddSubject(string subject)
        {
            return Add(new Meta(2, subject));
        }

        public virtual bool AddKeywords(string keywords)
        {
            return Add(new Meta(3, keywords));
        }

        public virtual bool AddAuthor(string author)
        {
            return Add(new Meta(4, author));
        }

        public virtual bool AddCreator(string creator)
        {
            return Add(new Meta(7, creator));
        }

        public virtual bool AddProducer()
        {
            return Add(new Meta(5, Version.GetInstance().GetVersion));
        }

        public virtual bool AddLanguage(string language)
        {
            try
            {
                return Add(new Meta(8, language));
            }
            catch (DocumentException ex)
            {
                throw ex;
            }
        }

        public virtual bool AddCreationDate()
        {
            return Add(new Meta(6, DateTime.Now.ToString("ddd MMM dd HH:mm:ss zzz yyyy")));
        }

        public virtual float GetLeft(float margin)
        {
            return pageSize.GetLeft(marginLeft + margin);
        }

        public virtual float GetRight(float margin)
        {
            return pageSize.GetRight(marginRight + margin);
        }

        public virtual float GetTop(float margin)
        {
            return pageSize.GetTop(marginTop + margin);
        }

        public virtual float GetBottom(float margin)
        {
            return pageSize.GetBottom(marginBottom + margin);
        }

        public virtual bool IsOpen()
        {
            return open;
        }

        public virtual bool SetMarginMirroring(bool marginMirroring)
        {
            this.marginMirroring = marginMirroring;
            foreach (IDocListener listener in listeners)
            {
                listener.SetMarginMirroring(marginMirroring);
            }

            return true;
        }

        public virtual bool SetMarginMirroringTopBottom(bool marginMirroringTopBottom)
        {
            this.marginMirroringTopBottom = marginMirroringTopBottom;
            foreach (IDocListener listener in listeners)
            {
                listener.SetMarginMirroringTopBottom(marginMirroringTopBottom);
            }

            return true;
        }

        public virtual bool IsMarginMirroring()
        {
            return marginMirroring;
        }

        public virtual void Dispose()
        {
            if (IsOpen())
            {
                Close();
            }
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
