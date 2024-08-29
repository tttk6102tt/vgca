using Sign.itext.pdf;

namespace Sign.itext.text.pdf.collection
{
    public class PdfCollectionSchema : PdfDictionary
    {
        public PdfCollectionSchema()
            : base(PdfName.COLLECTIONSCHEMA)
        {
        }

        public virtual void AddField(string name, PdfCollectionField field)
        {
            Put(new PdfName(name), field);
        }
    }
}
