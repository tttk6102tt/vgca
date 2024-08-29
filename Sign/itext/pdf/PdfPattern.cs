using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PdfPattern : PdfStream
    {
        internal PdfPattern(PdfPatternPainter painter)
            : this(painter, -1)
        {
        }

        internal PdfPattern(PdfPatternPainter painter, int compressionLevel)
        {
            PdfNumber value = new PdfNumber(1);
            PdfArray matrix = painter.Matrix;
            if (matrix != null)
            {
                Put(PdfName.MATRIX, matrix);
            }

            Put(PdfName.TYPE, PdfName.PATTERN);
            Put(PdfName.BBOX, new PdfRectangle(painter.BoundingBox));
            Put(PdfName.RESOURCES, painter.Resources);
            Put(PdfName.TILINGTYPE, value);
            Put(PdfName.PATTERNTYPE, value);
            if (painter.IsStencil())
            {
                Put(PdfName.PAINTTYPE, new PdfNumber(2));
            }
            else
            {
                Put(PdfName.PAINTTYPE, value);
            }

            Put(PdfName.XSTEP, new PdfNumber(painter.XStep));
            Put(PdfName.YSTEP, new PdfNumber(painter.YStep));
            bytes = painter.ToPdf(null);
            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
            FlateCompress(compressionLevel);
        }
    }
}
