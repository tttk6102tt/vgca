using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class RadioCheckField : BaseField
    {
        public const int TYPE_CHECK = 1;

        public const int TYPE_CIRCLE = 2;

        public const int TYPE_CROSS = 3;

        public const int TYPE_DIAMOND = 4;

        public const int TYPE_SQUARE = 5;

        public const int TYPE_STAR = 6;

        protected static string[] typeChars = new string[6] { "4", "l", "8", "u", "n", "H" };

        protected int checkType;

        private string onValue;

        private bool vchecked;

        public virtual int CheckType
        {
            get
            {
                return checkType;
            }
            set
            {
                checkType = value;
                if (checkType < 1 || checkType > 6)
                {
                    checkType = 2;
                }

                Text = typeChars[checkType - 1];
                Font = BaseFont.CreateFont("ZapfDingbats", "Cp1252", embedded: false);
            }
        }

        public virtual string OnValue
        {
            get
            {
                return onValue;
            }
            set
            {
                onValue = value;
            }
        }

        public virtual bool Checked
        {
            get
            {
                return vchecked;
            }
            set
            {
                vchecked = value;
            }
        }

        public virtual PdfFormField RadioField => GetField(isRadio: true);

        public virtual PdfFormField CheckField => GetField(isRadio: false);

        public RadioCheckField(PdfWriter writer, Rectangle box, string fieldName, string onValue)
            : base(writer, box, fieldName)
        {
            OnValue = onValue;
            CheckType = 2;
        }

        public virtual PdfAppearance GetAppearance(bool isRadio, bool on)
        {
            if (isRadio && checkType == 2)
            {
                return GetAppearanceRadioCircle(on);
            }

            PdfAppearance borderAppearance = GetBorderAppearance();
            if (!on)
            {
                return borderAppearance;
            }

            BaseFont realFont = RealFont;
            bool num = borderStyle == 2 || borderStyle == 3;
            float num2 = box.Height - borderWidth * 2f;
            float num3 = borderWidth;
            if (num)
            {
                num2 -= borderWidth * 2f;
                num3 *= 2f;
            }

            float val = (num ? (2f * borderWidth) : borderWidth);
            val = Math.Max(val, 1f);
            float num4 = Math.Min(num3, val);
            float num5 = box.Width - 2f * num4;
            float h = box.Height - 2f * num4;
            float num6 = fontSize;
            if (num6 == 0f)
            {
                float widthPoint = realFont.GetWidthPoint(text, 1f);
                num6 = ((widthPoint != 0f) ? (num5 / widthPoint) : 12f);
                float val2 = num2 / realFont.GetFontDescriptor(1, 1f);
                num6 = Math.Min(num6, val2);
            }

            borderAppearance.SaveState();
            borderAppearance.Rectangle(num4, num4, num5, h);
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
            borderAppearance.SetFontAndSize(realFont, num6);
            borderAppearance.SetTextMatrix((box.Width - realFont.GetWidthPoint(text, num6)) / 2f, (box.Height - realFont.GetAscentPoint(text, num6)) / 2f);
            borderAppearance.ShowText(text);
            borderAppearance.EndText();
            borderAppearance.RestoreState();
            return borderAppearance;
        }

        public virtual PdfAppearance GetAppearanceRadioCircle(bool on)
        {
            PdfAppearance pdfAppearance = PdfAppearance.CreateAppearance(writer, box.Width, box.Height);
            switch (rotation)
            {
                case 90:
                    pdfAppearance.SetMatrix(0f, 1f, -1f, 0f, box.Height, 0f);
                    break;
                case 180:
                    pdfAppearance.SetMatrix(-1f, 0f, 0f, -1f, box.Width, box.Height);
                    break;
                case 270:
                    pdfAppearance.SetMatrix(0f, -1f, 1f, 0f, 0f, box.Width);
                    break;
            }

            Rectangle rectangle = new Rectangle(pdfAppearance.BoundingBox);
            float x = rectangle.Width / 2f;
            float y = rectangle.Height / 2f;
            float num = (Math.Min(rectangle.Width, rectangle.Height) - borderWidth) / 2f;
            if (num <= 0f)
            {
                return pdfAppearance;
            }

            if (backgroundColor != null)
            {
                pdfAppearance.SetColorFill(backgroundColor);
                pdfAppearance.Circle(x, y, num + borderWidth / 2f);
                pdfAppearance.Fill();
            }

            if (borderWidth > 0f && borderColor != null)
            {
                pdfAppearance.SetLineWidth(borderWidth);
                pdfAppearance.SetColorStroke(borderColor);
                pdfAppearance.Circle(x, y, num);
                pdfAppearance.Stroke();
            }

            if (on)
            {
                if (textColor == null)
                {
                    pdfAppearance.ResetGrayFill();
                }
                else
                {
                    pdfAppearance.SetColorFill(textColor);
                }

                pdfAppearance.Circle(x, y, num / 2f);
                pdfAppearance.Fill();
            }

            return pdfAppearance;
        }

        public virtual PdfFormField GetRadioGroup(bool noToggleToOff, bool radiosInUnison)
        {
            PdfFormField pdfFormField = PdfFormField.CreateRadioButton(writer, noToggleToOff);
            if (radiosInUnison)
            {
                pdfFormField.SetFieldFlags(33554432);
            }

            pdfFormField.FieldName = fieldName;
            if (((uint)options & (true ? 1u : 0u)) != 0)
            {
                pdfFormField.SetFieldFlags(1);
            }

            if (((uint)options & 2u) != 0)
            {
                pdfFormField.SetFieldFlags(2);
            }

            pdfFormField.ValueAsName = (vchecked ? onValue : "Off");
            return pdfFormField;
        }

        protected virtual PdfFormField GetField(bool isRadio)
        {
            PdfFormField pdfFormField = null;
            pdfFormField = ((!isRadio) ? PdfFormField.CreateCheckBox(writer) : PdfFormField.CreateEmpty(writer));
            pdfFormField.SetWidget(box, PdfAnnotation.HIGHLIGHT_INVERT);
            if (!isRadio)
            {
                if (!"Yes".Equals(onValue))
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.name.for.checkbox.appearance", onValue));
                }

                pdfFormField.FieldName = fieldName;
                if (((uint)options & (true ? 1u : 0u)) != 0)
                {
                    pdfFormField.SetFieldFlags(1);
                }

                if (((uint)options & 2u) != 0)
                {
                    pdfFormField.SetFieldFlags(2);
                }

                pdfFormField.ValueAsName = (vchecked ? onValue : "Off");
                CheckType = checkType;
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
            PdfAppearance appearance = GetAppearance(isRadio, on: true);
            PdfAppearance appearance2 = GetAppearance(isRadio, on: false);
            pdfFormField.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, onValue, appearance);
            pdfFormField.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, "Off", appearance2);
            pdfFormField.AppearanceState = (vchecked ? onValue : "Off");
            PdfAppearance pdfAppearance = (PdfAppearance)appearance.Duplicate;
            BaseFont realFont = RealFont;
            if (realFont != null)
            {
                pdfAppearance.SetFontAndSize(realFont, fontSize);
            }

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

            return pdfFormField;
        }
    }
}
