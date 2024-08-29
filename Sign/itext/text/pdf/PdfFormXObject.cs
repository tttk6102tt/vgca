using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfFormXObject : PdfStream
    {
        public static PdfNumber ZERO = new PdfNumber(0);

        public static PdfNumber ONE = new PdfNumber(1);

        public static PdfLiteral MATRIX = new PdfLiteral("[1 0 0 1 0 0]");

        internal PdfFormXObject(PdfTemplate template, int compressionLevel)
        {
            Put(PdfName.TYPE, PdfName.XOBJECT);
            Put(PdfName.SUBTYPE, PdfName.FORM);
            Put(PdfName.RESOURCES, template.Resources);
            Put(PdfName.BBOX, new PdfRectangle(template.BoundingBox));
            Put(PdfName.FORMTYPE, ONE);
            PdfArray matrix = template.Matrix;
            if (template.Layer != null)
            {
                Put(PdfName.OC, template.Layer.Ref);
            }

            if (template.Group != null)
            {
                Put(PdfName.GROUP, template.Group);
            }

            if (matrix == null)
            {
                Put(PdfName.MATRIX, MATRIX);
            }
            else
            {
                Put(PdfName.MATRIX, matrix);
            }

            bytes = template.ToPdf(null);
            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
            if (template.Additional != null)
            {
                Merge(template.Additional);
            }

            FlateCompress(compressionLevel);
        }
    }
}
