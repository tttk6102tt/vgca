using Sign.itext.awt.geom;
using Sign.itext.error_messages;
using Sign.itext.pdf.interfaces;
using Sign.itext.text;
using Sign.itext.text.exceptions;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.intern;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;

namespace Sign.itext.pdf
{
    public class PdfContentByte
    {
        public class GraphicState
        {
            internal FontDetails fontDetails;

            internal ColorDetails colorDetails;

            internal float size;

            protected internal float xTLM;

            protected internal float yTLM;

            internal float aTLM = 1f;

            internal float bTLM;

            internal float cTLM;

            internal float dTLM = 1f;

            internal float tx;

            protected internal float leading;

            protected internal float scale = 100f;

            protected internal float charSpace;

            protected internal float wordSpace;

            protected internal BaseColor textColorFill = new GrayColor(0);

            protected internal BaseColor colorFill = new GrayColor(0);

            protected internal BaseColor textColorStroke = new GrayColor(0);

            protected internal BaseColor colorStroke = new GrayColor(0);

            protected internal int textRenderMode;

            protected internal AffineTransform CTM = new AffineTransform();

            protected internal PdfObject extGState;

            internal GraphicState()
            {
            }

            internal GraphicState(GraphicState cp)
            {
                CopyParameters(cp);
            }

            internal void CopyParameters(GraphicState cp)
            {
                fontDetails = cp.fontDetails;
                colorDetails = cp.colorDetails;
                size = cp.size;
                xTLM = cp.xTLM;
                yTLM = cp.yTLM;
                aTLM = cp.aTLM;
                bTLM = cp.bTLM;
                cTLM = cp.cTLM;
                dTLM = cp.dTLM;
                tx = cp.tx;
                leading = cp.leading;
                scale = cp.scale;
                charSpace = cp.charSpace;
                wordSpace = cp.wordSpace;
                textColorFill = cp.textColorFill;
                colorFill = cp.colorFill;
                textColorStroke = cp.textColorStroke;
                colorStroke = cp.colorStroke;
                CTM = (AffineTransform)cp.CTM.Clone();
                textRenderMode = cp.textRenderMode;
                extGState = cp.extGState;
            }

            internal void Restore(GraphicState restore)
            {
                CopyParameters(restore);
            }
        }

        private class UncoloredPattern : PatternColor
        {
            protected internal BaseColor color;

            protected internal float tint;

            protected internal UncoloredPattern(PdfPatternPainter p, BaseColor color, float tint)
                : base(p)
            {
                this.color = color;
                this.tint = tint;
            }

            public override bool Equals(object obj)
            {
                if (obj is UncoloredPattern && ((UncoloredPattern)obj).Painter.Equals(Painter) && ((UncoloredPattern)obj).color.Equals(color))
                {
                    return ((UncoloredPattern)obj).tint == tint;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public const int ALIGN_CENTER = 1;

        public const int ALIGN_LEFT = 0;

        public const int ALIGN_RIGHT = 2;

        public const int LINE_CAP_BUTT = 0;

        public const int LINE_CAP_ROUND = 1;

        public const int LINE_CAP_PROJECTING_SQUARE = 2;

        public const int LINE_JOIN_MITER = 0;

        public const int LINE_JOIN_ROUND = 1;

        public const int LINE_JOIN_BEVEL = 2;

        public const int TEXT_RENDER_MODE_FILL = 0;

        public const int TEXT_RENDER_MODE_STROKE = 1;

        public const int TEXT_RENDER_MODE_FILL_STROKE = 2;

        public const int TEXT_RENDER_MODE_INVISIBLE = 3;

        public const int TEXT_RENDER_MODE_FILL_CLIP = 4;

        public const int TEXT_RENDER_MODE_STROKE_CLIP = 5;

        public const int TEXT_RENDER_MODE_FILL_STROKE_CLIP = 6;

        public const int TEXT_RENDER_MODE_CLIP = 7;

        private static float[] unitRect;

        protected ByteBuffer content = new ByteBuffer();

        protected int markedContentSize;

        protected internal PdfWriter writer;

        protected internal PdfDocument pdf;

        protected GraphicState state = new GraphicState();

        protected List<int> layerDepth;

        protected List<GraphicState> stateList = new List<GraphicState>();

        protected int separator = 10;

        private int mcDepth;

        private bool inText;

        private IList<IAccessibleElement> mcElements = new List<IAccessibleElement>();

        protected internal PdfContentByte duplicatedFrom;

        private static Dictionary<PdfName, string> abrev;

        public virtual ByteBuffer InternalBuffer => content;

        public virtual float XTLM => state.xTLM;

        public virtual float YTLM => state.yTLM;

        public virtual float CharacterSpacing => state.charSpace;

        public virtual float WordSpacing => state.wordSpace;

        public virtual float HorizontalScaling => state.scale;

        public virtual float Leading => state.leading;

        internal int Size => GetSize(includeMarkedContentSize: true);

        public virtual PdfOutline RootOutline
        {
            get
            {
                CheckWriter();
                return pdf.RootOutline;
            }
        }

        public virtual PdfWriter PdfWriter => writer;

        public virtual PdfDocument PdfDocument => pdf;

        public virtual PdfContentByte Duplicate => new PdfContentByte(writer)
        {
            duplicatedFrom = this
        };

        internal virtual PageResources PageResources => pdf.PageResources;

        protected virtual PdfIndirectReference CurrentPage => writer.CurrentPage;

        protected internal virtual bool InText => inText;

        static PdfContentByte()
        {
            unitRect = new float[8] { 0f, 0f, 0f, 1f, 1f, 0f, 1f, 1f };
            abrev = new Dictionary<PdfName, string>();
            abrev[PdfName.BITSPERCOMPONENT] = "/BPC ";
            abrev[PdfName.COLORSPACE] = "/CS ";
            abrev[PdfName.DECODE] = "/D ";
            abrev[PdfName.DECODEPARMS] = "/DP ";
            abrev[PdfName.FILTER] = "/F ";
            abrev[PdfName.HEIGHT] = "/H ";
            abrev[PdfName.IMAGEMASK] = "/IM ";
            abrev[PdfName.INTENT] = "/Intent ";
            abrev[PdfName.INTERPOLATE] = "/I ";
            abrev[PdfName.WIDTH] = "/W ";
        }

        public PdfContentByte(PdfWriter wr)
        {
            if (wr != null)
            {
                writer = wr;
                pdf = writer.PdfDocument;
            }
        }

        public override string ToString()
        {
            return content.ToString();
        }

        public virtual bool IsTagged()
        {
            if (writer != null)
            {
                return writer.IsTagged();
            }

            return false;
        }

        public virtual byte[] ToPdf(PdfWriter writer)
        {
            SanityCheck();
            return content.ToByteArray();
        }

        public virtual void Add(PdfContentByte other)
        {
            if (other.writer != null && writer != other.writer)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("inconsistent.writers.are.you.mixing.two.documents"));
            }

            content.Append(other.content);
            markedContentSize += other.markedContentSize;
        }

        public virtual void SetLeading(float v)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.leading = v;
            content.Append(v).Append(" TL").Append_i(separator);
        }

        public virtual void SetFlatness(float value)
        {
            if (value >= 0f && value <= 100f)
            {
                content.Append(value).Append(" i").Append_i(separator);
            }
        }

        public virtual void SetLineCap(int value)
        {
            if (value >= 0 && value <= 2)
            {
                content.Append(value).Append(" J").Append_i(separator);
            }
        }

        public virtual void SetRenderingIntent(PdfName ri)
        {
            content.Append(ri.GetBytes()).Append(" ri").Append_i(separator);
        }

        public virtual void SetLineDash(float value)
        {
            content.Append("[] ").Append(value).Append(" d")
                .Append_i(separator);
        }

        public virtual void SetLineDash(float unitsOn, float phase)
        {
            content.Append('[').Append(unitsOn).Append("] ")
                .Append(phase)
                .Append(" d")
                .Append_i(separator);
        }

        public virtual void SetLineDash(float unitsOn, float unitsOff, float phase)
        {
            content.Append('[').Append(unitsOn).Append(' ')
                .Append(unitsOff)
                .Append("] ")
                .Append(phase)
                .Append(" d")
                .Append_i(separator);
        }

        public void SetLineDash(float[] array, float phase)
        {
            content.Append('[');
            for (int i = 0; i < array.Length; i++)
            {
                content.Append(array[i]);
                if (i < array.Length - 1)
                {
                    content.Append(' ');
                }
            }

            content.Append("] ").Append(phase).Append(" d")
                .Append_i(separator);
        }

        public virtual void SetLineJoin(int value)
        {
            if (value >= 0 && value <= 2)
            {
                content.Append(value).Append(" j").Append_i(separator);
            }
        }

        public virtual void SetLineWidth(float value)
        {
            content.Append(value).Append(" w").Append_i(separator);
        }

        public virtual void SetMiterLimit(float value)
        {
            if (value > 1f)
            {
                content.Append(value).Append(" M").Append_i(separator);
            }
        }

        public virtual void Clip()
        {
            if (inText && IsTagged())
            {
                EndText();
            }

            content.Append('W').Append_i(separator);
        }

        public virtual void EoClip()
        {
            if (inText && IsTagged())
            {
                EndText();
            }

            content.Append("W*").Append_i(separator);
        }

        public virtual void SetGrayFill(float value)
        {
            SaveColor(new GrayColor(value), fill: true);
            content.Append(value).Append(" g").Append_i(separator);
        }

        public virtual void ResetGrayFill()
        {
            SaveColor(new GrayColor(0), fill: true);
            content.Append("0 g").Append_i(separator);
        }

        public virtual void SetGrayStroke(float value)
        {
            SaveColor(new GrayColor(value), fill: false);
            content.Append(value).Append(" G").Append_i(separator);
        }

        public virtual void ResetGrayStroke()
        {
            SaveColor(new GrayColor(0), fill: false);
            content.Append("0 G").Append_i(separator);
        }

        private void HelperRGB(float red, float green, float blue)
        {
            if (red < 0f)
            {
                red = 0f;
            }
            else if (red > 1f)
            {
                red = 1f;
            }

            if (green < 0f)
            {
                green = 0f;
            }
            else if (green > 1f)
            {
                green = 1f;
            }

            if (blue < 0f)
            {
                blue = 0f;
            }
            else if (blue > 1f)
            {
                blue = 1f;
            }

            content.Append(red).Append(' ').Append(green)
                .Append(' ')
                .Append(blue);
        }

        public virtual void SetRGBColorFillF(float red, float green, float blue)
        {
            SaveColor(new BaseColor(red, green, blue), fill: true);
            HelperRGB(red, green, blue);
            content.Append(" rg").Append_i(separator);
        }

        public virtual void ResetRGBColorFill()
        {
            ResetGrayFill();
        }

        public virtual void SetRGBColorStrokeF(float red, float green, float blue)
        {
            SaveColor(new BaseColor(red, green, blue), fill: false);
            HelperRGB(red, green, blue);
            content.Append(" RG").Append_i(separator);
        }

        public virtual void ResetRGBColorStroke()
        {
            ResetGrayStroke();
        }

        private void HelperCMYK(float cyan, float magenta, float yellow, float black)
        {
            if (cyan < 0f)
            {
                cyan = 0f;
            }
            else if (cyan > 1f)
            {
                cyan = 1f;
            }

            if (magenta < 0f)
            {
                magenta = 0f;
            }
            else if (magenta > 1f)
            {
                magenta = 1f;
            }

            if (yellow < 0f)
            {
                yellow = 0f;
            }
            else if (yellow > 1f)
            {
                yellow = 1f;
            }

            if (black < 0f)
            {
                black = 0f;
            }
            else if (black > 1f)
            {
                black = 1f;
            }

            content.Append(cyan).Append(' ').Append(magenta)
                .Append(' ')
                .Append(yellow)
                .Append(' ')
                .Append(black);
        }

