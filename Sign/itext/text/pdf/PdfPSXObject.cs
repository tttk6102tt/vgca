using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfPSXObject : PdfTemplate
    {
        public override PdfContentByte Duplicate => new PdfPSXObject
        {
            writer = writer,
            pdf = pdf,
            thisReference = thisReference,
            pageResources = pageResources,
            separator = separator
        };

        protected PdfPSXObject()
        {
        }

        public PdfPSXObject(PdfWriter wr)
            : base(wr)
        {
        }

        public override PdfStream GetFormXObject(int compressionLevel)
        {
            PdfStream pdfStream = new PdfStream(content.ToByteArray());
            pdfStream.Put(PdfName.TYPE, PdfName.XOBJECT);
            pdfStream.Put(PdfName.SUBTYPE, PdfName.PS);
            pdfStream.FlateCompress(compressionLevel);
            return pdfStream;
        }
    }
}
