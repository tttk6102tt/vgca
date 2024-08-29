using Sign.itext.error_messages;
using Sign.itext.pdf;
using System.Text;

namespace Sign.itext.text.pdf
{
    public abstract class BaseField
    {
        public const float BORDER_WIDTH_THIN = 1f;

        public const float BORDER_WIDTH_MEDIUM = 2f;

        public const float BORDER_WIDTH_THICK = 3f;

        public const int VISIBLE = 0;

        public const int HIDDEN = 1;

        public const int VISIBLE_BUT_DOES_NOT_PRINT = 2;

        public const int HIDDEN_BUT_PRINTABLE = 3;

        public const int READ_ONLY = 1;

        public const int REQUIRED = 2;

        public const int MULTILINE = 4096;

        public const int DO_NOT_SCROLL = 8388608;

        public const int PASSWORD = 8192;

        public const int FILE_SELECTION = 1048576;

        public const int DO_NOT_SPELL_CHECK = 4194304;

        public const int EDIT = 262144;

        public const int MULTISELECT = 2097152;

        public const int COMB = 16777216;

        protected float borderWidth = 1f;

        protected int borderStyle;

        protected BaseColor borderColor;

        protected BaseColor backgroundColor;

        protected BaseColor textColor;

        protected BaseFont font;

        protected float fontSize;

        protected int alignment;

        protected PdfWriter writer;

        protected string text;

        protected Rectangle box;

        protected int rotation;

        protected int visibility;

        protected string fieldName;

        protected int options;

        protected int maxCharacterLength;

        private static Dictionary<PdfName, int> fieldKeys;

        protected virtual BaseFont RealFont
        {
            get
            {
                if (font == null)
                {
                    return BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
                }

                return font;
            }
        }

        public virtual float BorderWidth
        {
            get
            {
                return borderWidth;
            }
            set
            {
                borderWidth = value;
            }
        }

        public virtual int BorderStyle
        {
            get
            {
                return borderStyle;
            }
            set
            {
                borderStyle = value;
            }
        }

