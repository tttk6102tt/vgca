using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfShading
    {
        protected PdfDictionary shading;

        protected PdfWriter writer;

        protected int shadingType;

        protected ColorDetails colorDetails;

        protected PdfName shadingName;

        protected PdfIndirectReference shadingReference;

        protected float[] bBox;

        protected bool antiAlias;

        private BaseColor cspace;

        public virtual BaseColor ColorSpace => cspace;

        internal PdfName ShadingName => shadingName;

        internal PdfIndirectReference ShadingReference
        {
            get
            {
                if (shadingReference == null)
                {
                    shadingReference = writer.PdfIndirectReference;
                }

                return shadingReference;
            }
        }

        internal int Name
        {
            set
            {
                shadingName = new PdfName("Sh" + value);
            }
        }

        internal PdfWriter Writer => writer;

        internal ColorDetails ColorDetails => colorDetails;

        public virtual float[] BBox
        {
            get
            {
                return bBox;
            }
            set
            {
                if (value.Length != 4)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("bbox.must.be.a.4.element.array"));
                }

                bBox = value;
            }
        }

        public virtual bool AntiAlias
        {
            get
            {
                return antiAlias;
            }
            set
            {
                antiAlias = value;
            }
        }

        protected PdfShading(PdfWriter writer)
        {
            this.writer = writer;
        }

        protected virtual void SetColorSpace(BaseColor color)
        {
            cspace = color;
            int type = ExtendedColor.GetType(color);
            PdfObject value = null;
            switch (type)
            {
                case 1:
                    value = PdfName.DEVICEGRAY;
                    break;
                case 2:
                    value = PdfName.DEVICECMYK;
                    break;
                case 3:
                    {
                        SpotColor spotColor = (SpotColor)color;
                        colorDetails = writer.AddSimple(spotColor.PdfSpotColor);
                        value = colorDetails.IndirectReference;
                        break;
                    }
                case 6:
                    {
                        DeviceNColor deviceNColor = (DeviceNColor)color;
                        colorDetails = writer.AddSimple(deviceNColor.PdfDeviceNColor);
                        value = colorDetails.IndirectReference;
                        break;
                    }
                case 4:
                case 5:
                    ThrowColorSpaceError();
                    break;
                default:
                    value = PdfName.DEVICERGB;
                    break;
            }

            shading.Put(PdfName.COLORSPACE, value);
        }

        public static void ThrowColorSpaceError()
        {
            throw new ArgumentException(MessageLocalization.GetComposedMessage("a.tiling.or.shading.pattern.cannot.be.used.as.a.color.space.in.a.shading.pattern"));
        }

        public static void CheckCompatibleColors(BaseColor c1, BaseColor c2)
        {
            int type = ExtendedColor.GetType(c1);
            int type2 = ExtendedColor.GetType(c2);
            if (type != type2)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("both.colors.must.be.of.the.same.type"));
            }

            if (type == 3 && ((SpotColor)c1).PdfSpotColor != ((SpotColor)c2).PdfSpotColor)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.spot.color.must.be.the.same.only.the.tint.can.vary"));
            }

            if (type == 4 || type == 5)
            {
                ThrowColorSpaceError();
            }
        }

        public static float[] GetColorArray(BaseColor color)
        {
            switch (ExtendedColor.GetType(color))
            {
                case 1:
                    return new float[1] { ((GrayColor)color).Gray };
                case 2:
                    {
                        CMYKColor cMYKColor = (CMYKColor)color;
                        return new float[4] { cMYKColor.Cyan, cMYKColor.Magenta, cMYKColor.Yellow, cMYKColor.Black };
                    }
                case 3:
                    return new float[1] { ((SpotColor)color).Tint };
                case 6:
                    return ((DeviceNColor)color).Tints;
                case 0:
                    return new float[3]
                    {
                    (float)color.R / 255f,
                    (float)color.G / 255f,
                    (float)color.B / 255f
                    };
                default:
                    ThrowColorSpaceError();
                    return null;
            }
        }

        public static PdfShading Type1(PdfWriter writer, BaseColor colorSpace, float[] domain, float[] tMatrix, PdfFunction function)
        {
            PdfShading pdfShading = new PdfShading(writer);
            pdfShading.shading = new PdfDictionary();
            pdfShading.shadingType = 1;
            pdfShading.shading.Put(PdfName.SHADINGTYPE, new PdfNumber(pdfShading.shadingType));
            pdfShading.SetColorSpace(colorSpace);
            if (domain != null)
            {
                pdfShading.shading.Put(PdfName.DOMAIN, new PdfArray(domain));
            }

            if (tMatrix != null)
            {
                pdfShading.shading.Put(PdfName.MATRIX, new PdfArray(tMatrix));
            }

            pdfShading.shading.Put(PdfName.FUNCTION, function.Reference);
            return pdfShading;
        }

        public static PdfShading Type2(PdfWriter writer, BaseColor colorSpace, float[] coords, float[] domain, PdfFunction function, bool[] extend)
        {
            PdfShading pdfShading = new PdfShading(writer);
            pdfShading.shading = new PdfDictionary();
            pdfShading.shadingType = 2;
            pdfShading.shading.Put(PdfName.SHADINGTYPE, new PdfNumber(pdfShading.shadingType));
            pdfShading.SetColorSpace(colorSpace);
            pdfShading.shading.Put(PdfName.COORDS, new PdfArray(coords));
            if (domain != null)
            {
                pdfShading.shading.Put(PdfName.DOMAIN, new PdfArray(domain));
            }

            pdfShading.shading.Put(PdfName.FUNCTION, function.Reference);
            if (extend != null && (extend[0] || extend[1]))
            {
                PdfArray pdfArray = new PdfArray(extend[0] ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
                pdfArray.Add(extend[1] ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
                pdfShading.shading.Put(PdfName.EXTEND, pdfArray);
            }

            return pdfShading;
        }

        public static PdfShading Type3(PdfWriter writer, BaseColor colorSpace, float[] coords, float[] domain, PdfFunction function, bool[] extend)
        {
            PdfShading pdfShading = Type2(writer, colorSpace, coords, domain, function, extend);
            pdfShading.shadingType = 3;
            pdfShading.shading.Put(PdfName.SHADINGTYPE, new PdfNumber(pdfShading.shadingType));
            return pdfShading;
        }

        public static PdfShading SimpleAxial(PdfWriter writer, float x0, float y0, float x1, float y1, BaseColor startColor, BaseColor endColor, bool extendStart, bool extendEnd)
        {
            CheckCompatibleColors(startColor, endColor);
            PdfFunction function = PdfFunction.Type2(writer, new float[2] { 0f, 1f }, null, GetColorArray(startColor), GetColorArray(endColor), 1f);
            return Type2(writer, startColor, new float[4] { x0, y0, x1, y1 }, null, function, new bool[2] { extendStart, extendEnd });
        }

        public static PdfShading SimpleAxial(PdfWriter writer, float x0, float y0, float x1, float y1, BaseColor startColor, BaseColor endColor)
        {
            return SimpleAxial(writer, x0, y0, x1, y1, startColor, endColor, extendStart: true, extendEnd: true);
        }

        public static PdfShading SimpleRadial(PdfWriter writer, float x0, float y0, float r0, float x1, float y1, float r1, BaseColor startColor, BaseColor endColor, bool extendStart, bool extendEnd)
        {
            CheckCompatibleColors(startColor, endColor);
            PdfFunction function = PdfFunction.Type2(writer, new float[2] { 0f, 1f }, null, GetColorArray(startColor), GetColorArray(endColor), 1f);
            return Type3(writer, startColor, new float[6] { x0, y0, r0, x1, y1, r1 }, null, function, new bool[2] { extendStart, extendEnd });
        }

        public static PdfShading SimpleRadial(PdfWriter writer, float x0, float y0, float r0, float x1, float y1, float r1, BaseColor startColor, BaseColor endColor)
        {
            return SimpleRadial(writer, x0, y0, r0, x1, y1, r1, startColor, endColor, extendStart: true, extendEnd: true);
        }

        public virtual void AddToBody()
        {
            if (bBox != null)
            {
                shading.Put(PdfName.BBOX, new PdfArray(bBox));
            }

            if (antiAlias)
            {
                shading.Put(PdfName.ANTIALIAS, PdfBoolean.PDFTRUE);
            }

            writer.AddToBody(shading, ShadingReference);
        }
    }
}
