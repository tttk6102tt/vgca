using Sign.itext.pdf.interfaces;
using Sign.itext.text;

namespace Sign.itext.pdf
{
    public class ListBody : IAccessibleElement
    {
        protected PdfName role = PdfName.LBODY;

        protected AccessibleElementId id = new AccessibleElementId();

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected internal ListItem parentItem;

        protected internal float indentation;

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

        protected internal ListBody(ListItem parentItem)
        {
            this.parentItem = parentItem;
        }

        public ListBody(ListItem parentItem, float indentation)
            : this(parentItem)
        {
            this.indentation = indentation;
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
