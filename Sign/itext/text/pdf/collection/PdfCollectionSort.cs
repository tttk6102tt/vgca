using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf.collection
{
    public class PdfCollectionSort : PdfDictionary
    {
        public PdfCollectionSort(string key)
            : base(PdfName.COLLECTIONSORT)
        {
            Put(PdfName.S, new PdfName(key));
        }

        public PdfCollectionSort(string[] keys)
            : base(PdfName.COLLECTIONSORT)
        {
            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < keys.Length; i++)
            {
                pdfArray.Add(new PdfName(keys[i]));
            }

            Put(PdfName.S, pdfArray);
        }

        public virtual void SetSortOrder(bool ascending)
        {
            if (Get(PdfName.S) is PdfName)
            {
                Put(PdfName.A, new PdfBoolean(ascending));
                return;
            }

            throw new InvalidOperationException(MessageLocalization.GetComposedMessage("you.have.to.define.a.boolean.array.for.this.collection.sort.dictionary"));
        }

        public virtual void SetSortOrder(bool[] ascending)
        {
            PdfObject pdfObject = Get(PdfName.S);
            if (pdfObject is PdfArray)
            {
                if (((PdfArray)pdfObject).Size != ascending.Length)
                {
                    throw new InvalidOperationException(MessageLocalization.GetComposedMessage("the.number.of.booleans.in.this.array.doesn.t.correspond.with.the.number.of.fields"));
                }

                PdfArray pdfArray = new PdfArray();
                for (int i = 0; i < ascending.Length; i++)
                {
                    pdfArray.Add(new PdfBoolean(ascending[i]));
                }

                Put(PdfName.A, pdfArray);
                return;
            }

            throw new InvalidOperationException(MessageLocalization.GetComposedMessage("you.need.a.single.boolean.for.this.collection.sort.dictionary"));
        }
    }
}
