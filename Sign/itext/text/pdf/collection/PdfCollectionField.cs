using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf.collection
{
    public class PdfCollectionField : PdfDictionary
    {
        public const int TEXT = 0;

        public const int DATE = 1;

        public new const int NUMBER = 2;

        public const int FILENAME = 3;

        public const int DESC = 4;

        public const int MODDATE = 5;

        public const int CREATIONDATE = 6;

        public const int SIZE = 7;

        protected internal int fieldType;

        public virtual int Order
        {
            set
            {
                Put(PdfName.O, new PdfNumber(value));
            }
        }

        public virtual bool Visible
        {
            set
            {
                Put(PdfName.V, new PdfBoolean(value));
            }
        }

        public virtual bool Editable
        {
            set
            {
                Put(PdfName.E, new PdfBoolean(value));
            }
        }

        public PdfCollectionField(string name, int type)
            : base(PdfName.COLLECTIONFIELD)
        {
            Put(PdfName.N, new PdfString(name, "UnicodeBig"));
            fieldType = type;
            switch (type)
            {
                default:
                    Put(PdfName.SUBTYPE, PdfName.S);
                    break;
                case 1:
                    Put(PdfName.SUBTYPE, PdfName.D);
                    break;
                case 2:
                    Put(PdfName.SUBTYPE, PdfName.N);
                    break;
                case 3:
                    Put(PdfName.SUBTYPE, PdfName.F);
                    break;
                case 4:
                    Put(PdfName.SUBTYPE, PdfName.DESC);
                    break;
                case 5:
                    Put(PdfName.SUBTYPE, PdfName.MODDATE);
                    break;
                case 6:
                    Put(PdfName.SUBTYPE, PdfName.CREATIONDATE);
                    break;
                case 7:
                    Put(PdfName.SUBTYPE, PdfName.SIZE);
                    break;
            }
        }

        public virtual bool IsCollectionItem()
        {
            switch (fieldType)
            {
                case 0:
                case 1:
                case 2:
                    return true;
                default:
                    return false;
            }
        }

        public virtual PdfObject GetValue(string v)
        {
            return fieldType switch
            {
                0 => new PdfString(v, "UnicodeBig"),
                1 => new PdfDate(PdfDate.Decode(v)),
                2 => new PdfNumber(v),
                _ => throw new InvalidOperationException(MessageLocalization.GetComposedMessage("1.is.not.an.acceptable.value.for.the.field.2", v, Get(PdfName.N).ToString())),
            };
        }
    }
}