        public virtual BaseColor BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                borderColor = value;
            }
        }

        public virtual BaseColor BackgroundColor
        {
            get
            {
                return backgroundColor;
            }
            set
            {
                backgroundColor = value;
            }
        }

        public virtual BaseColor TextColor
        {
            get
            {
                return textColor;
            }
            set
            {
                textColor = value;
            }
        }

        public virtual BaseFont Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
            }
        }

        public virtual float FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                fontSize = value;
            }
        }

        public virtual int Alignment
        {
            get
            {
                return alignment;
            }
            set
            {
                alignment = value;
            }
        }

        public virtual string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public virtual Rectangle Box
        {
            get
            {
                return box;
            }
            set
            {
                if (value == null)
                {
                    box = null;
                    return;
                }

                box = new Rectangle(value);
                box.Normalize();
            }
        }

        public virtual int Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                if (value % 90 != 0)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("rotation.must.be.a.multiple.of.90"));
                }

                rotation = value % 360;
                if (rotation < 0)
                {
                    rotation += 360;
                }
            }
        }

        public virtual int Visibility
        {
            get
            {
                return visibility;
            }
            set
            {
                visibility = value;
            }
        }

        public virtual string FieldName
        {
            get
            {
                return fieldName;
            }
            set
            {
                fieldName = value;
            }
        }

        public virtual int Options
        {
            get
            {
                return options;
            }
            set
            {
                options = value;
            }
        }

        public virtual int MaxCharacterLength
        {
            get
            {
                return maxCharacterLength;
            }
            set
            {
                maxCharacterLength = value;
            }
        }

        public virtual PdfWriter Writer
        {
            get
            {
                return writer;
            }
            set
            {
                writer = value;
            }
        }

        static BaseField()
        {
            fieldKeys = new Dictionary<PdfName, int>();
            foreach (KeyValuePair<PdfName, int> fieldKey in PdfCopyFieldsImp.fieldKeys)
            {
                fieldKeys[fieldKey.Key] = fieldKey.Value;
            }

            fieldKeys[PdfName.T] = 1;
        }

        public BaseField(PdfWriter writer, Rectangle box, string fieldName)
        {
            this.writer = writer;
            Box = box;
            this.fieldName = fieldName;
        }

        protected virtual PdfAppearance GetBorderAppearance()
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

            pdfAppearance.SaveState();
            if (backgroundColor != null)
            {
                pdfAppearance.SetColorFill(backgroundColor);
                pdfAppearance.Rectangle(0f, 0f, box.Width, box.Height);
                pdfAppearance.Fill();
            }

            if (borderStyle == 4)
            {
                if (borderWidth != 0f && borderColor != null)
                {
                    pdfAppearance.SetColorStroke(borderColor);
                    pdfAppearance.SetLineWidth(borderWidth);
                    pdfAppearance.MoveTo(0f, borderWidth / 2f);
                    pdfAppearance.LineTo(box.Width, borderWidth / 2f);
                    pdfAppearance.Stroke();
                }
            }
            else if (borderStyle == 2)
            {
                if (borderWidth != 0f && borderColor != null)
                {
                    pdfAppearance.SetColorStroke(borderColor);
                    pdfAppearance.SetLineWidth(borderWidth);
                    pdfAppearance.Rectangle(borderWidth / 2f, borderWidth / 2f, box.Width - borderWidth, box.Height - borderWidth);
                    pdfAppearance.Stroke();
                }

                BaseColor wHITE = backgroundColor;
                if (wHITE == null)
                {
                    wHITE = BaseColor.WHITE;
                }

                pdfAppearance.SetGrayFill(1f);
                DrawTopFrame(pdfAppearance);
                pdfAppearance.SetColorFill(wHITE.Darker());
                DrawBottomFrame(pdfAppearance);
            }
            else if (borderStyle == 3)
            {
                if (borderWidth != 0f && borderColor != null)
                {
                    pdfAppearance.SetColorStroke(borderColor);
                    pdfAppearance.SetLineWidth(borderWidth);
                    pdfAppearance.Rectangle(borderWidth / 2f, borderWidth / 2f, box.Width - borderWidth, box.Height - borderWidth);
                    pdfAppearance.Stroke();
                }

                pdfAppearance.SetGrayFill(0.5f);
                DrawTopFrame(pdfAppearance);
                pdfAppearance.SetGrayFill(0.75f);
                DrawBottomFrame(pdfAppearance);
            }
            else if (borderWidth != 0f && borderColor != null)
            {
                if (borderStyle == 1)
                {
                    pdfAppearance.SetLineDash(3f, 0f);
                }

                pdfAppearance.SetColorStroke(borderColor);
                pdfAppearance.SetLineWidth(borderWidth);
                pdfAppearance.Rectangle(borderWidth / 2f, borderWidth / 2f, box.Width - borderWidth, box.Height - borderWidth);
                pdfAppearance.Stroke();
                if (((uint)options & 0x1000000u) != 0 && maxCharacterLength > 1)
                {
                    float num = box.Width / (float)maxCharacterLength;
                    float y = borderWidth / 2f;
                    float y2 = box.Height - borderWidth / 2f;
                    for (int i = 1; i < maxCharacterLength; i++)
                    {
                        float x = num * (float)i;
                        pdfAppearance.MoveTo(x, y);
                        pdfAppearance.LineTo(x, y2);
                    }

                    pdfAppearance.Stroke();
                }
            }

            pdfAppearance.RestoreState();
            return pdfAppearance;
        }

        protected static List<string> GetHardBreaks(string text)
        {
            List<string> list = new List<string>();
            char[] array = text.ToCharArray();
            int num = array.Length;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < num; i++)
            {
                char c = array[i];
                switch (c)
                {
                    case '\r':
                        if (i + 1 < num && array[i + 1] == '\n')
                        {
                            i++;
                        }

                        list.Add(stringBuilder.ToString());
                        stringBuilder = new StringBuilder();
                        break;
                    case '\n':
                        list.Add(stringBuilder.ToString());
                        stringBuilder = new StringBuilder();
                        break;
                    default:
                        stringBuilder.Append(c);
                        break;
                }
            }

            list.Add(stringBuilder.ToString());
            return list;
        }

        protected static void TrimRight(StringBuilder buf)
        {
            int num = buf.Length;
            while (num != 0 && buf[--num] == ' ')
            {
                buf.Length = num;
            }
        }

        protected static List<string> BreakLines(List<string> breaks, BaseFont font, float fontSize, float width)
        {
            List<string> list = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < breaks.Count; i++)
            {
                stringBuilder.Length = 0;
                float num = 0f;
                char[] array = breaks[i].ToCharArray();
                int num2 = array.Length;
                int num3 = 0;
                int num4 = -1;
                char c = '\0';
                int num5 = 0;
                for (int j = 0; j < num2; j++)
                {
                    c = array[j];
                    switch (num3)
                    {
                        case 0:
                            num += font.GetWidthPoint(c, fontSize);
                            stringBuilder.Append(c);
                            if (num > width)
                            {
                                num = 0f;
                                if (stringBuilder.Length > 1)
                                {
                                    j--;
                                    stringBuilder.Length--;
                                }

                                list.Add(stringBuilder.ToString());
                                stringBuilder.Length = 0;
                                num5 = j;
                                num3 = ((c != ' ') ? 1 : 2);
                            }
                            else if (c != ' ')
                            {
                                num3 = 1;
                            }

                            break;
                        case 1:
                            num += font.GetWidthPoint(c, fontSize);
                            stringBuilder.Append(c);
                            if (c == ' ')
                            {
                                num4 = j;
                            }

                            if (!(num > width))
                            {
                                break;
                            }

                            num = 0f;
                            if (num4 >= 0)
                            {
                                j = num4;
                                stringBuilder.Length = num4 - num5;
                                TrimRight(stringBuilder);
                                list.Add(stringBuilder.ToString());
                                stringBuilder.Length = 0;
                                num5 = j;
                                num4 = -1;
                                num3 = 2;
                                break;
                            }

                            if (stringBuilder.Length > 1)
                            {
                                j--;
                                stringBuilder.Length--;
                            }

                            list.Add(stringBuilder.ToString());
                            stringBuilder.Length = 0;
                            num5 = j;
                            if (c == ' ')
                            {
                                num3 = 2;
                            }

                            break;
                        case 2:
                            if (c != ' ')
                            {
                                num = 0f;
                                j--;
                                num3 = 1;
                            }

                            break;
                    }
                }

                TrimRight(stringBuilder);
                list.Add(stringBuilder.ToString());
            }

            return list;
        }

        private void DrawTopFrame(PdfAppearance app)
        {
            app.MoveTo(borderWidth, borderWidth);
            app.LineTo(borderWidth, box.Height - borderWidth);
            app.LineTo(box.Width - borderWidth, box.Height - borderWidth);
            app.LineTo(box.Width - 2f * borderWidth, box.Height - 2f * borderWidth);
            app.LineTo(2f * borderWidth, box.Height - 2f * borderWidth);
            app.LineTo(2f * borderWidth, 2f * borderWidth);
            app.LineTo(borderWidth, borderWidth);
            app.Fill();
        }

        private void DrawBottomFrame(PdfAppearance app)
        {
            app.MoveTo(borderWidth, borderWidth);
            app.LineTo(box.Width - borderWidth, borderWidth);
            app.LineTo(box.Width - borderWidth, box.Height - borderWidth);
            app.LineTo(box.Width - 2f * borderWidth, box.Height - 2f * borderWidth);
            app.LineTo(box.Width - 2f * borderWidth, 2f * borderWidth);
            app.LineTo(2f * borderWidth, 2f * borderWidth);
            app.LineTo(borderWidth, borderWidth);
            app.Fill();
        }

        public virtual void SetRotationFromPage(Rectangle page)
        {
            Rotation = page.Rotation;
        }

        public static void MoveFields(PdfDictionary from, PdfDictionary to)
        {
            PdfName[] array = new PdfName[from.Size];
            from.Keys.CopyTo(array, 0);
            PdfName[] array2 = array;
            foreach (PdfName key in array2)
            {
                if (fieldKeys.ContainsKey(key))
                {
                    to?.Put(key, from.Get(key));
                    from.Remove(key);
                }
            }
        }
    }
}
