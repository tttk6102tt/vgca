using Sign.itext.pdf;

namespace Sign.itext.text.pdf.collection
{
    public class PdfCollection : PdfDictionary
    {
        public const int DETAILS = 0;

        public const int TILE = 1;

        public const int HIDDEN = 2;

        public const int CUSTOM = 3;

        public virtual string InitialDocument
        {
            set
            {
                Put(PdfName.D, new PdfString(value, null));
            }
        }

        public virtual PdfCollectionSchema Schema
        {
            get
            {
                return (PdfCollectionSchema)Get(PdfName.SCHEMA);
            }
            set
            {
                Put(PdfName.SCHEMA, value);
            }
        }

        public virtual PdfCollectionSort Sort
        {
            set
            {
                Put(PdfName.SORT, value);
            }
        }

        public PdfCollection(int type)
            : base(PdfName.COLLECTION)
        {
            switch (type)
            {
                case 1:
                    Put(PdfName.VIEW, PdfName.T);
                    break;
                case 2:
                    Put(PdfName.VIEW, PdfName.H);
                    break;
                case 3:
                    Put(PdfName.VIEW, PdfName.C);
                    break;
                default:
                    Put(PdfName.VIEW, PdfName.D);
                    break;
            }
        }
    }
}