        public virtual void SetCMYKColorFillF(float cyan, float magenta, float yellow, float black)
        {
            SaveColor(new CMYKColor(cyan, magenta, yellow, black), fill: true);
            HelperCMYK(cyan, magenta, yellow, black);
            content.Append(" k").Append_i(separator);
        }

        public virtual void ResetCMYKColorFill()
        {
            SaveColor(new CMYKColor(0, 0, 0, 1), fill: true);
            content.Append("0 0 0 1 k").Append_i(separator);
        }

        public virtual void SetCMYKColorStrokeF(float cyan, float magenta, float yellow, float black)
        {
            SaveColor(new CMYKColor(cyan, magenta, yellow, black), fill: false);
            HelperCMYK(cyan, magenta, yellow, black);
            content.Append(" K").Append_i(separator);
        }

        public virtual void ResetCMYKColorStroke()
        {
            SaveColor(new CMYKColor(0, 0, 0, 1), fill: false);
            content.Append("0 0 0 1 K").Append_i(separator);
        }

        public virtual void MoveTo(float x, float y)
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            content.Append(x).Append(' ').Append(y)
                .Append(" m")
                .Append_i(separator);
        }

        public virtual void LineTo(float x, float y)
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            content.Append(x).Append(' ').Append(y)
                .Append(" l")
                .Append_i(separator);
        }

        public virtual void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            content.Append(x1).Append(' ').Append(y1)
                .Append(' ')
                .Append(x2)
                .Append(' ')
                .Append(y2)
                .Append(' ')
                .Append(x3)
                .Append(' ')
                .Append(y3)
                .Append(" c")
                .Append_i(separator);
        }

        public virtual void CurveTo(float x2, float y2, float x3, float y3)
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            content.Append(x2).Append(' ').Append(y2)
                .Append(' ')
                .Append(x3)
                .Append(' ')
                .Append(y3)
                .Append(" v")
                .Append_i(separator);
        }

        public virtual void CurveFromTo(float x1, float y1, float x3, float y3)
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            content.Append(x1).Append(' ').Append(y1)
                .Append(' ')
                .Append(x3)
                .Append(' ')
                .Append(y3)
                .Append(" y")
                .Append_i(separator);
        }

        public virtual void Circle(float x, float y, float r)
        {
            float num = 0.5523f;
            MoveTo(x + r, y);
            CurveTo(x + r, y + r * num, x + r * num, y + r, x, y + r);
            CurveTo(x - r * num, y + r, x - r, y + r * num, x - r, y);
            CurveTo(x - r, y - r * num, x - r * num, y - r, x, y - r);
            CurveTo(x + r * num, y - r, x + r, y - r * num, x + r, y);
        }

        public virtual void Rectangle(float x, float y, float w, float h)
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            content.Append(x).Append(' ').Append(y)
                .Append(' ')
                .Append(w)
                .Append(' ')
                .Append(h)
                .Append(" re")
                .Append_i(separator);
        }

        private bool CompareColors(BaseColor c1, BaseColor c2)
        {
            if (c1 == null && c2 == null)
            {
                return true;
            }

            if (c1 == null || c2 == null)
            {
                return false;
            }

            if (c1 is ExtendedColor)
            {
                return c1.Equals(c2);
            }

            return c2.Equals(c1);
        }

        public virtual void VariableRectangle(Rectangle rect)
        {
            float top = rect.Top;
            float bottom = rect.Bottom;
            float right = rect.Right;
            float left = rect.Left;
            float borderWidthTop = rect.BorderWidthTop;
            float borderWidthBottom = rect.BorderWidthBottom;
            float borderWidthRight = rect.BorderWidthRight;
            float borderWidthLeft = rect.BorderWidthLeft;
            BaseColor borderColorTop = rect.BorderColorTop;
            BaseColor borderColorBottom = rect.BorderColorBottom;
            BaseColor borderColorRight = rect.BorderColorRight;
            BaseColor borderColorLeft = rect.BorderColorLeft;
            SaveState();
            SetLineCap(0);
            SetLineJoin(0);
            float num = 0f;
            bool flag = false;
            BaseColor c = null;
            bool flag2 = false;
            BaseColor c2 = null;
            if (borderWidthTop > 0f)
            {
                SetLineWidth(num = borderWidthTop);
                flag = true;
                if (borderColorTop == null)
                {
                    ResetRGBColorStroke();
                }
                else
                {
                    SetColorStroke(borderColorTop);
                }

                c = borderColorTop;
                MoveTo(left, top - borderWidthTop / 2f);
                LineTo(right, top - borderWidthTop / 2f);
                Stroke();
            }

            if (borderWidthBottom > 0f)
            {
                if (borderWidthBottom != num)
                {
                    SetLineWidth(num = borderWidthBottom);
                }

                if (!flag || !CompareColors(c, borderColorBottom))
                {
                    flag = true;
                    if (borderColorBottom == null)
                    {
                        ResetRGBColorStroke();
                    }
                    else
                    {
                        SetColorStroke(borderColorBottom);
                    }

                    c = borderColorBottom;
                }

                MoveTo(right, bottom + borderWidthBottom / 2f);
                LineTo(left, bottom + borderWidthBottom / 2f);
                Stroke();
            }

            if (borderWidthRight > 0f)
            {
                if (borderWidthRight != num)
                {
                    SetLineWidth(num = borderWidthRight);
                }

                if (!flag || !CompareColors(c, borderColorRight))
                {
                    flag = true;
                    if (borderColorRight == null)
                    {
                        ResetRGBColorStroke();
                    }
                    else
                    {
                        SetColorStroke(borderColorRight);
                    }

                    c = borderColorRight;
                }

                bool flag3 = CompareColors(borderColorTop, borderColorRight);
                bool flag4 = CompareColors(borderColorBottom, borderColorRight);
                MoveTo(right - borderWidthRight / 2f, flag3 ? top : (top - borderWidthTop));
                LineTo(right - borderWidthRight / 2f, flag4 ? bottom : (bottom + borderWidthBottom));
                Stroke();
                if (!flag3 || !flag4)
                {
                    flag2 = true;
                    if (borderColorRight == null)
                    {
                        ResetRGBColorFill();
                    }
                    else
                    {
                        SetColorFill(borderColorRight);
                    }

                    c2 = borderColorRight;
                    if (!flag3)
                    {
                        MoveTo(right, top);
                        LineTo(right, top - borderWidthTop);
                        LineTo(right - borderWidthRight, top - borderWidthTop);
                        Fill();
                    }

                    if (!flag4)
                    {
                        MoveTo(right, bottom);
                        LineTo(right, bottom + borderWidthBottom);
                        LineTo(right - borderWidthRight, bottom + borderWidthBottom);
                        Fill();
                    }
                }
            }

            if (borderWidthLeft > 0f)
            {
                if (borderWidthLeft != num)
                {
                    SetLineWidth(borderWidthLeft);
                }

                if (!flag || !CompareColors(c, borderColorLeft))
                {
                    if (borderColorLeft == null)
                    {
                        ResetRGBColorStroke();
                    }
                    else
                    {
                        SetColorStroke(borderColorLeft);
                    }
                }

                bool flag5 = CompareColors(borderColorTop, borderColorLeft);
                bool flag6 = CompareColors(borderColorBottom, borderColorLeft);
                MoveTo(left + borderWidthLeft / 2f, flag5 ? top : (top - borderWidthTop));
                LineTo(left + borderWidthLeft / 2f, flag6 ? bottom : (bottom + borderWidthBottom));
                Stroke();
                if (!flag5 || !flag6)
                {
                    if (!flag2 || !CompareColors(c2, borderColorLeft))
                    {
                        if (borderColorLeft == null)
                        {
                            ResetRGBColorFill();
                        }
                        else
                        {
                            SetColorFill(borderColorLeft);
                        }
                    }

                    if (!flag5)
                    {
                        MoveTo(left, top);
                        LineTo(left, top - borderWidthTop);
                        LineTo(left + borderWidthLeft, top - borderWidthTop);
                        Fill();
                    }

                    if (!flag6)
                    {
                        MoveTo(left, bottom);
                        LineTo(left, bottom + borderWidthBottom);
                        LineTo(left + borderWidthLeft, bottom + borderWidthBottom);
                        Fill();
                    }
                }
            }

            RestoreState();
        }

        public virtual void Rectangle(Rectangle rectangle)
        {
            float left = rectangle.Left;
            float bottom = rectangle.Bottom;
            float right = rectangle.Right;
            float top = rectangle.Top;
            BaseColor backgroundColor = rectangle.BackgroundColor;
            if (backgroundColor != null)
            {
                SaveState();
                SetColorFill(backgroundColor);
                Rectangle(left, bottom, right - left, top - bottom);
                Fill();
                RestoreState();
            }

            if (!rectangle.HasBorders())
            {
                return;
            }

            if (rectangle.UseVariableBorders)
            {
                VariableRectangle(rectangle);
                return;
            }

            if (rectangle.BorderWidth != -1f)
            {
                SetLineWidth(rectangle.BorderWidth);
            }

            BaseColor borderColor = rectangle.BorderColor;
            if (borderColor != null)
            {
                SetColorStroke(borderColor);
            }

            if (rectangle.HasBorder(15))
            {
                Rectangle(left, bottom, right - left, top - bottom);
            }
            else
            {
                if (rectangle.HasBorder(8))
                {
                    MoveTo(right, bottom);
                    LineTo(right, top);
                }

                if (rectangle.HasBorder(4))
                {
                    MoveTo(left, bottom);
                    LineTo(left, top);
                }

                if (rectangle.HasBorder(2))
                {
                    MoveTo(left, bottom);
                    LineTo(right, bottom);
                }

                if (rectangle.HasBorder(1))
                {
                    MoveTo(left, top);
                    LineTo(right, top);
                }
            }

            Stroke();
            if (borderColor != null)
            {
                ResetRGBColorStroke();
            }
        }

        public virtual void ClosePath()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            content.Append('h').Append_i(separator);
        }

        public virtual void NewPath()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            content.Append('n').Append_i(separator);
        }

        public virtual void Stroke()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorStroke);
            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
            content.Append('S').Append_i(separator);
        }

        public virtual void ClosePathStroke()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorStroke);
            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
            content.Append('s').Append_i(separator);
        }

        public virtual void Fill()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorFill);
            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
            content.Append('f').Append_i(separator);
        }

        public virtual void EoFill()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorFill);
            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
            content.Append("f*").Append_i(separator);
        }

        public virtual void FillStroke()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorFill);
            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorStroke);
            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
            content.Append('B').Append_i(separator);
        }

        public virtual void ClosePathFillStroke()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorFill);
            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorStroke);
            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
            content.Append('b').Append_i(separator);
        }

        public virtual void EoFillStroke()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorFill);
            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorStroke);
            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
            content.Append("B*").Append_i(separator);
        }

        public virtual void ClosePathEoFillStroke()
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("path.construction.operator.inside.text.object"));
                }

                EndText();
            }

            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorFill);
            PdfWriter.CheckPdfIsoConformance(writer, 1, state.colorStroke);
            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
            content.Append("b*").Append_i(separator);
        }

        public virtual void AddImage(Image image)
        {
            AddImage(image, inlineImage: false);
        }

        public virtual void AddImage(Image image, bool inlineImage)
        {
            if (!image.HasAbsolutePosition())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.image.must.have.absolute.positioning"));
            }

            float[] matrix = image.GetMatrix();
            matrix[4] = image.AbsoluteX - matrix[4];
            matrix[5] = image.AbsoluteY - matrix[5];
            AddImage(image, matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5], inlineImage);
        }

        public virtual void AddImage(Image image, float a, float b, float c, float d, float e, float f)
        {
            AddImage(image, a, b, c, d, e, f, inlineImage: false);
        }

        public virtual void AddImage(Image image, AffineTransform transform)
        {
            double[] array = new double[6];
            transform.GetMatrix(array);
            AddImage(image, (float)array[0], (float)array[1], (float)array[2], (float)array[3], (float)array[4], (float)array[5], inlineImage: false);
        }

        [Obsolete]
        public void AddImage(Image image, Matrix transform)
        {
            float[] elements = transform.Elements;
            AddImage(image, elements[0], elements[1], elements[2], elements[3], elements[4], elements[5], inlineImage: false);
        }

        public virtual void AddImage(Image image, float a, float b, float c, float d, float e, float f, bool inlineImage)
        {
            AffineTransform affineTransform = new AffineTransform(a, b, c, d, e, f);
            if (image.Layer != null)
            {
                BeginLayer(image.Layer);
            }

            if (IsTagged())
            {
                if (inText)
                {
                    EndText();
                }

                Point2D[] src = new Point2D.Float[4]
                {
                    new Point2D.Float(0f, 0f),
                    new Point2D.Float(1f, 0f),
                    new Point2D.Float(1f, 1f),
                    new Point2D.Float(0f, 1f)
                };
                Point2D[] array = new Point2D.Float[4];
                affineTransform.Transform(src, 0, array, 0, 4);
                float num = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float num4 = float.MinValue;
                for (int i = 0; i < 4; i++)
                {
                    if (array[i].GetX() < (double)num)
                    {
                        num = (float)array[i].GetX();
                    }

                    if (array[i].GetX() > (double)num2)
                    {
                        num2 = (float)array[i].GetX();
                    }

                    if (array[i].GetY() < (double)num3)
                    {
                        num3 = (float)array[i].GetY();
                    }

                    if (array[i].GetY() > (double)num4)
                    {
                        num4 = (float)array[i].GetY();
                    }
                }

                image.SetAccessibleAttribute(PdfName.BBOX, new PdfArray(new float[4] { num, num3, num2, num4 }));
            }

            if (writer != null && image.IsImgTemplate())
            {
                writer.AddDirectImageSimple(image);
                PdfTemplate templateData = image.TemplateData;
                if (image.GetAccessibleAttributes() != null)
                {
                    foreach (PdfName key in image.GetAccessibleAttributes().Keys)
                    {
                        templateData.SetAccessibleAttribute(key, image.GetAccessibleAttribute(key));
                    }
                }

                float width = templateData.Width;
                float height = templateData.Height;
                AddTemplate(templateData, a / width, b / width, c / height, d / height, e, f);
            }
            else
            {
                content.Append("q ");
                if (!affineTransform.IsIdentity())
                {
                    content.Append(a).Append(' ');
                    content.Append(b).Append(' ');
                    content.Append(c).Append(' ');
                    content.Append(d).Append(' ');
                    content.Append(e).Append(' ');
                    content.Append(f).Append(" cm");
                }

                if (inlineImage)
                {
                    content.Append("\nBI\n");
                    PdfImage pdfImage = new PdfImage(image, "", null);
                    if (image is ImgJBIG2)
                    {
                        byte[] globalBytes = ((ImgJBIG2)image).GlobalBytes;
                        if (globalBytes != null)
                        {
                            PdfDictionary pdfDictionary = new PdfDictionary();
                            pdfDictionary.Put(PdfName.JBIG2GLOBALS, writer.GetReferenceJBIG2Globals(globalBytes));
                            pdfImage.Put(PdfName.DECODEPARMS, pdfDictionary);
                        }
                    }

                    PdfWriter.CheckPdfIsoConformance(writer, 17, pdfImage);
                    foreach (PdfName key2 in pdfImage.Keys)
                    {
                        if (!abrev.ContainsKey(key2))
                        {
                            continue;
                        }

                        PdfObject pdfObject = pdfImage.Get(key2);
                        string str = abrev[key2];
                        content.Append(str);
                        bool flag = true;
                        if (key2.Equals(PdfName.COLORSPACE) && pdfObject.IsArray())
                        {
                            PdfArray pdfArray = (PdfArray)pdfObject;
                            if (pdfArray.Size == 4 && PdfName.INDEXED.Equals(pdfArray.GetAsName(0)) && pdfArray.GetPdfObject(1).IsName() && pdfArray.GetPdfObject(2).IsNumber() && pdfArray.GetPdfObject(3).IsString())
                            {
                                flag = false;
                            }
                        }

                        if (flag && key2.Equals(PdfName.COLORSPACE) && !pdfObject.IsName())
                        {
                            PdfName colorspaceName = writer.GetColorspaceName();
                            PageResources.AddColor(colorspaceName, writer.AddToBody(pdfObject).IndirectReference);
                            pdfObject = colorspaceName;
                        }

                        pdfObject.ToPdf(null, content);
                        content.Append('\n');
                    }

                    content.Append("ID\n");
                    pdfImage.WriteContent(content);
                    content.Append("\nEI\nQ").Append_i(separator);
                }
                else
                {
                    PageResources pageResources = PageResources;
                    Image imageMask = image.ImageMask;
                    PdfName name;
                    if (imageMask != null)
                    {
                        name = writer.AddDirectImageSimple(imageMask);
                        pageResources.AddXObject(name, writer.GetImageReference(name));
                    }

                    name = writer.AddDirectImageSimple(image);
                    name = pageResources.AddXObject(name, writer.GetImageReference(name));
                    content.Append(' ').Append(name.GetBytes()).Append(" Do Q")
                        .Append_i(separator);
                }
            }

            if (image.HasBorders())
            {
                SaveState();
                float width2 = image.Width;
                float height2 = image.Height;
                ConcatCTM(a / width2, b / width2, c / height2, d / height2, e, f);
                Rectangle(image);
                RestoreState();
            }

            if (image.Layer != null)
            {
                EndLayer();
            }

            Annotation annotation = image.Annotation;
            if (annotation != null)
            {
                float[] array2 = new float[unitRect.Length];
                for (int j = 0; j < unitRect.Length; j += 2)
                {
                    array2[j] = a * unitRect[j] + c * unitRect[j + 1] + e;
                    array2[j + 1] = b * unitRect[j] + d * unitRect[j + 1] + f;
                }

                float num5 = array2[0];
                float num6 = array2[1];
                float num7 = num5;
                float num8 = num6;
                for (int k = 2; k < array2.Length; k += 2)
                {
                    num5 = Math.Min(num5, array2[k]);
                    num6 = Math.Min(num6, array2[k + 1]);
                    num7 = Math.Max(num7, array2[k]);
                    num8 = Math.Max(num8, array2[k + 1]);
                }

                annotation = new Annotation(annotation);
                annotation.SetDimensions(num5, num6, num7, num8);
                PdfAnnotation pdfAnnotation = PdfAnnotationsImp.ConvertAnnotation(writer, annotation, new Rectangle(num5, num6, num7, num8));
                if (pdfAnnotation != null)
                {
                    AddAnnotation(pdfAnnotation);
                }
            }
        }

        public virtual void Reset()
        {
            Reset(validateContent: true);
        }

        public virtual void Reset(bool validateContent)
        {
            content.Reset();
            markedContentSize = 0;
            if (validateContent)
            {
                SanityCheck();
            }

            state = new GraphicState();
            stateList = new List<GraphicState>();
        }

        protected internal virtual void BeginText(bool restoreTM)
        {
            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.begin.end.text.operators"));
                }

                return;
            }

            inText = true;
            content.Append("BT").Append_i(separator);
            if (restoreTM)
            {
                float xTLM = state.xTLM;
                float tx = state.tx;
                SetTextMatrix(state.aTLM, state.bTLM, state.cTLM, state.dTLM, state.tx, state.yTLM);
                state.xTLM = xTLM;
                state.tx = tx;
            }
            else
            {
                state.xTLM = 0f;
                state.yTLM = 0f;
                state.tx = 0f;
            }

            if (IsTagged())
            {
                try
                {
                    RestoreColor();
                }
                catch (IOException)
                {
                }
            }
        }

        public virtual void BeginText()
        {
            BeginText(restoreTM: false);
        }

        public virtual void EndText()
        {
            if (!inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.begin.end.text.operators"));
                }

                return;
            }

            inText = false;
            content.Append("ET").Append_i(separator);
            if (IsTagged())
            {
                try
                {
                    RestoreColor();
                }
                catch (IOException)
                {
                }
            }
        }

        public virtual void SaveState()
        {
            PdfWriter.CheckPdfIsoConformance(writer, 12, "q");
            if (inText && IsTagged())
            {
                EndText();
            }

            content.Append('q').Append_i(separator);
            stateList.Add(new GraphicState(state));
        }

        public virtual void RestoreState()
        {
            PdfWriter.CheckPdfIsoConformance(writer, 12, "Q");
            if (inText && IsTagged())
            {
                EndText();
            }

            content.Append('Q').Append_i(separator);
            int num = stateList.Count - 1;
            if (num < 0)
            {
                throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.save.restore.state.operators"));
            }

            state.Restore(stateList[num]);
            stateList.RemoveAt(num);
        }

        public virtual void SetCharacterSpacing(float value)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.charSpace = value;
            content.Append(value).Append(" Tc").Append_i(separator);
        }

        public virtual void SetWordSpacing(float value)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.wordSpace = value;
            content.Append(value).Append(" Tw").Append_i(separator);
        }

        public virtual void SetHorizontalScaling(float value)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.scale = value;
            content.Append(value).Append(" Tz").Append_i(separator);
        }

        public virtual void SetFontAndSize(BaseFont bf, float size)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            CheckWriter();
            if (size < 0.0001f && size > -0.0001f)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("font.size.too.small.1", size));
            }

            state.size = size;
            state.fontDetails = writer.AddSimple(bf);
            PageResources pageResources = PageResources;
            PdfName fontName = state.fontDetails.FontName;
            fontName = pageResources.AddFont(fontName, state.fontDetails.IndirectReference);
            content.Append(fontName.GetBytes()).Append(' ').Append(size)
                .Append(" Tf")
                .Append_i(separator);
        }

        public virtual void SetTextRenderingMode(int value)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.textRenderMode = value;
            content.Append(value).Append(" Tr").Append_i(separator);
        }

        public virtual void SetTextRise(float value)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            content.Append(value).Append(" Ts").Append_i(separator);
        }

        private void ShowText2(string text)
        {
            if (state.fontDetails == null)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("font.and.size.must.be.set.before.writing.any.text"));
            }

            StringUtils.EscapeString(state.fontDetails.ConvertToBytes(text), content);
        }

        public virtual void ShowText(string text)
        {
            CheckState();
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            ShowText2(text);
            UpdateTx(text, 0f);
            content.Append("Tj").Append_i(separator);
        }

        public virtual void ShowTextGid(string gids)
        {
            CheckState();
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            if (state.fontDetails == null)
            {
                throw new NullReferenceException(MessageLocalization.GetComposedMessage("font.and.size.must.be.set.before.writing.any.text"));
            }

            object[] array = state.fontDetails.ConvertToBytesGid(gids);
            StringUtils.EscapeString((byte[])array[0], content);
            state.tx += (float)((int?)array[2]).Value * 0.001f * state.size;
            content.Append("Tj").Append_i(separator);
        }

        public static PdfTextArray GetKernArray(string text, BaseFont font)
        {
            PdfTextArray pdfTextArray = new PdfTextArray();
            StringBuilder stringBuilder = new StringBuilder();
            int num = text.Length - 1;
            char[] array = text.ToCharArray();
            if (num >= 0)
            {
                stringBuilder.Append(array, 0, 1);
            }

            for (int i = 0; i < num; i++)
            {
                char c = array[i + 1];
                int kerning = font.GetKerning(array[i], c);
                if (kerning == 0)
                {
                    stringBuilder.Append(c);
                    continue;
                }

                pdfTextArray.Add(stringBuilder.ToString());
                stringBuilder.Length = 0;
                stringBuilder.Append(array, i + 1, 1);
                pdfTextArray.Add(-kerning);
            }

            pdfTextArray.Add(stringBuilder.ToString());
            return pdfTextArray;
        }

        public virtual void ShowTextKerned(string text)
        {
            if (state.fontDetails == null)
            {
                throw new ArgumentNullException(MessageLocalization.GetComposedMessage("font.and.size.must.be.set.before.writing.any.text"));
            }

            BaseFont baseFont = state.fontDetails.BaseFont;
            if (baseFont.HasKernPairs())
            {
                ShowText(GetKernArray(text, baseFont));
            }
            else
            {
                ShowText(text);
            }
        }

        public virtual void NewlineShowText(string text)
        {
            CheckState();
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.yTLM -= state.leading;
            ShowText2(text);
            content.Append('\'').Append_i(separator);
            state.tx = state.xTLM;
            UpdateTx(text, 0f);
        }

        public virtual void NewlineShowText(float wordSpacing, float charSpacing, string text)
        {
            CheckState();
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.yTLM -= state.leading;
            content.Append(wordSpacing).Append(' ').Append(charSpacing);
            ShowText2(text);
            content.Append("\"").Append_i(separator);
            state.charSpace = charSpacing;
            state.wordSpace = wordSpacing;
            state.tx = state.xTLM;
            UpdateTx(text, 0f);
        }

        public virtual void SetTextMatrix(float a, float b, float c, float d, float x, float y)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.xTLM = x;
            state.yTLM = y;
            state.aTLM = a;
            state.bTLM = b;
            state.cTLM = c;
            state.dTLM = d;
            state.tx = state.xTLM;
            content.Append(a).Append(' ').Append(b)
                .Append_i(32)
                .Append(c)
                .Append_i(32)
                .Append(d)
                .Append_i(32)
                .Append(x)
                .Append_i(32)
                .Append(y)
                .Append(" Tm")
                .Append_i(separator);
        }

        public virtual void SetTextMatrix(AffineTransform transform)
        {
            double[] array = new double[6];
            transform.GetMatrix(array);
            SetTextMatrix((float)array[0], (float)array[1], (float)array[2], (float)array[3], (float)array[4], (float)array[5]);
        }

        [Obsolete]
        public void SetTextMatrix(Matrix transform)
        {
            float[] elements = transform.Elements;
            SetTextMatrix(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
        }

        public virtual void SetTextMatrix(float x, float y)
        {
            SetTextMatrix(1f, 0f, 0f, 1f, x, y);
        }

        public virtual void MoveText(float x, float y)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.xTLM += x;
            state.yTLM += y;
            if (IsTagged() && state.xTLM != state.tx)
            {
                SetTextMatrix(state.aTLM, state.bTLM, state.cTLM, state.dTLM, state.xTLM, state.yTLM);
            }
            else
            {
                content.Append(x).Append(' ').Append(y)
                    .Append(" Td")
                    .Append_i(separator);
            }
        }

        public virtual void MoveTextWithLeading(float x, float y)
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            state.xTLM += x;
            state.yTLM += y;
            state.leading = 0f - y;
            if (IsTagged() && state.xTLM != state.tx)
            {
                SetTextMatrix(state.aTLM, state.bTLM, state.cTLM, state.dTLM, state.xTLM, state.yTLM);
            }
            else
            {
                content.Append(x).Append(' ').Append(y)
                    .Append(" TD")
                    .Append_i(separator);
            }
        }

        public virtual void NewlineText()
        {
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            if (IsTagged() && state.xTLM != state.tx)
            {
                SetTextMatrix(state.aTLM, state.bTLM, state.cTLM, state.dTLM, state.xTLM, state.yTLM);
            }

            state.yTLM -= state.leading;
            content.Append("T*").Append_i(separator);
        }

        internal int GetSize(bool includeMarkedContentSize)
        {
            if (includeMarkedContentSize)
            {
                return content.Size;
            }

            return content.Size - markedContentSize;
        }

        public virtual void AddOutline(PdfOutline outline, string name)
        {
            CheckWriter();
            pdf.AddOutline(outline, name);
        }

        public virtual float GetEffectiveStringWidth(string text, bool kerned)
        {
            BaseFont baseFont = state.fontDetails.BaseFont;
            float num = ((!kerned) ? baseFont.GetWidthPoint(text, state.size) : baseFont.GetWidthPointKerned(text, state.size));
            if (state.charSpace != 0f && text.Length > 1)
            {
                num += state.charSpace * (float)(text.Length - 1);
            }

            if (state.wordSpace != 0f && !baseFont.IsVertical())
            {
                for (int i = 0; i < text.Length - 1; i++)
                {
                    if (text[i] == ' ')
                    {
                        num += state.wordSpace;
                    }
                }
            }

            if ((double)state.scale != 100.0)
            {
                num = num * state.scale / 100f;
            }

            return num;
        }

        private float GetEffectiveStringWidth(string text, bool kerned, float kerning)
        {
            BaseFont baseFont = state.fontDetails.BaseFont;
            float num = ((!kerned) ? baseFont.GetWidthPoint(text, state.size) : baseFont.GetWidthPointKerned(text, state.size));
            if (state.charSpace != 0f && text.Length > 0)
            {
                num += state.charSpace * (float)text.Length;
            }

            if (state.wordSpace != 0f && !baseFont.IsVertical())
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ')
                    {
                        num += state.wordSpace;
                    }
                }
            }

            num -= kerning / 1000f * state.size;
            if ((double)state.scale != 100.0)
            {
                num = num * state.scale / 100f;
            }

            return num;
        }

        public virtual void ShowTextAligned(int alignment, string text, float x, float y, float rotation)
        {
            ShowTextAligned(alignment, text, x, y, rotation, kerned: false);
        }

        private void ShowTextAligned(int alignment, string text, float x, float y, float rotation, bool kerned)
        {
            if (state.fontDetails == null)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("font.and.size.must.be.set.before.writing.any.text"));
            }

            if (rotation == 0f)
            {
                switch (alignment)
                {
                    case 1:
                        x -= GetEffectiveStringWidth(text, kerned) / 2f;
                        break;
                    case 2:
                        x -= GetEffectiveStringWidth(text, kerned);
                        break;
                }

                SetTextMatrix(x, y);
                if (kerned)
                {
                    ShowTextKerned(text);
                }
                else
                {
                    ShowText(text);
                }

                return;
            }

            double num = (double)rotation * Math.PI / 180.0;
            float num2 = (float)Math.Cos(num);
            float num3 = (float)Math.Sin(num);
            switch (alignment)
            {
                case 1:
                    {
                        float effectiveStringWidth = GetEffectiveStringWidth(text, kerned) / 2f;
                        x -= effectiveStringWidth * num2;
                        y -= effectiveStringWidth * num3;
                        break;
                    }
                case 2:
                    {
                        float effectiveStringWidth = GetEffectiveStringWidth(text, kerned);
                        x -= effectiveStringWidth * num2;
                        y -= effectiveStringWidth * num3;
                        break;
                    }
            }

            SetTextMatrix(num2, num3, 0f - num3, num2, x, y);
            if (kerned)
            {
                ShowTextKerned(text);
            }
            else
            {
                ShowText(text);
            }

            SetTextMatrix(0f, 0f);
        }

        public virtual void ShowTextAlignedKerned(int alignment, string text, float x, float y, float rotation)
        {
            ShowTextAligned(alignment, text, x, y, rotation, kerned: true);
        }

        public virtual void ConcatCTM(float a, float b, float c, float d, float e, float f)
        {
            if (inText && IsTagged())
            {
                EndText();
            }

            state.CTM.Concatenate(new AffineTransform(a, b, c, d, e, f));
            content.Append(a).Append(' ').Append(b)
                .Append(' ')
                .Append(c)
                .Append(' ');
            content.Append(d).Append(' ').Append(e)
                .Append(' ')
                .Append(f)
                .Append(" cm")
                .Append_i(separator);
        }

        public virtual void ConcatCTM(AffineTransform transform)
        {
            double[] array = new double[6];
            transform.GetMatrix(array);
            ConcatCTM((float)array[0], (float)array[1], (float)array[2], (float)array[3], (float)array[4], (float)array[5]);
        }

        [Obsolete]
        public void ConcatCTM(Matrix transform)
        {
            float[] elements = transform.Elements;
            ConcatCTM(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
        }

        public static List<float[]> BezierArc(float x1, float y1, float x2, float y2, float startAng, float extent)
        {
            if (x1 > x2)
            {
                float num = x1;
                x1 = x2;
                x2 = num;
            }

            if (y2 > y1)
            {
                float num2 = y1;
                y1 = y2;
                y2 = num2;
            }

            float num3;
            int num4;
            if (Math.Abs(extent) <= 90f)
            {
                num3 = extent;
                num4 = 1;
            }
            else
            {
                num4 = (int)Math.Ceiling(Math.Abs(extent) / 90f);
                num3 = extent / (float)num4;
            }

            float num5 = (x1 + x2) / 2f;
            float num6 = (y1 + y2) / 2f;
            float num7 = (x2 - x1) / 2f;
            float num8 = (y2 - y1) / 2f;
            float num9 = (float)((double)num3 * Math.PI / 360.0);
            float num10 = (float)Math.Abs(1.3333333333333333 * (1.0 - Math.Cos(num9)) / Math.Sin(num9));
            List<float[]> list = new List<float[]>();
            for (int i = 0; i < num4; i++)
            {
                float num11 = (float)((double)(startAng + (float)i * num3) * Math.PI / 180.0);
                float num12 = (float)((double)(startAng + (float)(i + 1) * num3) * Math.PI / 180.0);
                float num13 = (float)Math.Cos(num11);
                float num14 = (float)Math.Cos(num12);
                float num15 = (float)Math.Sin(num11);
                float num16 = (float)Math.Sin(num12);
                if (num3 > 0f)
                {
                    list.Add(new float[8]
                    {
                        num5 + num7 * num13,
                        num6 - num8 * num15,
                        num5 + num7 * (num13 - num10 * num15),
                        num6 - num8 * (num15 + num10 * num13),
                        num5 + num7 * (num14 + num10 * num16),
                        num6 - num8 * (num16 - num10 * num14),
                        num5 + num7 * num14,
                        num6 - num8 * num16
                    });
                }
                else
                {
                    list.Add(new float[8]
                    {
                        num5 + num7 * num13,
                        num6 - num8 * num15,
                        num5 + num7 * (num13 + num10 * num15),
                        num6 - num8 * (num15 - num10 * num13),
                        num5 + num7 * (num14 - num10 * num16),
                        num6 - num8 * (num16 + num10 * num14),
                        num5 + num7 * num14,
                        num6 - num8 * num16
                    });
                }
            }

            return list;
        }

        public virtual void Arc(float x1, float y1, float x2, float y2, float startAng, float extent)
        {
            List<float[]> list = BezierArc(x1, y1, x2, y2, startAng, extent);
            if (list.Count != 0)
            {
                float[] array = list[0];
                MoveTo(array[0], array[1]);
                for (int i = 0; i < list.Count; i++)
                {
                    array = list[i];
                    CurveTo(array[2], array[3], array[4], array[5], array[6], array[7]);
                }
            }
        }

        public virtual void Ellipse(float x1, float y1, float x2, float y2)
        {
            Arc(x1, y1, x2, y2, 0f, 360f);
        }

        public virtual PdfPatternPainter CreatePattern(float width, float height, float xstep, float ystep)
        {
            CheckWriter();
            if (xstep == 0f || ystep == 0f)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("xstep.or.ystep.can.not.be.zero"));
            }

            PdfPatternPainter pdfPatternPainter = new PdfPatternPainter(writer);
            pdfPatternPainter.Width = width;
            pdfPatternPainter.Height = height;
            pdfPatternPainter.XStep = xstep;
            pdfPatternPainter.YStep = ystep;
            writer.AddSimplePattern(pdfPatternPainter);
            return pdfPatternPainter;
        }

        public virtual PdfPatternPainter CreatePattern(float width, float height)
        {
            return CreatePattern(width, height, width, height);
        }

        public virtual PdfPatternPainter CreatePattern(float width, float height, float xstep, float ystep, BaseColor color)
        {
            CheckWriter();
            if (xstep == 0f || ystep == 0f)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("xstep.or.ystep.can.not.be.zero"));
            }

            PdfPatternPainter pdfPatternPainter = new PdfPatternPainter(writer, color);
            pdfPatternPainter.Width = width;
            pdfPatternPainter.Height = height;
            pdfPatternPainter.XStep = xstep;
            pdfPatternPainter.YStep = ystep;
            writer.AddSimplePattern(pdfPatternPainter);
            return pdfPatternPainter;
        }

        public virtual PdfPatternPainter CreatePattern(float width, float height, BaseColor color)
        {
            return CreatePattern(width, height, width, height, color);
        }

        public virtual PdfTemplate CreateTemplate(float width, float height)
        {
            return CreateTemplate(width, height, null);
        }

        internal PdfTemplate CreateTemplate(float width, float height, PdfName forcedName)
        {
            CheckWriter();
            PdfTemplate pdfTemplate = new PdfTemplate(writer);
            pdfTemplate.Width = width;
            pdfTemplate.Height = height;
            writer.AddDirectTemplateSimple(pdfTemplate, forcedName);
            return pdfTemplate;
        }

        public virtual PdfAppearance CreateAppearance(float width, float height)
        {
            return CreateAppearance(width, height, null);
        }

        internal PdfAppearance CreateAppearance(float width, float height, PdfName forcedName)
        {
            CheckWriter();
            PdfAppearance pdfAppearance = new PdfAppearance(writer);
            pdfAppearance.Width = width;
            pdfAppearance.Height = height;
            writer.AddDirectTemplateSimple(pdfAppearance, forcedName);
            return pdfAppearance;
        }

        public virtual void AddPSXObject(PdfPSXObject psobject)
        {
            if (inText && IsTagged())
            {
                EndText();
            }

            CheckWriter();
            PdfName name = writer.AddDirectTemplateSimple(psobject, null);
            name = PageResources.AddXObject(name, psobject.IndirectReference);
            content.Append(name.GetBytes()).Append(" Do").Append_i(separator);
        }

        public virtual void AddTemplate(PdfTemplate template, float a, float b, float c, float d, float e, float f)
        {
            AddTemplate(template, a, b, c, d, e, f, tagContent: false);
        }

        public virtual void AddTemplate(PdfTemplate template, float a, float b, float c, float d, float e, float f, bool tagContent)
        {
            CheckWriter();
            CheckNoPattern(template);
            PdfWriter.CheckPdfIsoConformance(writer, 20, template);
            PdfName name = writer.AddDirectTemplateSimple(template, null);
            name = PageResources.AddXObject(name, template.IndirectReference);
            if (IsTagged())
            {
                if (inText)
                {
                    EndText();
                }

                if (template.ContentTagged || (template.PageReference != null && tagContent))
                {
                    throw new InvalidOperationException(MessageLocalization.GetComposedMessage("template.with.tagged.could.not.be.used.more.than.once"));
                }

                template.PageReference = writer.CurrentPage;
                if (tagContent)
                {
                    template.ContentTagged = true;
                    IList<IAccessibleElement> list = GetMcElements();
                    if (list != null && list.Count > 0)
                    {
                        template.GetMcElements().Add(list[list.Count - 1]);
                    }
                }
                else
                {
                    OpenMCBlock(template);
                }
            }

            content.Append("q ");
            content.Append(a).Append(' ');
            content.Append(b).Append(' ');
            content.Append(c).Append(' ');
            content.Append(d).Append(' ');
            content.Append(e).Append(' ');
            content.Append(f).Append(" cm ");
            content.Append(name.GetBytes()).Append(" Do Q").Append_i(separator);
            if (IsTagged() && !tagContent)
            {
                CloseMCBlock(template);
                template.ID = null;
            }
        }

        public virtual PdfName AddFormXObj(PdfStream formXObj, PdfName name, float a, float b, float c, float d, float e, float f)
        {
            CheckWriter();
            PdfWriter.CheckPdfIsoConformance(writer, 9, formXObj);
            PdfName pdfName = PageResources.AddXObject(name, writer.AddToBody(formXObj).IndirectReference);
            PdfArtifact element = null;
            if (IsTagged())
            {
                if (inText)
                {
                    EndText();
                }

                element = new PdfArtifact();
                OpenMCBlock(element);
            }

            content.Append("q ");
            content.Append(a).Append(' ');
            content.Append(b).Append(' ');
            content.Append(c).Append(' ');
            content.Append(d).Append(' ');
            content.Append(e).Append(' ');
            content.Append(f).Append(" cm ");
            content.Append(pdfName.GetBytes()).Append(" Do Q").Append_i(separator);
            if (IsTagged())
            {
                CloseMCBlock(element);
            }

            return pdfName;
        }

        public virtual void AddTemplate(PdfTemplate template, AffineTransform transform)
        {
            AddTemplate(template, transform, tagContent: false);
        }

        public virtual void AddTemplate(PdfTemplate template, AffineTransform transform, bool tagContent)
        {
            double[] array = new double[6];
            transform.GetMatrix(array);
            AddTemplate(template, (float)array[0], (float)array[1], (float)array[2], (float)array[3], (float)array[4], (float)array[5], tagContent);
        }

        [Obsolete]
        public void AddTemplate(PdfTemplate template, Matrix transform, bool tagContent)
        {
            float[] elements = transform.Elements;
            AddTemplate(template, elements[0], elements[1], elements[2], elements[3], elements[4], elements[5], tagContent);
        }

        [Obsolete]
        public void AddTemplate(PdfTemplate template, Matrix transform)
        {
            AddTemplate(template, transform, tagContent: false);
        }

        internal void AddTemplateReference(PdfIndirectReference template, PdfName name, float a, float b, float c, float d, float e, float f)
        {
            if (inText && IsTagged())
            {
                EndText();
            }

            CheckWriter();
            name = PageResources.AddXObject(name, template);
            content.Append("q ");
            content.Append(a).Append(' ');
            content.Append(b).Append(' ');
            content.Append(c).Append(' ');
            content.Append(d).Append(' ');
            content.Append(e).Append(' ');
            content.Append(f).Append(" cm ");
            content.Append(name.GetBytes()).Append(" Do Q").Append_i(separator);
        }

        public virtual void AddTemplate(PdfTemplate template, float x, float y)
        {
            AddTemplate(template, 1f, 0f, 0f, 1f, x, y);
        }

        public virtual void AddTemplate(PdfTemplate template, float x, float y, bool tagContent)
        {
            AddTemplate(template, 1f, 0f, 0f, 1f, x, y, tagContent);
        }

        public virtual void SetCMYKColorFill(int cyan, int magenta, int yellow, int black)
        {
            SaveColor(new CMYKColor(cyan, magenta, yellow, black), fill: true);
            content.Append((float)(cyan & 0xFF) / 255f);
            content.Append(' ');
            content.Append((float)(magenta & 0xFF) / 255f);
            content.Append(' ');
            content.Append((float)(yellow & 0xFF) / 255f);
            content.Append(' ');
            content.Append((float)(black & 0xFF) / 255f);
            content.Append(" k").Append_i(separator);
        }

        public virtual void SetCMYKColorStroke(int cyan, int magenta, int yellow, int black)
        {
            SaveColor(new CMYKColor(cyan, magenta, yellow, black), fill: false);
            content.Append((float)(cyan & 0xFF) / 255f);
            content.Append(' ');
            content.Append((float)(magenta & 0xFF) / 255f);
            content.Append(' ');
            content.Append((float)(yellow & 0xFF) / 255f);
            content.Append(' ');
            content.Append((float)(black & 0xFF) / 255f);
            content.Append(" K").Append_i(separator);
        }

        public virtual void SetRGBColorFill(int red, int green, int blue)
        {
            SaveColor(new BaseColor(red, green, blue), fill: true);
            HelperRGB((float)(red & 0xFF) / 255f, (float)(green & 0xFF) / 255f, (float)(blue & 0xFF) / 255f);
            content.Append(" rg").Append_i(separator);
        }

        public virtual void SetRGBColorStroke(int red, int green, int blue)
        {
            SaveColor(new BaseColor(red, green, blue), fill: false);
            HelperRGB((float)(red & 0xFF) / 255f, (float)(green & 0xFF) / 255f, (float)(blue & 0xFF) / 255f);
            content.Append(" RG").Append_i(separator);
        }

        public virtual void SetColorStroke(BaseColor value)
        {
            switch (ExtendedColor.GetType(value))
            {
                case 1:
                    SetGrayStroke(((GrayColor)value).Gray);
                    break;
                case 2:
                    {
                        CMYKColor cMYKColor = (CMYKColor)value;
                        SetCMYKColorStrokeF(cMYKColor.Cyan, cMYKColor.Magenta, cMYKColor.Yellow, cMYKColor.Black);
                        break;
                    }
                case 3:
                    {
                        SpotColor spotColor = (SpotColor)value;
                        SetColorStroke(spotColor.PdfSpotColor, spotColor.Tint);
                        break;
                    }
                case 4:
                    {
                        PatternColor patternColor = (PatternColor)value;
                        SetPatternStroke(patternColor.Painter);
                        break;
                    }
                case 5:
                    {
                        ShadingColor shadingColor = (ShadingColor)value;
                        SetShadingStroke(shadingColor.PdfShadingPattern);
                        break;
                    }
                case 6:
                    {
                        DeviceNColor deviceNColor = (DeviceNColor)value;
                        SetColorStroke(deviceNColor.PdfDeviceNColor, deviceNColor.Tints);
                        break;
                    }
                case 7:
                    {
                        LabColor labColor = (LabColor)value;
                        SetColorStroke(labColor.LabColorSpace, labColor.L, labColor.A, labColor.B);
                        break;
                    }
                default:
                    SetRGBColorStroke(value.R, value.G, value.B);
                    break;
            }
        }

        public virtual void SetColorFill(BaseColor value)
        {
            switch (ExtendedColor.GetType(value))
            {
                case 1:
                    SetGrayFill(((GrayColor)value).Gray);
                    break;
                case 2:
                    {
                        CMYKColor cMYKColor = (CMYKColor)value;
                        SetCMYKColorFillF(cMYKColor.Cyan, cMYKColor.Magenta, cMYKColor.Yellow, cMYKColor.Black);
                        break;
                    }
                case 3:
                    {
                        SpotColor spotColor = (SpotColor)value;
                        SetColorFill(spotColor.PdfSpotColor, spotColor.Tint);
                        break;
                    }
                case 4:
                    {
                        PatternColor patternColor = (PatternColor)value;
                        SetPatternFill(patternColor.Painter);
                        break;
                    }
                case 5:
                    {
                        ShadingColor shadingColor = (ShadingColor)value;
                        SetShadingFill(shadingColor.PdfShadingPattern);
                        break;
                    }
                case 6:
                    {
                        DeviceNColor deviceNColor = (DeviceNColor)value;
                        SetColorFill(deviceNColor.PdfDeviceNColor, deviceNColor.Tints);
                        break;
                    }
                case 7:
                    {
                        LabColor labColor = (LabColor)value;
                        SetColorFill(labColor.LabColorSpace, labColor.L, labColor.A, labColor.B);
                        break;
                    }
                default:
                    SetRGBColorFill(value.R, value.G, value.B);
                    break;
            }
        }

        public virtual void SetColorFill(PdfSpotColor sp, float tint)
        {
            CheckWriter();
            state.colorDetails = writer.AddSimple(sp);
            PageResources pageResources = PageResources;
            PdfName colorSpaceName = state.colorDetails.ColorSpaceName;
            colorSpaceName = pageResources.AddColor(colorSpaceName, state.colorDetails.IndirectReference);
            SaveColor(new SpotColor(sp, tint), fill: true);
            content.Append(colorSpaceName.GetBytes()).Append(" cs ").Append(tint)
                .Append(" scn")
                .Append_i(separator);
        }

        public virtual void SetColorFill(PdfDeviceNColor dn, float[] tints)
        {
            CheckWriter();
            state.colorDetails = writer.AddSimple(dn);
            PageResources pageResources = PageResources;
            PdfName colorSpaceName = state.colorDetails.ColorSpaceName;
            colorSpaceName = pageResources.AddColor(colorSpaceName, state.colorDetails.IndirectReference);
            SaveColor(new DeviceNColor(dn, tints), fill: true);
            content.Append(colorSpaceName.GetBytes()).Append(" cs ");
            foreach (float num in tints)
            {
                content.Append(num.ToString(NumberFormatInfo.InvariantInfo) + " ");
            }

            content.Append("scn").Append_i(separator);
        }

        public virtual void SetColorFill(PdfLabColor lab, float l, float a, float b)
        {
            CheckWriter();
            state.colorDetails = writer.AddSimple(lab);
            PageResources pageResources = PageResources;
            PdfName colorSpaceName = state.colorDetails.ColorSpaceName;
            colorSpaceName = pageResources.AddColor(colorSpaceName, state.colorDetails.IndirectReference);
            SaveColor(new LabColor(lab, l, a, b), fill: true);
            content.Append(colorSpaceName.GetBytes()).Append(" cs ");
            content.Append(l.ToString(NumberFormatInfo.InvariantInfo) + " " + a.ToString(NumberFormatInfo.InvariantInfo) + " " + b.ToString(NumberFormatInfo.InvariantInfo) + " ");
            content.Append("scn").Append_i(separator);
        }

        public virtual void SetColorStroke(PdfSpotColor sp, float tint)
        {
            CheckWriter();
            state.colorDetails = writer.AddSimple(sp);
            PageResources pageResources = PageResources;
            PdfName colorSpaceName = state.colorDetails.ColorSpaceName;
            colorSpaceName = pageResources.AddColor(colorSpaceName, state.colorDetails.IndirectReference);
            SaveColor(new SpotColor(sp, tint), fill: false);
            content.Append(colorSpaceName.GetBytes()).Append(" CS ").Append(tint)
                .Append(" SCN")
                .Append_i(separator);
        }

        public virtual void SetColorStroke(PdfDeviceNColor sp, float[] tints)
        {
            CheckWriter();
            state.colorDetails = writer.AddSimple(sp);
            PageResources pageResources = PageResources;
            PdfName colorSpaceName = state.colorDetails.ColorSpaceName;
            colorSpaceName = pageResources.AddColor(colorSpaceName, state.colorDetails.IndirectReference);
            SaveColor(new DeviceNColor(sp, tints), fill: true);
            content.Append(colorSpaceName.GetBytes()).Append(" CS ");
            foreach (float num in tints)
            {
                content.Append(num.ToString(NumberFormatInfo.InvariantInfo) + " ");
            }

            content.Append("SCN").Append_i(separator);
        }

        public virtual void SetColorStroke(PdfLabColor lab, float l, float a, float b)
        {
            CheckWriter();
            state.colorDetails = writer.AddSimple(lab);
            PageResources pageResources = PageResources;
            PdfName colorSpaceName = state.colorDetails.ColorSpaceName;
            colorSpaceName = pageResources.AddColor(colorSpaceName, state.colorDetails.IndirectReference);
            SaveColor(new LabColor(lab, l, a, b), fill: true);
            content.Append(colorSpaceName.GetBytes()).Append(" CS ");
            content.Append(l + " " + a + " " + b + " ");
            content.Append("SCN").Append_i(separator);
        }

        public virtual void SetPatternFill(PdfPatternPainter p)
        {
            if (p.IsStencil())
            {
                SetPatternFill(p, p.DefaultColor);
                return;
            }

            CheckWriter();
            PageResources pageResources = PageResources;
            PdfName name = writer.AddSimplePattern(p);
            name = pageResources.AddPattern(name, p.IndirectReference);
            SaveColor(new PatternColor(p), fill: true);
            content.Append(PdfName.PATTERN.GetBytes()).Append(" cs ").Append(name.GetBytes())
                .Append(" scn")
                .Append_i(separator);
        }

        internal void OutputColorNumbers(BaseColor color, float tint)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 1, color);
            switch (ExtendedColor.GetType(color))
            {
                case 0:
                    content.Append((float)color.R / 255f);
                    content.Append(' ');
                    content.Append((float)color.G / 255f);
                    content.Append(' ');
                    content.Append((float)color.B / 255f);
                    break;
                case 1:
                    content.Append(((GrayColor)color).Gray);
                    break;
                case 2:
                    {
                        CMYKColor cMYKColor = (CMYKColor)color;
                        content.Append(cMYKColor.Cyan).Append(' ').Append(cMYKColor.Magenta);
                        content.Append(' ').Append(cMYKColor.Yellow).Append(' ')
                            .Append(cMYKColor.Black);
                        break;
                    }
                case 3:
                    content.Append(tint);
                    break;
                default:
                    throw new Exception(MessageLocalization.GetComposedMessage("invalid.color.type"));
            }
        }

        public virtual void SetPatternFill(PdfPatternPainter p, BaseColor color)
        {
            if (ExtendedColor.GetType(color) == 3)
            {
                SetPatternFill(p, color, ((SpotColor)color).Tint);
            }
            else
            {
                SetPatternFill(p, color, 0f);
            }
        }

        public virtual void SetPatternFill(PdfPatternPainter p, BaseColor color, float tint)
        {
            CheckWriter();
            if (!p.IsStencil())
            {
                throw new Exception(MessageLocalization.GetComposedMessage("an.uncolored.pattern.was.expected"));
            }

            PageResources pageResources = PageResources;
            PdfName name = writer.AddSimplePattern(p);
            name = pageResources.AddPattern(name, p.IndirectReference);
            ColorDetails colorDetails = writer.AddSimplePatternColorspace(color);
            PdfName pdfName = pageResources.AddColor(colorDetails.ColorSpaceName, colorDetails.IndirectReference);
            SaveColor(new UncoloredPattern(p, color, tint), fill: true);
            content.Append(pdfName.GetBytes()).Append(" cs").Append_i(separator);
            OutputColorNumbers(color, tint);
            content.Append(' ').Append(name.GetBytes()).Append(" scn")
                .Append_i(separator);
        }

        public virtual void SetPatternStroke(PdfPatternPainter p, BaseColor color)
        {
            if (ExtendedColor.GetType(color) == 3)
            {
                SetPatternStroke(p, color, ((SpotColor)color).Tint);
            }
            else
            {
                SetPatternStroke(p, color, 0f);
            }
        }

        public virtual void SetPatternStroke(PdfPatternPainter p, BaseColor color, float tint)
        {
            CheckWriter();
            if (!p.IsStencil())
            {
                throw new Exception(MessageLocalization.GetComposedMessage("an.uncolored.pattern.was.expected"));
            }

            PageResources pageResources = PageResources;
            PdfName name = writer.AddSimplePattern(p);
            name = pageResources.AddPattern(name, p.IndirectReference);
            ColorDetails colorDetails = writer.AddSimplePatternColorspace(color);
            PdfName pdfName = pageResources.AddColor(colorDetails.ColorSpaceName, colorDetails.IndirectReference);
            SaveColor(new UncoloredPattern(p, color, tint), fill: false);
            content.Append(pdfName.GetBytes()).Append(" CS").Append_i(separator);
            OutputColorNumbers(color, tint);
            content.Append(' ').Append(name.GetBytes()).Append(" SCN")
                .Append_i(separator);
        }

        public virtual void SetPatternStroke(PdfPatternPainter p)
        {
            if (p.IsStencil())
            {
                SetPatternStroke(p, p.DefaultColor);
                return;
            }

            CheckWriter();
            PageResources pageResources = PageResources;
            PdfName name = writer.AddSimplePattern(p);
            name = pageResources.AddPattern(name, p.IndirectReference);
            SaveColor(new PatternColor(p), fill: false);
            content.Append(PdfName.PATTERN.GetBytes()).Append(" CS ").Append(name.GetBytes())
                .Append(" SCN")
                .Append_i(separator);
        }

        public virtual void PaintShading(PdfShading shading)
        {
            writer.AddSimpleShading(shading);
            PageResources pageResources = PageResources;
            PdfName pdfName = pageResources.AddShading(shading.ShadingName, shading.ShadingReference);
            content.Append(pdfName.GetBytes()).Append(" sh").Append_i(separator);
            ColorDetails colorDetails = shading.ColorDetails;
            if (colorDetails != null)
            {
                pageResources.AddColor(colorDetails.ColorSpaceName, colorDetails.IndirectReference);
            }
        }

        public virtual void PaintShading(PdfShadingPattern shading)
        {
            PaintShading(shading.Shading);
        }

        public virtual void SetShadingFill(PdfShadingPattern shading)
        {
            writer.AddSimpleShadingPattern(shading);
            PageResources pageResources = PageResources;
            PdfName pdfName = pageResources.AddPattern(shading.PatternName, shading.PatternReference);
            SaveColor(new ShadingColor(shading), fill: true);
            content.Append(PdfName.PATTERN.GetBytes()).Append(" cs ").Append(pdfName.GetBytes())
                .Append(" scn")
                .Append_i(separator);
            ColorDetails colorDetails = shading.ColorDetails;
            if (colorDetails != null)
            {
                pageResources.AddColor(colorDetails.ColorSpaceName, colorDetails.IndirectReference);
            }
        }

        public virtual void SetShadingStroke(PdfShadingPattern shading)
        {
            writer.AddSimpleShadingPattern(shading);
            PageResources pageResources = PageResources;
            PdfName pdfName = pageResources.AddPattern(shading.PatternName, shading.PatternReference);
            SaveColor(new ShadingColor(shading), fill: false);
            content.Append(PdfName.PATTERN.GetBytes()).Append(" CS ").Append(pdfName.GetBytes())
                .Append(" SCN")
                .Append_i(separator);
            ColorDetails colorDetails = shading.ColorDetails;
            if (colorDetails != null)
            {
                pageResources.AddColor(colorDetails.ColorSpaceName, colorDetails.IndirectReference);
            }
        }

        protected virtual void CheckWriter()
        {
            if (writer == null)
            {
                throw new ArgumentNullException(MessageLocalization.GetComposedMessage("the.writer.in.pdfcontentbyte.is.null"));
            }
        }

        public virtual void ShowText(PdfTextArray text)
        {
            CheckState();
            if (!inText && IsTagged())
            {
                BeginText(restoreTM: true);
            }

            if (state.fontDetails == null)
            {
                throw new ArgumentNullException(MessageLocalization.GetComposedMessage("font.and.size.must.be.set.before.writing.any.text"));
            }

            content.Append('[');
            List<object> arrayList = text.ArrayList;
            bool flag = false;
            foreach (object item in arrayList)
            {
                if (item is string)
                {
                    ShowText2((string)item);
                    UpdateTx((string)item, 0f);
                    flag = false;
                    continue;
                }

                if (flag)
                {
                    content.Append(' ');
                }
                else
                {
                    flag = true;
                }

                content.Append((float)item);
                UpdateTx("", (float)item);
            }

            content.Append("]TJ").Append_i(separator);
        }

        public virtual void LocalGoto(string name, float llx, float lly, float urx, float ury)
        {
            pdf.LocalGoto(name, llx, lly, urx, ury);
        }

        public virtual bool LocalDestination(string name, PdfDestination destination)
        {
            return pdf.LocalDestination(name, destination);
        }

        public virtual PdfContentByte GetDuplicate(bool inheritGraphicState)
        {
            PdfContentByte duplicate = Duplicate;
            if (inheritGraphicState)
            {
                duplicate.state = state;
                duplicate.stateList = stateList;
            }

            return duplicate;
        }

        public virtual void InheritGraphicState(PdfContentByte parentCanvas)
        {
            state = parentCanvas.state;
            stateList = parentCanvas.stateList;
        }

        public virtual void RemoteGoto(string filename, string name, float llx, float lly, float urx, float ury)
        {
            pdf.RemoteGoto(filename, name, llx, lly, urx, ury);
        }

        public virtual void RemoteGoto(string filename, int page, float llx, float lly, float urx, float ury)
        {
            pdf.RemoteGoto(filename, page, llx, lly, urx, ury);
        }

        public virtual void RoundRectangle(float x, float y, float w, float h, float r)
        {
            if (w < 0f)
            {
                x += w;
                w = 0f - w;
            }

            if (h < 0f)
            {
                y += h;
                h = 0f - h;
            }

            if (r < 0f)
            {
                r = 0f - r;
            }

            float num = 0.4477f;
            MoveTo(x + r, y);
            LineTo(x + w - r, y);
            CurveTo(x + w - r * num, y, x + w, y + r * num, x + w, y + r);
            LineTo(x + w, y + h - r);
            CurveTo(x + w, y + h - r * num, x + w - r * num, y + h, x + w - r, y + h);
            LineTo(x + r, y + h);
            CurveTo(x + r * num, y + h, x, y + h - r * num, x, y + h - r);
            LineTo(x, y + r);
            CurveTo(x, y + r * num, x + r * num, y, x + r, y);
        }

        public virtual void SetAction(PdfAction action, float llx, float lly, float urx, float ury)
        {
            pdf.SetAction(action, llx, lly, urx, ury);
        }

        public virtual void SetLiteral(string s)
        {
            content.Append(s);
        }

        public virtual void SetLiteral(char c)
        {
            content.Append(c);
        }

        public virtual void SetLiteral(float n)
        {
            content.Append(n);
        }

        internal void CheckNoPattern(PdfTemplate t)
        {
            if (t.Type == 3)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.use.of.a.pattern.a.template.was.expected"));
            }
        }

        public virtual void DrawRadioField(float llx, float lly, float urx, float ury, bool on)
        {
            if (llx > urx)
            {
                float num = llx;
                llx = urx;
                urx = num;
            }

            if (lly > ury)
            {
                float num2 = lly;
                lly = ury;
                ury = num2;
            }

            SaveState();
            SetLineWidth(1f);
            SetLineCap(1);
            SetColorStroke(new BaseColor(192, 192, 192));
            Arc(llx + 1f, lly + 1f, urx - 1f, ury - 1f, 0f, 360f);
            Stroke();
            SetLineWidth(1f);
            SetLineCap(1);
            SetColorStroke(new BaseColor(160, 160, 160));
            Arc(llx + 0.5f, lly + 0.5f, urx - 0.5f, ury - 0.5f, 45f, 180f);
            Stroke();
            SetLineWidth(1f);
            SetLineCap(1);
            SetColorStroke(new BaseColor(0, 0, 0));
            Arc(llx + 1.5f, lly + 1.5f, urx - 1.5f, ury - 1.5f, 45f, 180f);
            Stroke();
            if (on)
            {
                SetLineWidth(1f);
                SetLineCap(1);
                SetColorFill(new BaseColor(0, 0, 0));
                Arc(llx + 4f, lly + 4f, urx - 4f, ury - 4f, 0f, 360f);
                Fill();
            }

            RestoreState();
        }

        public virtual void DrawTextField(float llx, float lly, float urx, float ury)
        {
            if (llx > urx)
            {
                float num = llx;
                llx = urx;
                urx = num;
            }

            if (lly > ury)
            {
                float num2 = lly;
                lly = ury;
                ury = num2;
            }

            SaveState();
            SetColorStroke(new BaseColor(192, 192, 192));
            SetLineWidth(1f);
            SetLineCap(0);
            Rectangle(llx, lly, urx - llx, ury - lly);
            Stroke();
            SetLineWidth(1f);
            SetLineCap(0);
            SetColorFill(new BaseColor(255, 255, 255));
            Rectangle(llx + 0.5f, lly + 0.5f, urx - llx - 1f, ury - lly - 1f);
            Fill();
            SetColorStroke(new BaseColor(192, 192, 192));
            SetLineWidth(1f);
            SetLineCap(0);
            MoveTo(llx + 1f, lly + 1.5f);
            LineTo(urx - 1.5f, lly + 1.5f);
            LineTo(urx - 1.5f, ury - 1f);
            Stroke();
            SetColorStroke(new BaseColor(160, 160, 160));
            SetLineWidth(1f);
            SetLineCap(0);
            MoveTo(llx + 1f, lly + 1f);
            LineTo(llx + 1f, ury - 1f);
            LineTo(urx - 1f, ury - 1f);
            Stroke();
            SetColorStroke(new BaseColor(0, 0, 0));
            SetLineWidth(1f);
            SetLineCap(0);
            MoveTo(llx + 2f, lly + 2f);
            LineTo(llx + 2f, ury - 2f);
            LineTo(urx - 2f, ury - 2f);
            Stroke();
            RestoreState();
        }

        public virtual void DrawButton(float llx, float lly, float urx, float ury, string text, BaseFont bf, float size)
        {
            if (llx > urx)
            {
                float num = llx;
                llx = urx;
                urx = num;
            }

            if (lly > ury)
            {
                float num2 = lly;
                lly = ury;
                ury = num2;
            }

            SaveState();
            SetColorStroke(new BaseColor(0, 0, 0));
            SetLineWidth(1f);
            SetLineCap(0);
            Rectangle(llx, lly, urx - llx, ury - lly);
            Stroke();
            SetLineWidth(1f);
            SetLineCap(0);
            SetColorFill(new BaseColor(192, 192, 192));
            Rectangle(llx + 0.5f, lly + 0.5f, urx - llx - 1f, ury - lly - 1f);
            Fill();
            SetColorStroke(new BaseColor(255, 255, 255));
            SetLineWidth(1f);
            SetLineCap(0);
            MoveTo(llx + 1f, lly + 1f);
            LineTo(llx + 1f, ury - 1f);
            LineTo(urx - 1f, ury - 1f);
            Stroke();
            SetColorStroke(new BaseColor(160, 160, 160));
            SetLineWidth(1f);
            SetLineCap(0);
            MoveTo(llx + 1f, lly + 1f);
            LineTo(urx - 1f, lly + 1f);
            LineTo(urx - 1f, ury - 1f);
            Stroke();
            ResetRGBColorFill();
            BeginText();
            SetFontAndSize(bf, size);
            ShowTextAligned(1, text, llx + (urx - llx) / 2f, lly + (ury - lly - size) / 2f, 0f);
            EndText();
            RestoreState();
        }

        public virtual void SetGState(PdfGState gstate)
        {
            PdfObject[] array = writer.AddSimpleExtGState(gstate);
            PdfName pdfName = PageResources.AddExtGState((PdfName)array[0], (PdfIndirectReference)array[1]);
            state.extGState = gstate;
            content.Append(pdfName.GetBytes()).Append(" gs").Append_i(separator);
        }

        public virtual void BeginLayer(IPdfOCG layer)
        {
            if (layer is PdfLayer && ((PdfLayer)layer).Title != null)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("a.title.is.not.a.layer"));
            }

            if (layerDepth == null)
            {
                layerDepth = new List<int>();
            }

            if (layer is PdfLayerMembership)
            {
                layerDepth.Add(1);
                BeginLayer2(layer);
                return;
            }

            int num = 0;
            for (PdfLayer pdfLayer = (PdfLayer)layer; pdfLayer != null; pdfLayer = pdfLayer.Parent)
            {
                if (pdfLayer.Title == null)
                {
                    BeginLayer2(pdfLayer);
                    num++;
                }
            }

            layerDepth.Add(num);
        }

        private void BeginLayer2(IPdfOCG layer)
        {
            PdfName name = (PdfName)writer.AddSimpleProperty(layer, layer.Ref)[0];
            name = PageResources.AddProperty(name, layer.Ref);
            content.Append("/OC ").Append(name.GetBytes()).Append(" BDC")
                .Append_i(separator);
        }

        public virtual void EndLayer()
        {
            int num = 1;
            if (layerDepth != null && layerDepth.Count > 0)
            {
                num = layerDepth[layerDepth.Count - 1];
                layerDepth.RemoveAt(layerDepth.Count - 1);
                while (num-- > 0)
                {
                    content.Append("EMC").Append_i(separator);
                }

                return;
            }

            throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.layer.operators"));
        }

        internal virtual void AddAnnotation(PdfAnnotation annot)
        {
            bool num = IsTagged() && annot.Role != null && (!(annot is PdfFormField) || ((PdfFormField)annot).Kids == null);
            if (num)
            {
                OpenMCBlock(annot);
            }

            writer.AddAnnotation(annot);
            if (num)
            {
                PdfStructureElement value = null;
                pdf.structElements.TryGetValue(annot.ID, out value);
                if (value != null)
                {
                    int structParentIndex = pdf.GetStructParentIndex(annot);
                    annot.Put(PdfName.STRUCTPARENT, new PdfNumber(structParentIndex));
                    value.SetAnnotation(annot, CurrentPage);
                    writer.StructureTreeRoot.SetAnnotationMark(structParentIndex, value.Reference);
                }

                CloseMCBlock(annot);
            }
        }

        public virtual void AddAnnotation(PdfAnnotation annot, bool applyCTM)
        {
            if (applyCTM && !state.CTM.IsIdentity())
            {
                annot.ApplyCTM(state.CTM);
            }

            AddAnnotation(annot);
        }

        public virtual void SetDefaultColorspace(PdfName name, PdfObject obj)
        {
            PageResources.AddDefaultColor(name, obj);
        }

        public virtual void Transform(AffineTransform af)
        {
            if (inText && IsTagged())
            {
                EndText();
            }

            double[] array = new double[6];
            af.GetMatrix(array);
            state.CTM.Concatenate(af);
            content.Append(array[0]).Append(' ').Append(array[1])
                .Append(' ')
                .Append(array[2])
                .Append(' ');
            content.Append(array[3]).Append(' ').Append(array[4])
                .Append(' ')
                .Append(array[5])
                .Append(" cm")
                .Append_i(separator);
        }

        [Obsolete]
        public void Transform(Matrix tx)
        {
            if (inText && IsTagged())
            {
                EndText();
            }

            float[] elements = tx.Elements;
            ConcatCTM(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
        }

        public virtual void BeginMarkedContentSequence(PdfStructureElement struc)
        {
            PdfObject pdfObject = struc.Get(PdfName.K);
            int[] structParentIndexAndNextMarkPoint = pdf.GetStructParentIndexAndNextMarkPoint(CurrentPage);
            int page = structParentIndexAndNextMarkPoint[0];
            int num = structParentIndexAndNextMarkPoint[1];
            if (pdfObject != null)
            {
                PdfArray pdfArray = null;
                if (pdfObject.IsNumber())
                {
                    pdfArray = new PdfArray();
                    pdfArray.Add(pdfObject);
                    struc.Put(PdfName.K, pdfArray);
                }
                else
                {
                    if (!pdfObject.IsArray())
                    {
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.object.at.k.1", pdfObject.GetType().ToString()));
                    }

                    pdfArray = (PdfArray)pdfObject;
                }

                if (pdfArray.GetAsNumber(0) != null)
                {
                    PdfDictionary pdfDictionary = new PdfDictionary(PdfName.MCR);
                    pdfDictionary.Put(PdfName.PG, CurrentPage);
                    pdfDictionary.Put(PdfName.MCID, new PdfNumber(num));
                    pdfArray.Add(pdfDictionary);
                }

                struc.SetPageMark(pdf.GetStructParentIndex(CurrentPage), -1);
            }
            else
            {
                struc.SetPageMark(page, num);
                struc.Put(PdfName.PG, CurrentPage);
            }

            SetMcDepth(GetMcDepth() + 1);
            int size = content.Size;
            content.Append(struc.Get(PdfName.S).GetBytes()).Append(" <</MCID ").Append(num)
                .Append(">> BDC")
                .Append_i(separator);
            markedContentSize += content.Size - size;
        }

        public virtual void EndMarkedContentSequence()
        {
            if (GetMcDepth() == 0)
            {
                throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.begin.end.marked.content.operators"));
            }

            int size = content.Size;
            SetMcDepth(GetMcDepth() - 1);
            content.Append("EMC").Append_i(separator);
            markedContentSize += content.Size - size;
        }

        public virtual void BeginMarkedContentSequence(PdfName tag, PdfDictionary property, bool inline)
        {
            int size = content.Size;
            if (property == null)
            {
                content.Append(tag.GetBytes()).Append(" BMC").Append_i(separator);
                SetMcDepth(GetMcDepth() + 1);
            }
            else
            {
                content.Append(tag.GetBytes()).Append(' ');
                if (inline)
                {
                    property.ToPdf(writer, content);
                }
                else
                {
                    PdfObject[] array = ((!writer.PropertyExists(property)) ? writer.AddSimpleProperty(property, writer.PdfIndirectReference) : writer.AddSimpleProperty(property, null));
                    PdfName name = (PdfName)array[0];
                    name = PageResources.AddProperty(name, (PdfIndirectReference)array[1]);
                    content.Append(name.GetBytes());
                }

                content.Append(" BDC").Append_i(separator);
                SetMcDepth(GetMcDepth() + 1);
            }

            markedContentSize += content.Size - size;
        }

        public virtual void BeginMarkedContentSequence(PdfName tag)
        {
            BeginMarkedContentSequence(tag, null, inline: false);
        }

        public virtual void SanityCheck()
        {
            if (GetMcDepth() != 0)
            {
                throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.marked.content.operators"));
            }

            if (inText)
            {
                if (!IsTagged())
                {
                    throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.begin.end.text.operators"));
                }

                EndText();
            }

            if (layerDepth != null && layerDepth.Count > 0)
            {
                throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.layer.operators"));
            }

            if (stateList.Count > 0)
            {
                throw new IllegalPdfSyntaxException(MessageLocalization.GetComposedMessage("unbalanced.save.restore.state.operators"));
            }
        }

        public virtual void OpenMCBlock(IAccessibleElement element)
        {
            if (!IsTagged())
            {
                return;
            }

            if (pdf.openMCDocument)
            {
                pdf.openMCDocument = false;
                writer.DirectContentUnder.OpenMCBlock(pdf);
            }

            if (element != null && !GetMcElements().Contains(element))
            {
                PdfStructureElement pdfStructureElement = OpenMCBlockInt(element);
                GetMcElements().Add(element);
                if (pdfStructureElement != null)
                {
                    pdf.structElements[element.ID] = pdfStructureElement;
                }
            }
        }

        private PdfDictionary GetParentStructureElement()
        {
            PdfStructureElement value = null;
            if (GetMcElements().Count > 0 && pdf.structElements.TryGetValue(GetMcElements()[GetMcElements().Count - 1].ID, out value))
            {
                return value;
            }

            return writer.StructureTreeRoot;
        }

        private PdfStructureElement OpenMCBlockInt(IAccessibleElement element)
        {
            PdfStructureElement value = null;
            if (IsTagged())
            {
                IAccessibleElement parent = null;
                if (GetMcElements().Count > 0)
                {
                    parent = GetMcElements()[GetMcElements().Count - 1];
                }

                writer.CheckElementRole(element, parent);
                if (element.Role != null)
                {
                    if (!PdfName.ARTIFACT.Equals(element.Role) && !pdf.structElements.TryGetValue(element.ID, out value))
                    {
                        value = new PdfStructureElement(GetParentStructureElement(), element.Role);
                    }

                    if (PdfName.ARTIFACT.Equals(element.Role))
                    {
                        Dictionary<PdfName, PdfObject> accessibleAttributes = element.GetAccessibleAttributes();
                        PdfDictionary pdfDictionary = null;
                        if (accessibleAttributes != null && accessibleAttributes.Count != 0)
                        {
                            pdfDictionary = new PdfDictionary();
                            foreach (KeyValuePair<PdfName, PdfObject> item in accessibleAttributes)
                            {
                                pdfDictionary.Put(item.Key, item.Value);
                            }
                        }

                        bool num = inText;
                        if (inText)
                        {
                            EndText();
                        }

                        BeginMarkedContentSequence(element.Role, pdfDictionary, inline: true);
                        if (num)
                        {
                            BeginText(restoreTM: true);
                        }
                    }
                    else if (writer.NeedToBeMarkedInContent(element))
                    {
                        bool num2 = inText;
                        if (inText)
                        {
                            EndText();
                        }

                        BeginMarkedContentSequence(value);
                        if (num2)
                        {
                            BeginText(restoreTM: true);
                        }
                    }
                }
            }

            return value;
        }

        public virtual void CloseMCBlock(IAccessibleElement element)
        {
            if (IsTagged() && element != null && GetMcElements().Contains(element))
            {
                CloseMCBlockInt(element);
                GetMcElements().Remove(element);
            }
        }

        private void CloseMCBlockInt(IAccessibleElement element)
        {
            if (!IsTagged() || element.Role == null)
            {
                return;
            }

            PdfStructureElement value = null;
            pdf.structElements.TryGetValue(element.ID, out value);
            value?.WriteAttributes(element);
            if (writer.NeedToBeMarkedInContent(element))
            {
                bool num = inText;
                if (inText)
                {
                    EndText();
                }

                EndMarkedContentSequence();
                if (num)
                {
                    BeginText(restoreTM: true);
                }
            }
        }

        internal IList<IAccessibleElement> SaveMCBlocks()
        {
            IList<IAccessibleElement> list = new List<IAccessibleElement>();
            if (IsTagged())
            {
                list = GetMcElements();
                for (int i = 0; i < list.Count; i++)
                {
                    CloseMCBlockInt(list[i]);
                }

                SetMcElements(new List<IAccessibleElement>());
            }

            return list;
        }

        internal void RestoreMCBlocks(IList<IAccessibleElement> mcElements)
        {
            if (IsTagged() && mcElements != null)
            {
                SetMcElements(mcElements);
                for (int i = 0; i < GetMcElements().Count; i++)
                {
                    OpenMCBlockInt(GetMcElements()[i]);
                }
            }
        }

        internal int GetMcDepth()
        {
            if (duplicatedFrom != null)
            {
                return duplicatedFrom.GetMcDepth();
            }

            return mcDepth;
        }

        internal void SetMcDepth(int value)
        {
            if (duplicatedFrom != null)
            {
                duplicatedFrom.SetMcDepth(value);
            }
            else
            {
                mcDepth = value;
            }
        }

        internal IList<IAccessibleElement> GetMcElements()
        {
            if (duplicatedFrom != null)
            {
                return duplicatedFrom.GetMcElements();
            }

            return mcElements;
        }

        internal void SetMcElements(IList<IAccessibleElement> value)
        {
            if (duplicatedFrom != null)
            {
                duplicatedFrom.SetMcElements(value);
            }
            else
            {
                mcElements = value;
            }
        }

        internal void UpdateTx(string text, float Tj)
        {
            state.tx += GetEffectiveStringWidth(text, kerned: false, Tj);
        }

        private void SaveColor(BaseColor color, bool fill)
        {
            if (IsTagged())
            {
                if (inText)
                {
                    if (fill)
                    {
                        state.textColorFill = color;
                    }
                    else
                    {
                        state.textColorStroke = color;
                    }
                }
                else if (fill)
                {
                    state.colorFill = color;
                }
                else
                {
                    state.colorStroke = color;
                }
            }
            else if (fill)
            {
                state.colorFill = color;
            }
            else
            {
                state.colorStroke = color;
            }
        }

        private void RestoreColor(BaseColor color, bool fill)
        {
            if (!IsTagged())
            {
                return;
            }

            if (color is UncoloredPattern)
            {
                UncoloredPattern uncoloredPattern = (UncoloredPattern)color;
                if (fill)
                {
                    SetPatternFill(uncoloredPattern.Painter, uncoloredPattern.color, uncoloredPattern.tint);
                }
                else
                {
                    SetPatternStroke(uncoloredPattern.Painter, uncoloredPattern.color, uncoloredPattern.tint);
                }
            }
            else if (fill)
            {
                SetColorFill(color);
            }
            else
            {
                SetColorStroke(color);
            }
        }

        private void RestoreColor()
        {
            if (!IsTagged())
            {
                return;
            }

            if (inText)
            {
                if (!state.textColorFill.Equals(state.colorFill))
                {
                    RestoreColor(state.textColorFill, fill: true);
                }

                if (!state.textColorStroke.Equals(state.colorStroke))
                {
                    RestoreColor(state.textColorStroke, fill: false);
                }
            }
            else
            {
                if (!state.textColorFill.Equals(state.colorFill))
                {
                    RestoreColor(state.colorFill, fill: true);
                }

                if (!state.textColorStroke.Equals(state.colorStroke))
                {
                    RestoreColor(state.colorStroke, fill: false);
                }
            }
        }

        protected virtual void CheckState()
        {
            bool flag = false;
            bool flag2 = false;
            if (state.textRenderMode == 0)
            {
                flag2 = true;
            }
            else if (state.textRenderMode == 1)
            {
                flag = true;
            }
            else if (state.textRenderMode == 2)
            {
                flag2 = true;
                flag = true;
            }

            if (flag2)
            {
                PdfWriter.CheckPdfIsoConformance(writer, 1, IsTagged() ? state.textColorFill : state.colorFill);
            }

            if (flag)
            {
                PdfWriter.CheckPdfIsoConformance(writer, 1, IsTagged() ? state.textColorStroke : state.colorStroke);
            }

            PdfWriter.CheckPdfIsoConformance(writer, 6, state.extGState);
        }
    }
}
