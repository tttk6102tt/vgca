using Sign.itext.text;

namespace Sign.itext.pdf
{
    public class ListLabel : ListBody
    {
        public override PdfName Role
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

        [Obsolete]
        public virtual bool TagLabelContent
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public override bool IsInline => true;

        protected internal ListLabel(ListItem parentItem)
            : base(parentItem)
        {
            role = PdfName.LBL;
            indentation = 0f;
        }

        public virtual PdfObject GetAccessibleProperty(PdfName key)
        {
            return null;
        }

        public virtual void SetAccessibleProperty(PdfName key, PdfObject value)
        {
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleProperties()
        {
            return null;
        }
    }
}
