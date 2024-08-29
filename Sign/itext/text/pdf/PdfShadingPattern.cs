using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfShadingPattern : PdfDictionary
    {
        protected PdfShading shading;

        protected PdfWriter writer;

        protected float[] matrix = new float[6] { 1f, 0f, 0f, 1f, 0f, 0f };

        protected PdfName patternName;

        protected PdfIndirectReference patternReference;

        internal PdfName PatternName => patternName;

        internal PdfName ShadingName => shading.ShadingName;

        internal PdfIndirectReference PatternReference
        {
            get
            {
                if (patternReference == null)
                {
                    patternReference = writer.PdfIndirectReference;
                }

                return patternReference;
            }
        }

        internal PdfIndirectReference ShadingReference => shading.ShadingReference;

        internal int Name
        {
            set
            {
                patternName = new PdfName("P" + value);
            }
        }

        public virtual float[] Matrix
        {
            get
            {
                return matrix;
            }
            set
            {
                if (value.Length != 6)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("the.matrix.size.must.be.6"));
                }

                matrix = value;
            }
        }

        public virtual PdfShading Shading => shading;

        internal ColorDetails ColorDetails => shading.ColorDetails;

        public PdfShadingPattern(PdfShading shading)
        {
            writer = shading.Writer;
            Put(PdfName.PATTERNTYPE, new PdfNumber(2));
            this.shading = shading;
        }

        public virtual void AddToBody()
        {
            Put(PdfName.SHADING, ShadingReference);
            Put(PdfName.MATRIX, new PdfArray(matrix));
            writer.AddToBody(this, PatternReference);
        }
    }
}
