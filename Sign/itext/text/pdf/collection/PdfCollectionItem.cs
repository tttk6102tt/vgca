using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf.collection
{
    public class PdfCollectionItem : PdfDictionary
    {
        internal PdfCollectionSchema schema;

        public PdfCollectionItem(PdfCollectionSchema schema)
            : base(PdfName.COLLECTIONITEM)
        {
            this.schema = schema;
        }

        public virtual void AddItem(string key, string value)
        {
            PdfName key2 = new PdfName(key);
            PdfCollectionField pdfCollectionField = (PdfCollectionField)schema.Get(key2);
            Put(key2, pdfCollectionField.GetValue(value));
        }

        public virtual void AddItem(string key, PdfString value)
        {
            PdfName key2 = new PdfName(key);
            if (((PdfCollectionField)schema.Get(key2)).fieldType == 0)
            {
                Put(key2, value);
            }
        }

        public virtual void AddItem(string key, PdfDate d)
        {
            PdfName key2 = new PdfName(key);
            if (((PdfCollectionField)schema.Get(key2)).fieldType == 1)
            {
                Put(key2, d);
            }
        }

        public virtual void AddItem(string key, PdfNumber n)
        {
            PdfName key2 = new PdfName(key);
            if (((PdfCollectionField)schema.Get(key2)).fieldType == 2)
            {
                Put(key2, n);
            }
        }

        public virtual void AddItem(string key, DateTime c)
        {
            AddItem(key, new PdfDate(c));
        }

        public virtual void AddItem(string key, int i)
        {
            AddItem(key, new PdfNumber(i));
        }

        public virtual void AddItem(string key, float f)
        {
            AddItem(key, new PdfNumber(f));
        }

        public virtual void AddItem(string key, double d)
        {
            AddItem(key, new PdfNumber(d));
        }

        public virtual void SetPrefix(string key, string prefix)
        {
            PdfName key2 = new PdfName(key);
            PdfObject pdfObject = Get(key2);
            if (pdfObject == null)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("you.must.set.a.value.before.adding.a.prefix"));
            }

            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.COLLECTIONSUBITEM);
            pdfDictionary.Put(PdfName.D, pdfObject);
            pdfDictionary.Put(PdfName.P, new PdfString(prefix, "UnicodeBig"));
            Put(key2, pdfDictionary);
        }
    }
}
