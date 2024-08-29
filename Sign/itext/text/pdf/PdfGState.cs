using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfGState : PdfDictionary
    {
        public static PdfName BM_NORMAL = new PdfName("Normal");

        public static PdfName BM_COMPATIBLE = new PdfName("Compatible");

        public static PdfName BM_MULTIPLY = new PdfName("Multiply");

        public static PdfName BM_SCREEN = new PdfName("Screen");

        public static PdfName BM_OVERLAY = new PdfName("Overlay");

        public static PdfName BM_DARKEN = new PdfName("Darken");

        public static PdfName BM_LIGHTEN = new PdfName("Lighten");

        public static PdfName BM_COLORDODGE = new PdfName("ColorDodge");

        public static PdfName BM_COLORBURN = new PdfName("ColorBurn");

        public static PdfName BM_HARDLIGHT = new PdfName("HardLight");

        public static PdfName BM_SOFTLIGHT = new PdfName("SoftLight");

        public static PdfName BM_DIFFERENCE = new PdfName("Difference");

        public static PdfName BM_EXCLUSION = new PdfName("Exclusion");

        public virtual bool OverPrintStroking
        {
            set
            {
                Put(PdfName.OP, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }

        public virtual bool OverPrintNonStroking
        {
            set
            {
                Put(PdfName.op_, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }

        public virtual int OverPrintMode
        {
            set
            {
                Put(PdfName.OPM, new PdfNumber((value != 0) ? 1 : 0));
            }
        }

        public virtual float StrokeOpacity
        {
            set
            {
                Put(PdfName.CA, new PdfNumber(value));
            }
        }

        public virtual float FillOpacity
        {
            set
            {
                Put(PdfName.ca, new PdfNumber(value));
            }
        }

        public virtual bool AlphaIsShape
        {
            set
            {
                Put(PdfName.AIS, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }

        public virtual bool TextKnockout
        {
            set
            {
                Put(PdfName.TK, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }

        public virtual PdfName BlendMode
        {
            set
            {
                Put(PdfName.BM, value);
            }
        }

        public virtual PdfName RenderingIntent
        {
            set
            {
                Put(PdfName.RI, value);
            }
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 6, this);
            base.ToPdf(writer, os);
        }
    }
}
