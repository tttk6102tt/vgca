using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfPHeaderCell : PdfPCell
    {
        public const int NONE = 0;

        public const int ROW = 1;

        public const int COLUMN = 2;

        public const int BOTH = 3;

        protected int scope;

        protected string name;

        public virtual int Scope
        {
            get
            {
                return scope;
            }
            set
            {
                scope = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

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

        public PdfPHeaderCell()
        {
            role = PdfName.TH;
        }

        public PdfPHeaderCell(PdfPHeaderCell headerCell)
            : base(headerCell)
        {
            role = headerCell.role;
            scope = headerCell.scope;
            name = headerCell.name;
        }
    }
}
