using Sign.itext.error_messages;
using Sign.itext.text;
using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public sealed class PdfPatternPainter : PdfTemplate
    {
        internal float xstep;

        internal float ystep;

        internal bool stencil;

        internal BaseColor defaultColor;

        public float XStep
        {
            get
            {
                return xstep;
            }
            set
            {
                xstep = value;
            }
        }

        public float YStep
        {
            get
            {
                return ystep;
            }
            set
            {
                ystep = value;
            }
        }

        public override PdfContentByte Duplicate => new PdfPatternPainter
        {
            writer = writer,
            pdf = pdf,
            thisReference = thisReference,
            pageResources = pageResources,
            bBox = new Rectangle(bBox),
            xstep = xstep,
            ystep = ystep,
            matrix = matrix,
            stencil = stencil,
            defaultColor = defaultColor
        };

        public BaseColor DefaultColor => defaultColor;

        private PdfPatternPainter()
        {
            type = 3;
        }

        internal PdfPatternPainter(PdfWriter wr)
            : base(wr)
        {
            type = 3;
        }

        internal PdfPatternPainter(PdfWriter wr, BaseColor defaultColor)
            : this(wr)
        {
            stencil = true;
            if (defaultColor == null)
            {
                this.defaultColor = BaseColor.GRAY;
            }
            else
            {
                this.defaultColor = defaultColor;
            }
        }

        public bool IsStencil()
        {
            return stencil;
        }

        public void SetPatternMatrix(float a, float b, float c, float d, float e, float f)
        {
            SetMatrix(a, b, c, d, e, f);
        }

        public PdfPattern GetPattern()
        {
            return new PdfPattern(this);
        }

        public PdfPattern GetPattern(int compressionLevel)
        {
            return new PdfPattern(this, compressionLevel);
        }

        public override void SetGrayFill(float gray)
        {
            CheckNoColor();
            base.SetGrayFill(gray);
        }

        public override void ResetGrayFill()
        {
            CheckNoColor();
            base.ResetGrayFill();
        }

        public override void SetGrayStroke(float gray)
        {
            CheckNoColor();
            base.SetGrayStroke(gray);
        }

        public override void ResetGrayStroke()
        {
            CheckNoColor();
            base.ResetGrayStroke();
        }

        public override void SetRGBColorFillF(float red, float green, float blue)
        {
            CheckNoColor();
            base.SetRGBColorFillF(red, green, blue);
        }

        public override void ResetRGBColorFill()
        {
            CheckNoColor();
            base.ResetRGBColorFill();
        }

        public override void SetRGBColorStrokeF(float red, float green, float blue)
        {
            CheckNoColor();
            base.SetRGBColorStrokeF(red, green, blue);
        }

        public override void ResetRGBColorStroke()
        {
            CheckNoColor();
            base.ResetRGBColorStroke();
        }

        public override void SetCMYKColorFillF(float cyan, float magenta, float yellow, float black)
        {
            CheckNoColor();
            base.SetCMYKColorFillF(cyan, magenta, yellow, black);
        }

        public override void ResetCMYKColorFill()
        {
            CheckNoColor();
            base.ResetCMYKColorFill();
        }

        public override void SetCMYKColorStrokeF(float cyan, float magenta, float yellow, float black)
        {
            CheckNoColor();
            base.SetCMYKColorStrokeF(cyan, magenta, yellow, black);
        }

        public override void ResetCMYKColorStroke()
        {
            CheckNoColor();
            base.ResetCMYKColorStroke();
        }

        public override void AddImage(Image image, float a, float b, float c, float d, float e, float f)
        {
            if (stencil && !image.IsMask())
            {
                CheckNoColor();
            }

            base.AddImage(image, a, b, c, d, e, f);
        }

        public override void SetCMYKColorFill(int cyan, int magenta, int yellow, int black)
        {
            CheckNoColor();
            base.SetCMYKColorFill(cyan, magenta, yellow, black);
        }

        public override void SetCMYKColorStroke(int cyan, int magenta, int yellow, int black)
        {
            CheckNoColor();
            base.SetCMYKColorStroke(cyan, magenta, yellow, black);
        }

        public override void SetRGBColorFill(int red, int green, int blue)
        {
            CheckNoColor();
            base.SetRGBColorFill(red, green, blue);
        }

        public override void SetRGBColorStroke(int red, int green, int blue)
        {
            CheckNoColor();
            base.SetRGBColorStroke(red, green, blue);
        }

        public override void SetColorStroke(BaseColor color)
        {
            CheckNoColor();
            base.SetColorStroke(color);
        }

        public override void SetColorFill(BaseColor color)
        {
            CheckNoColor();
            base.SetColorFill(color);
        }

        public override void SetColorFill(PdfSpotColor sp, float tint)
        {
            CheckNoColor();
            base.SetColorFill(sp, tint);
        }

        public override void SetColorStroke(PdfSpotColor sp, float tint)
        {
            CheckNoColor();
            base.SetColorStroke(sp, tint);
        }

        public override void SetPatternFill(PdfPatternPainter p)
        {
            CheckNoColor();
            base.SetPatternFill(p);
        }

        public override void SetPatternFill(PdfPatternPainter p, BaseColor color, float tint)
        {
            CheckNoColor();
            base.SetPatternFill(p, color, tint);
        }

        public override void SetPatternStroke(PdfPatternPainter p, BaseColor color, float tint)
        {
            CheckNoColor();
            base.SetPatternStroke(p, color, tint);
        }

        public override void SetPatternStroke(PdfPatternPainter p)
        {
            CheckNoColor();
            base.SetPatternStroke(p);
        }

        internal void CheckNoColor()
        {
            if (stencil)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("colors.are.not.allowed.in.uncolored.tile.patterns"));
            }
        }
    }
}
