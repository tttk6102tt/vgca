using Sign.itext.pdf;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class TextField : BaseField
    {
        private string defaultText;

        private string[] choices;

        private string[] choiceExports;

        private List<int> choiceSelections = new List<int>();

        private int topFirst;

        private int visibleTopChoice = -1;

        private float extraMarginLeft;

        private float extraMarginTop;

        private List<BaseFont> substitutionFonts;

        private BaseFont extensionFont;

        public virtual string DefaultText
        {
            get
            {
                return defaultText;
            }
            set
            {
                defaultText = value;
            }
        }

        public virtual string[] Choices
        {
            get
            {
                return choices;
            }
            set
            {
                choices = value;
            }
        }

        public virtual string[] ChoiceExports
        {
            get
            {
                return choiceExports;
            }
            set
            {
                choiceExports = value;
            }
        }

        public virtual int ChoiceSelection
        {
            get
            {
                return GetTopChoice();
            }
            set
            {
                choiceSelections = new List<int>();
                choiceSelections.Add(value);
            }
        }

        public virtual List<int> ChoiceSelections
        {
            get
            {
                return choiceSelections;
            }
            set
            {
                if (value != null)
                {
                    choiceSelections = new List<int>(value);
                    if (choiceSelections.Count > 1 && (options & 0x200000) == 0)
                    {
                        while (choiceSelections.Count > 1)
                        {
                            choiceSelections.RemoveAt(1);
                        }
                    }
                }
                else
                {
                    choiceSelections.Clear();
                }
            }
        }

        public virtual int VisibleTopChoice
        {
            get
            {
                return visibleTopChoice;
            }
            set
            {
                if (value >= 0 && choices != null && value < choices.Length)
                {
                    visibleTopChoice = value;
                }
            }
        }

        internal int TopFirst => topFirst;

        public virtual List<BaseFont> SubstitutionFonts
        {
            get
            {
                return substitutionFonts;
            }
            set
            {
                substitutionFonts = value;
            }
        }

        public virtual BaseFont ExtensionFont
        {
            get
            {
                return extensionFont;
            }
            set
            {
                extensionFont = value;
            }
        }

        public TextField(PdfWriter writer, Rectangle box, string fieldName)
            : base(writer, box, fieldName)
        {
        }

        private static bool CheckRTL(string text)
        {
            if (text == null || text.Length == 0)
            {
                return false;
            }

            char[] array = text.ToCharArray();
            foreach (int num in array)
            {
                if (num >= 1424 && num < 1920)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ChangeFontSize(Phrase p, float size)
        {
            foreach (Chunk item in p)
            {
                item.Font.Size = size;
            }
        }

        private Phrase ComposePhrase(string text, BaseFont ufont, BaseColor color, float fontSize)
        {
            Phrase phrase = null;
            if (extensionFont == null && (substitutionFonts == null || substitutionFonts.Count == 0))
            {
                return new Phrase(new Chunk(text, new Font(ufont, fontSize, 0, color)));
            }

            FontSelector fontSelector = new FontSelector();
            fontSelector.AddFont(new Font(ufont, fontSize, 0, color));
            if (extensionFont != null)
            {
                fontSelector.AddFont(new Font(extensionFont, fontSize, 0, color));
            }

            if (substitutionFonts != null)
            {
                foreach (BaseFont substitutionFont in substitutionFonts)
                {
                    fontSelector.AddFont(new Font(substitutionFont, fontSize, 0, color));
                }
            }

            return fontSelector.Process(text);
        }

        public static string RemoveCRLF(string text)
        {
            if (text.IndexOf('\n') >= 0 || text.IndexOf('\r') >= 0)
            {
                char[] array = text.ToCharArray();
                StringBuilder stringBuilder = new StringBuilder(array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    char c = array[i];
                    switch (c)
                    {
                        case '\n':
                            stringBuilder.Append(' ');
                            break;
                        case '\r':
                            stringBuilder.Append(' ');
                            if (i < array.Length - 1 && array[i + 1] == '\n')
                            {
                                i++;
                            }

                            break;
                        default:
                            stringBuilder.Append(c);
                            break;
                    }
                }

                return stringBuilder.ToString();
            }

            return text;
        }

        public static string ObfuscatePassword(string text)
        {
            return new string('*', text.Length);
        }

        public virtual PdfAppearance GetAppearance()
        {
            PdfAppearance borderAppearance = GetBorderAppearance();
            borderAppearance.BeginVariableText();
            if (base.text == null || base.text.Length == 0)
            {
                borderAppearance.EndVariableText();
                return borderAppearance;
            }

            bool num = borderStyle == 2 || borderStyle == 3;
            float num2 = box.Height - borderWidth * 2f - extraMarginTop;
            float num3 = borderWidth;
            if (num)
            {
                num2 -= borderWidth * 2f;
                num3 *= 2f;
            }

            float num4 = Math.Max(num3, 1f);
            float num5 = Math.Min(num3, num4);
            borderAppearance.SaveState();
            borderAppearance.Rectangle(num5, num5, box.Width - 2f * num5, box.Height - 2f * num5);
            borderAppearance.Clip();
            borderAppearance.NewPath();
            string text = ((((uint)options & 0x2000u) != 0) ? ObfuscatePassword(base.text) : ((((uint)options & 0x1000u) != 0) ? base.text : RemoveCRLF(base.text)));
            BaseFont realFont = RealFont;
            BaseColor color = ((textColor == null) ? GrayColor.GRAYBLACK : textColor);
            int runDirection = ((!CheckRTL(text)) ? 1 : 2);
            float num6 = fontSize;
            Phrase phrase = ComposePhrase(text, realFont, color, num6);
            if (((uint)options & 0x1000u) != 0)
            {
                float urx = box.Width - 4f * num4 - extraMarginLeft;
                float num7 = realFont.GetFontDescriptor(8, 1f) - realFont.GetFontDescriptor(6, 1f);
                ColumnText columnText = new ColumnText(null);
                if (num6 == 0f)
                {
                    num6 = num2 / num7;
                    if (num6 > 4f)
                    {
                        if (num6 > 12f)
                        {
                            num6 = 12f;
                        }

                        float num8 = Math.Max((num6 - 4f) / 10f, 0.2f);
                        columnText.SetSimpleColumn(0f, 0f - num2, urx, 0f);
                        columnText.Alignment = alignment;
                        columnText.RunDirection = runDirection;
                        while (num6 > 4f)
                        {
                            columnText.YLine = 0f;
                            ChangeFontSize(phrase, num6);
                            columnText.SetText(phrase);
                            columnText.Leading = num7 * num6;
                            if ((columnText.Go(simulate: true) & 2) == 0)
                            {
                                break;
                            }

                            num6 -= num8;
                        }
                    }

                    if (num6 < 4f)
                    {
                        num6 = 4f;
                    }
                }

                ChangeFontSize(phrase, num6);
                columnText.Canvas = borderAppearance;
                float num9 = num6 * num7;
                float num10 = num4 + num2 - realFont.GetFontDescriptor(8, num6);
                columnText.SetSimpleColumn(extraMarginLeft + 2f * num4, -20000f, box.Width - 2f * num4, num10 + num9);
                columnText.Leading = num9;
                columnText.Alignment = alignment;
                columnText.RunDirection = runDirection;
                columnText.SetText(phrase);
                columnText.Go();
            }
            else
            {
                if (num6 == 0f)
                {
                    float num11 = num2 / (realFont.GetFontDescriptor(7, 1f) - realFont.GetFontDescriptor(6, 1f));
                    ChangeFontSize(phrase, 1f);
                    float width = ColumnText.GetWidth(phrase, runDirection, 0);
                    num6 = ((width != 0f) ? Math.Min(num11, (box.Width - extraMarginLeft - 4f * num4) / width) : num11);
                    if (num6 < 4f)
                    {
                        num6 = 4f;
                    }
                }

                ChangeFontSize(phrase, num6);
                float num12 = num5 + (box.Height - 2f * num5 - realFont.GetFontDescriptor(1, num6)) / 2f;
                if (num12 < num5)
                {
                    num12 = num5;
                }

                if (num12 - num5 < 0f - realFont.GetFontDescriptor(3, num6))
                {
                    float val = 0f - realFont.GetFontDescriptor(3, num6) + num5;
                    float val2 = box.Height - num5 - realFont.GetFontDescriptor(1, num6);
                    num12 = Math.Min(val, Math.Max(num12, val2));
                }

                if ((options & 0x1000000) == 0 || maxCharacterLength <= 0)
                {
                    ColumnText.ShowTextAligned(x: alignment switch
                    {
                        2 => extraMarginLeft + box.Width - 2f * num4,
                        1 => extraMarginLeft + box.Width / 2f,
                        _ => extraMarginLeft + 2f * num4,
                    }, canvas: borderAppearance, alignment: alignment, phrase: phrase, y: num12 - extraMarginTop, rotation: 0f, runDirection: runDirection, arabicOptions: 0);
                }
                else
                {
                    int num13 = Math.Min(maxCharacterLength, text.Length);
                    int num14 = 0;
                    if (alignment == 2)
                    {
                        num14 = maxCharacterLength - num13;
                    }
                    else if (alignment == 1)
                    {
                        num14 = (maxCharacterLength - num13) / 2;
                    }

                    float num15 = (box.Width - extraMarginLeft) / (float)maxCharacterLength;
                    float num16 = num15 / 2f + (float)num14 * num15;
                    if (textColor == null)
                    {
                        borderAppearance.SetGrayFill(0f);
                    }
                    else
                    {
                        borderAppearance.SetColorFill(textColor);
                    }

                    borderAppearance.BeginText();
                    foreach (Chunk item in phrase)
                    {
                        BaseFont baseFont = item.Font.BaseFont;
                        borderAppearance.SetFontAndSize(baseFont, num6);
                        StringBuilder stringBuilder = item.Append("");
                        for (int i = 0; i < stringBuilder.Length; i++)
                        {
                            string text2 = stringBuilder.ToString(i, 1);
                            float widthPoint = baseFont.GetWidthPoint(text2, num6);
                            borderAppearance.SetTextMatrix(extraMarginLeft + num16 - widthPoint / 2f, num12 - extraMarginTop);
                            borderAppearance.ShowText(text2);
                            num16 += num15;
                        }
                    }

                    borderAppearance.EndText();
                }
            }

            borderAppearance.RestoreState();
            borderAppearance.EndVariableText();
            return borderAppearance;
        }

        internal PdfAppearance GetListAppearance()
        {
            PdfAppearance borderAppearance = GetBorderAppearance();
            if (choices == null || choices.Length == 0)
            {
                return borderAppearance;
            }

            borderAppearance.BeginVariableText();
            int topChoice = GetTopChoice();
            BaseFont realFont = RealFont;
            float num = fontSize;
            if (num == 0f)
            {
                num = 12f;
            }

            bool num2 = borderStyle == 2 || borderStyle == 3;
            float num3 = box.Height - borderWidth * 2f;
            float num4 = borderWidth;
            if (num2)
            {
                num3 -= borderWidth * 2f;
                num4 *= 2f;
            }

            float num5 = realFont.GetFontDescriptor(8, num) - realFont.GetFontDescriptor(6, num);
            int num6 = (int)(num3 / num5) + 1;
            int num7 = 0;
            int num8 = 0;
            num7 = topChoice;
            num8 = num7 + num6;
            if (num8 > choices.Length)
            {
                num8 = choices.Length;
            }

            topFirst = num7;
            borderAppearance.SaveState();
            borderAppearance.Rectangle(num4, num4, box.Width - 2f * num4, box.Height - 2f * num4);
            borderAppearance.Clip();
            borderAppearance.NewPath();
            BaseColor baseColor = ((textColor == null) ? GrayColor.GRAYBLACK : textColor);
            borderAppearance.SetColorFill(new BaseColor(10, 36, 106));
            for (int i = 0; i < choiceSelections.Count; i++)
            {
                int num9 = choiceSelections[i];
                if (num9 >= num7 && num9 <= num8)
                {
                    borderAppearance.Rectangle(num4, num4 + num3 - (float)(num9 - num7 + 1) * num5, box.Width - 2f * num4, num5);
                    borderAppearance.Fill();
                }
            }

            float x = num4 * 2f;
            float num10 = num4 + num3 - realFont.GetFontDescriptor(8, num);
            int num11 = num7;
            while (num11 < num8)
            {
                string text = choices[num11];
                int runDirection = ((!CheckRTL(text)) ? 1 : 2);
                text = RemoveCRLF(text);
                BaseColor color = (choiceSelections.Contains(num11) ? GrayColor.GRAYWHITE : baseColor);
                Phrase phrase = ComposePhrase(text, realFont, color, num);
                ColumnText.ShowTextAligned(borderAppearance, 0, phrase, x, num10, 0f, runDirection, 0);
                num11++;
                num10 -= num5;
            }

            borderAppearance.RestoreState();
            borderAppearance.EndVariableText();
            return borderAppearance;
        }

        public virtual PdfFormField GetTextField()
        {
            if (maxCharacterLength <= 0)
            {
                options &= -16777217;
            }

            if (((uint)options & 0x1000000u) != 0)
            {
                options &= -4097;
            }

            PdfFormField pdfFormField = PdfFormField.CreateTextField(writer, multiline: false, password: false, maxCharacterLength);
            pdfFormField.SetWidget(box, PdfAnnotation.HIGHLIGHT_INVERT);
            switch (alignment)
            {
                case 1:
                    pdfFormField.Quadding = 1;
                    break;
                case 2:
                    pdfFormField.Quadding = 2;
                    break;
            }

            if (rotation != 0)
            {
                pdfFormField.MKRotation = rotation;
            }

            if (fieldName != null)
            {
                pdfFormField.FieldName = fieldName;
                if (!"".Equals(text))
                {
                    pdfFormField.ValueAsString = text;
                }

                if (defaultText != null)
                {
                    pdfFormField.DefaultValueAsString = defaultText;
                }

                if (((uint)options & (true ? 1u : 0u)) != 0)
                {
                    pdfFormField.SetFieldFlags(1);
                }

                if (((uint)options & 2u) != 0)
                {
                    pdfFormField.SetFieldFlags(2);
                }

                if (((uint)options & 0x1000u) != 0)
                {
                    pdfFormField.SetFieldFlags(4096);
                }

                if (((uint)options & 0x800000u) != 0)
                {
                    pdfFormField.SetFieldFlags(8388608);
                }

                if (((uint)options & 0x2000u) != 0)
                {
                    pdfFormField.SetFieldFlags(8192);
                }

                if (((uint)options & 0x100000u) != 0)
                {
                    pdfFormField.SetFieldFlags(1048576);
                }

                if (((uint)options & 0x400000u) != 0)
                {
                    pdfFormField.SetFieldFlags(4194304);
                }

                if (((uint)options & 0x1000000u) != 0)
                {
                    pdfFormField.SetFieldFlags(16777216);
                }
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

            return pdfFormField;
        }

        public virtual PdfFormField GetComboField()
        {
            return GetChoiceField(isList: false);
        }

        public virtual PdfFormField GetListField()
        {
            return GetChoiceField(isList: true);
        }

        private int GetTopChoice()
        {
            if (choiceSelections == null || choiceSelections.Count == 0)
            {
                return 0;
            }

            int num = choiceSelections[0];
            int result = 0;
            if (choices != null)
            {
                if (visibleTopChoice != -1)
                {
                    return visibleTopChoice;
                }

                result = num;
                result = Math.Min(result, choices.Length);
                result = Math.Max(0, result);
            }

            return result;
        }

        protected virtual PdfFormField GetChoiceField(bool isList)
        {
            options &= -16781313;
            string[] array = choices;
            if (array == null)
            {
                array = new string[0];
            }

            int topChoice = GetTopChoice();
            if (array.Length > topChoice)
            {
                text = array[topChoice];
            }

            if (text == null)
            {
                text = "";
            }

            PdfFormField pdfFormField = null;
            string[,] array2 = null;
            if (choiceExports == null)
            {
                pdfFormField = ((!isList) ? PdfFormField.CreateCombo(writer, (options & 0x40000) != 0, array, topChoice) : PdfFormField.CreateList(writer, array, topChoice));
            }
            else
            {
                array2 = new string[array.Length, 2];
                for (int i = 0; i < array2.GetLength(0); i++)
                {
                    array2[i, 0] = (array2[i, 1] = array[i]);
                }

                int num = Math.Min(array.Length, choiceExports.Length);
                for (int j = 0; j < num; j++)
                {
                    if (choiceExports[j] != null)
                    {
                        array2[j, 0] = choiceExports[j];
                    }
                }

                pdfFormField = ((!isList) ? PdfFormField.CreateCombo(writer, (options & 0x40000) != 0, array2, topChoice) : PdfFormField.CreateList(writer, array2, topChoice));
            }

            pdfFormField.SetWidget(box, PdfAnnotation.HIGHLIGHT_INVERT);
            if (rotation != 0)
            {
                pdfFormField.MKRotation = rotation;
            }

            if (fieldName != null)
            {
                pdfFormField.FieldName = fieldName;
                if (array.Length != 0)
                {
                    if (array2 != null)
                    {
                        if (choiceSelections.Count < 2)
                        {
                            pdfFormField.ValueAsString = array2[topChoice, 0];
                            pdfFormField.DefaultValueAsString = array2[topChoice, 0];
                        }
                        else
                        {
                            WriteMultipleValues(pdfFormField, array2);
                        }
                    }
                    else if (choiceSelections.Count < 2)
                    {
                        pdfFormField.ValueAsString = text;
                        pdfFormField.DefaultValueAsString = text;
                    }
                    else
                    {
                        WriteMultipleValues(pdfFormField, null);
                    }
                }

                if (((uint)options & (true ? 1u : 0u)) != 0)
                {
                    pdfFormField.SetFieldFlags(1);
                }

                if (((uint)options & 2u) != 0)
                {
                    pdfFormField.SetFieldFlags(2);
                }

                if (((uint)options & 0x400000u) != 0)
                {
                    pdfFormField.SetFieldFlags(4194304);
                }

                if (((uint)options & 0x200000u) != 0)
                {
                    pdfFormField.SetFieldFlags(2097152);
                }
            }

            pdfFormField.BorderStyle = new PdfBorderDictionary(borderWidth, borderStyle, new PdfDashPattern(3f));
            PdfAppearance pdfAppearance;
            if (isList)
            {
                pdfAppearance = GetListAppearance();
                if (topFirst > 0)
                {
                    pdfFormField.Put(PdfName.TI, new PdfNumber(topFirst));
                }
            }
            else
            {
                pdfAppearance = GetAppearance();
            }

            pdfFormField.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, pdfAppearance);
            PdfAppearance pdfAppearance2 = (PdfAppearance)pdfAppearance.Duplicate;
            pdfAppearance2.SetFontAndSize(RealFont, fontSize);
            if (textColor == null)
            {
                pdfAppearance2.SetGrayFill(0f);
            }
            else
            {
                pdfAppearance2.SetColorFill(textColor);
            }

            pdfFormField.DefaultAppearanceString = pdfAppearance2;
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

        private void WriteMultipleValues(PdfFormField field, string[,] mix)
        {
            PdfArray pdfArray = new PdfArray();
            PdfArray pdfArray2 = new PdfArray();
            for (int i = 0; i < choiceSelections.Count; i++)
            {
                int num = choiceSelections[i];
                pdfArray.Add(new PdfNumber(num));
                if (mix != null)
                {
                    pdfArray2.Add(new PdfString(mix[num, 0]));
                }
                else if (choices != null)
                {
                    pdfArray2.Add(new PdfString(choices[num]));
                }
            }

            field.Put(PdfName.V, pdfArray2);
            field.Put(PdfName.I, pdfArray);
        }

        public virtual void AddChoiceSelection(int selection)
        {
            if (((uint)options & 0x200000u) != 0)
            {
                choiceSelections.Add(selection);
            }
        }

        public virtual void SetExtraMargin(float extraMarginLeft, float extraMarginTop)
        {
            this.extraMarginLeft = extraMarginLeft;
            this.extraMarginTop = extraMarginTop;
        }
    }
}
