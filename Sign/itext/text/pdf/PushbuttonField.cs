using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PushbuttonField : BaseField
    {
        public const int LAYOUT_LABEL_ONLY = 1;

        public const int LAYOUT_ICON_ONLY = 2;

        public const int LAYOUT_ICON_TOP_LABEL_BOTTOM = 3;

        public const int LAYOUT_LABEL_TOP_ICON_BOTTOM = 4;

        public const int LAYOUT_ICON_LEFT_LABEL_RIGHT = 5;

        public const int LAYOUT_LABEL_LEFT_ICON_RIGHT = 6;

        public const int LAYOUT_LABEL_OVER_ICON = 7;

        public const int SCALE_ICON_ALWAYS = 1;

        public const int SCALE_ICON_NEVER = 2;

        public const int SCALE_ICON_IS_TOO_BIG = 3;

        public const int SCALE_ICON_IS_TOO_SMALL = 4;

        private int layout = 1;

        private Image image;

        private PdfTemplate template;

        private int scaleIcon = 1;

        private bool proportionalIcon = true;

        private float iconVerticalAdjustment = 0.5f;

        private float iconHorizontalAdjustment = 0.5f;

        private bool iconFitToBounds;

        private PdfTemplate tp;

        private PRIndirectReference iconReference;

        public virtual int Layout
        {
            get
            {
                return layout;
            }
            set
            {
                if (value < 1 || value > 7)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("layout.out.of.bounds"));
                }

                layout = value;
            }
        }

        public virtual Image Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                template = null;
            }
        }

        public virtual PdfTemplate Template
        {
            get
            {
                return template;
            }
            set
            {
                template = value;
                image = null;
            }
        }

        public virtual int ScaleIcon
        {
            get
            {
                return scaleIcon;
            }
            set
            {
                if (value < 1 || value > 4)
                {
                    scaleIcon = 1;
                }
                else
                {
                    scaleIcon = value;
                }
            }
        }

        public virtual bool ProportionalIcon
        {
            get
            {
                return proportionalIcon;
            }
            set
            {
                proportionalIcon = value;
            }
        }

        public virtual float IconVerticalAdjustment
        {
            get
            {
                return iconVerticalAdjustment;
            }
            set
            {
                iconVerticalAdjustment = value;
                if (iconVerticalAdjustment < 0f)
                {
                    iconVerticalAdjustment = 0f;
                }
                else if (iconVerticalAdjustment > 1f)
                {
                    iconVerticalAdjustment = 1f;
                }
            }
        }

        public virtual float IconHorizontalAdjustment
        {
            get
            {
                return iconHorizontalAdjustment;
            }
            set
            {
                iconHorizontalAdjustment = value;
                if (iconHorizontalAdjustment < 0f)
                {
                    iconHorizontalAdjustment = 0f;
                }
                else if (iconHorizontalAdjustment > 1f)
                {
                    iconHorizontalAdjustment = 1f;
                }
            }
        }

        public virtual PdfFormField Field
        {
            get
            {
                PdfFormField pdfFormField = PdfFormField.CreatePushButton(writer);
                pdfFormField.SetWidget(box, PdfAnnotation.HIGHLIGHT_INVERT);
                if (fieldName != null)
                {
                    pdfFormField.FieldName = fieldName;
                    if (((uint)options & (true ? 1u : 0u)) != 0)
                    {
                        pdfFormField.SetFieldFlags(1);
                    }

                    if (((uint)options & 2u) != 0)
                    {
                        pdfFormField.SetFieldFlags(2);
                    }
                }

                if (text != null)
                {
                    pdfFormField.MKNormalCaption = text;
                }

                if (rotation != 0)
                {
                    pdfFormField.MKRotation = rotation;
                }

                pdfFormField.BorderStyle = new PdfBorderDictionary(borderWidth, borderStyle, new PdfDashPattern(3f));
                PdfAppearance appearance = GetAppearance();
                pdfFormField.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, appearance);
                PdfAppearance pdfAppearance = (PdfAppearance)appearance.Duplicate;
                pdfAppearance.SetFontAndSize(RealFont, fontSize);
                if (textColor == null)
                {
                    pdfAppearance.SetGrayFill(0f);
                }
                else
                {
                    pdfAppearance.SetColorFill(textColor);
                }

                pdfFormField.DefaultAppearanceString = pdfAppearance;
                if (borderColor != null)
                {
                    pdfFormField.MKBorderColor = borderColor;
                }

                if (backgroundColor != null)
                {
                    pdfFormField.MKBackgroundColor = backgroundColor;
                }

                switch (visibility)
                {
                    case 1:
                        pdfFormField.Flags = 6;
                        break;
                    case 3:
                        pdfFormField.Flags = 36;
                        break;
                    default:
                        pdfFormField.Flags = 4;
                        break;
                    case 2:
                        break;
                }

                if (tp != null)
                {
                    pdfFormField.MKNormalIcon = tp;
                }

                pdfFormField.MKTextPosition = layout - 1;
                PdfName scale = PdfName.A;
                if (scaleIcon == 3)
                {
                    scale = PdfName.B;
                }
                else if (scaleIcon == 4)
                {
                    scale = PdfName.S;
                }
                else if (scaleIcon == 2)
                {
                    scale = PdfName.N;
                }

                pdfFormField.SetMKIconFit(scale, proportionalIcon ? PdfName.P : PdfName.A, iconHorizontalAdjustment, iconVerticalAdjustment, iconFitToBounds);
                return pdfFormField;
            }
        }

        public virtual bool IconFitToBounds
        {
            get
            {
                return iconFitToBounds;
            }
            set
            {
                iconFitToBounds = value;
            }
        }

        public virtual PRIndirectReference IconReference
        {
            get
            {
                return iconReference;
            }
            set
            {
                iconReference = value;
            }
        }

        public PushbuttonField(PdfWriter writer, Rectangle box, string fieldName)
            : base(writer, box, fieldName)
        {
        }

        private float CalculateFontSize(float w, float h)
        {
            BaseFont realFont = RealFont;
            float num = fontSize;
            if (num == 0f)
            {
                float widthPoint = realFont.GetWidthPoint(text, 1f);
                num = ((widthPoint != 0f) ? (w / widthPoint) : 12f);
                float val = h / (1f - realFont.GetFontDescriptor(3, 1f));
                num = Math.Min(num, val);
                if (num < 4f)
                {
                    num = 4f;
                }
            }

            return num;
        }

        public virtual PdfAppearance GetAppearance()
        {
            PdfAppearance borderAppearance = GetBorderAppearance();
            Rectangle rectangle = new Rectangle(borderAppearance.BoundingBox);
            if ((text == null || text.Length == 0) && (layout == 1 || (image == null && template == null && iconReference == null)))
            {
                return borderAppearance;
            }

            if (layout == 2 && image == null && template == null && iconReference == null)
            {
                return borderAppearance;
            }

            BaseFont realFont = RealFont;
            bool num = borderStyle == 2 || borderStyle == 3;
            float num2 = rectangle.Height - borderWidth * 2f;
            float num3 = borderWidth;
            if (num)
            {
                num2 -= borderWidth * 2f;
                num3 *= 2f;
            }

            float val = (num ? (2f * borderWidth) : borderWidth);
            val = Math.Max(val, 1f);
            float num4 = Math.Min(num3, val);
            tp = null;
            float num5 = float.NaN;
            float num6 = 0f;
            float num7 = fontSize;
            float num8 = rectangle.Width - 2f * num4 - 2f;
            float num9 = rectangle.Height - 2f * num4;
            float num10 = (iconFitToBounds ? 0f : (num4 + 1f));
            int num11 = layout;
            if (image == null && template == null && iconReference == null)
            {
                num11 = 1;
            }

            Rectangle rectangle2 = null;
            while (true)
            {
                switch (num11)
                {
                    case 1:
                    case 7:
                        if (text != null && text.Length > 0 && num8 > 0f && num9 > 0f)
                        {
                            num7 = CalculateFontSize(num8, num9);
                            num5 = (rectangle.Width - realFont.GetWidthPoint(text, num7)) / 2f;
                            num6 = (rectangle.Height - realFont.GetFontDescriptor(1, num7)) / 2f;
                        }

                        goto case 2;
                    case 2:
                        if (num11 == 7 || num11 == 2)
                        {
                            rectangle2 = new Rectangle(rectangle.Left + num10, rectangle.Bottom + num10, rectangle.Right - num10, rectangle.Top - num10);
                        }

                        break;
                    case 3:
                        {
                            if (text == null || text.Length == 0 || num8 <= 0f || num9 <= 0f)
                            {
                                num11 = 2;
                                continue;
                            }

                            float num13 = rectangle.Height * 0.35f - num4;
                            num7 = ((!(num13 > 0f)) ? 4f : CalculateFontSize(num8, num13));
                            num5 = (rectangle.Width - realFont.GetWidthPoint(text, num7)) / 2f;
                            num6 = num4 - realFont.GetFontDescriptor(3, num7);
                            rectangle2 = new Rectangle(rectangle.Left + num10, num6 + num7, rectangle.Right - num10, rectangle.Top - num10);
                            break;
                        }
                    case 4:
                        {
                            if (text == null || text.Length == 0 || num8 <= 0f || num9 <= 0f)
                            {
                                num11 = 2;
                                continue;
                            }

                            float num13 = rectangle.Height * 0.35f - num4;
                            num7 = ((!(num13 > 0f)) ? 4f : CalculateFontSize(num8, num13));
                            num5 = (rectangle.Width - realFont.GetWidthPoint(text, num7)) / 2f;
                            num6 = rectangle.Height - num4 - num7;
                            if (num6 < num4)
                            {
                                num6 = num4;
                            }

                            rectangle2 = new Rectangle(rectangle.Left + num10, rectangle.Bottom + num10, rectangle.Right - num10, num6 + realFont.GetFontDescriptor(3, num7));
                            break;
                        }
                    case 6:
                        {
                            if (text == null || text.Length == 0 || num8 <= 0f || num9 <= 0f)
                            {
                                num11 = 2;
                                continue;
                            }

                            float num12 = rectangle.Width * 0.35f - num4;
                            num7 = ((!(num12 > 0f)) ? 4f : CalculateFontSize(num8, num12));
                            if (realFont.GetWidthPoint(text, num7) >= num8)
                            {
                                num11 = 1;
                                num7 = fontSize;
                                continue;
                            }

                            num5 = num4 + 1f;
                            num6 = (rectangle.Height - realFont.GetFontDescriptor(1, num7)) / 2f;
                            rectangle2 = new Rectangle(num5 + realFont.GetWidthPoint(text, num7), rectangle.Bottom + num10, rectangle.Right - num10, rectangle.Top - num10);
                            break;
                        }
                    case 5:
                        {
                            if (text == null || text.Length == 0 || num8 <= 0f || num9 <= 0f)
                            {
                                num11 = 2;
                                continue;
                            }

                            float num12 = rectangle.Width * 0.35f - num4;
                            num7 = ((!(num12 > 0f)) ? 4f : CalculateFontSize(num8, num12));
                            if (realFont.GetWidthPoint(text, num7) >= num8)
                            {
                                num11 = 1;
                                num7 = fontSize;
                                continue;
                            }

                            num5 = rectangle.Width - realFont.GetWidthPoint(text, num7) - num4 - 1f;
                            num6 = (rectangle.Height - realFont.GetFontDescriptor(1, num7)) / 2f;
                            rectangle2 = new Rectangle(rectangle.Left + num10, rectangle.Bottom + num10, num5 - 1f, rectangle.Top - num10);
                            break;
                        }
                }

                break;
            }

            if (num6 < rectangle.Bottom + num4)
            {
                num6 = rectangle.Bottom + num4;
            }

            if (rectangle2 != null && (rectangle2.Width <= 0f || rectangle2.Height <= 0f))
            {
                rectangle2 = null;
            }

            bool flag = false;
            float num14 = 0f;
            float num15 = 0f;
            PdfArray pdfArray = null;
            if (rectangle2 != null)
            {
                if (image != null)
                {
                    tp = new PdfTemplate(writer);
                    tp.BoundingBox = new Rectangle(image);
                    writer.AddDirectTemplateSimple(tp, PdfName.FRM);
                    tp.AddImage(image, image.Width, 0f, 0f, image.Height, 0f, 0f);
                    flag = true;
                    num14 = tp.BoundingBox.Width;
                    num15 = tp.BoundingBox.Height;
                }
                else if (template != null)
                {
                    tp = new PdfTemplate(writer);
                    tp.BoundingBox = new Rectangle(template.Width, template.Height);
                    writer.AddDirectTemplateSimple(tp, PdfName.FRM);
                    tp.AddTemplate(template, template.BoundingBox.Left, template.BoundingBox.Bottom);
                    flag = true;
                    num14 = tp.BoundingBox.Width;
                    num15 = tp.BoundingBox.Height;
                }
                else if (iconReference != null)
                {
                    PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObject(iconReference);
                    if (pdfDictionary != null)
                    {
                        Rectangle normalizedRectangle = PdfReader.GetNormalizedRectangle(pdfDictionary.GetAsArray(PdfName.BBOX));
                        pdfArray = pdfDictionary.GetAsArray(PdfName.MATRIX);
                        flag = true;
                        num14 = normalizedRectangle.Width;
                        num15 = normalizedRectangle.Height;
                    }
                }
            }

            if (flag)
            {
                float num16 = rectangle2.Width / num14;
                float num17 = rectangle2.Height / num15;
                if (proportionalIcon)
                {
                    switch (scaleIcon)
                    {
                        case 3:
                            num16 = Math.Min(num16, num17);
                            num16 = Math.Min(num16, 1f);
                            break;
                        case 4:
                            num16 = Math.Min(num16, num17);
                            num16 = Math.Max(num16, 1f);
                            break;
                        case 2:
                            num16 = 1f;
                            break;
                        default:
                            num16 = Math.Min(num16, num17);
                            break;
                    }

                    num17 = num16;
                }
                else
                {
                    switch (scaleIcon)
                    {
                        case 3:
                            num16 = Math.Min(num16, 1f);
                            num17 = Math.Min(num17, 1f);
                            break;
                        case 4:
                            num16 = Math.Max(num16, 1f);
                            num17 = Math.Max(num17, 1f);
                            break;
                        case 2:
                            num16 = (num17 = 1f);
                            break;
                    }
                }

                float num18 = rectangle2.Left + (rectangle2.Width - num14 * num16) * iconHorizontalAdjustment;
                float num19 = rectangle2.Bottom + (rectangle2.Height - num15 * num17) * iconVerticalAdjustment;
                borderAppearance.SaveState();
                borderAppearance.Rectangle(rectangle2.Left, rectangle2.Bottom, rectangle2.Width, rectangle2.Height);
                borderAppearance.Clip();
                borderAppearance.NewPath();
                if (tp != null)
                {
                    borderAppearance.AddTemplate(tp, num16, 0f, 0f, num17, num18, num19);
                }
                else
                {
                    float num20 = 0f;
                    float num21 = 0f;
                    if (pdfArray != null && pdfArray.Size == 6)
                    {
                        PdfNumber asNumber = pdfArray.GetAsNumber(4);
                        if (asNumber != null)
                        {
                            num20 = asNumber.FloatValue;
                        }

                        asNumber = pdfArray.GetAsNumber(5);
                        if (asNumber != null)
                        {
                            num21 = asNumber.FloatValue;
                        }
                    }

                    borderAppearance.AddTemplateReference(iconReference, PdfName.FRM, num16, 0f, 0f, num17, num18 - num20 * num16, num19 - num21 * num17);
                }

                borderAppearance.RestoreState();
            }

            if (!float.IsNaN(num5))
            {
                borderAppearance.SaveState();
                borderAppearance.Rectangle(num4, num4, rectangle.Width - 2f * num4, rectangle.Height - 2f * num4);
                borderAppearance.Clip();
                borderAppearance.NewPath();
                if (textColor == null)
                {
                    borderAppearance.ResetGrayFill();
                }
                else
                {
                    borderAppearance.SetColorFill(textColor);
                }

                borderAppearance.BeginText();
                borderAppearance.SetFontAndSize(realFont, num7);
                borderAppearance.SetTextMatrix(num5, num6);
                borderAppearance.ShowText(text);
                borderAppearance.EndText();
                borderAppearance.RestoreState();
            }

            return borderAppearance;
        }
    }
}
