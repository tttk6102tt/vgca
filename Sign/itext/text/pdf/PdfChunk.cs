using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;

namespace Sign.itext.text.pdf
{
    public class PdfChunk
    {
        private static char[] singleSpace;

        private static PdfChunk[] thisChunk;

        private const float ITALIC_ANGLE = 0.21256f;

        private static Dictionary<string, object> keysAttributes;

        private static Dictionary<string, object> keysNoStroke;

        private const string TABSTOP = "TABSTOP";

        protected string value = "";

        protected string encoding = "Cp1252";

        protected PdfFont font;

        protected BaseFont baseFont;

        protected ISplitCharacter splitCharacter;

        protected Dictionary<string, object> attributes = new Dictionary<string, object>();

        protected Dictionary<string, object> noStroke = new Dictionary<string, object>();

        protected bool newlineSplit;

        protected Image image;

        protected float imageScalePercentage = 1f;

        protected float offsetX;

        protected float offsetY;

        protected bool changeLeading;

        protected float leading;

        internal IAccessibleElement accessibleElement;

        public const float UNDERLINE_THICKNESS = 71f / (339f * (float)Math.PI);

        public const float UNDERLINE_OFFSET = -0.333333343f;

        internal PdfFont Font => font;

        internal BaseColor Color
        {
            get
            {
                if (noStroke.ContainsKey("COLOR"))
                {
                    return (BaseColor)noStroke["COLOR"];
                }

                return null;
            }
        }

        public virtual float TextRise
        {
            get
            {
                object attribute = GetAttribute("SUBSUPSCRIPT");
                if (attribute != null)
                {
                    return (float)attribute;
                }

                return 0f;
            }
        }

        internal TabStop TabStop
        {
            get
            {
                if (attributes.TryGetValue("TABSTOP", out var obj))
                {
                    return (TabStop)obj;
                }

                return null;
            }
            set
            {
                attributes["TABSTOP"] = value;
            }
        }

        internal Image Image => image;

        internal float ImageHeight => image.ScaledHeight * imageScalePercentage;

        internal float ImageWidth => image.ScaledWidth * imageScalePercentage;

        public virtual float ImageScalePercentage
        {
            get
            {
                return imageScalePercentage;
            }
            set
            {
                imageScalePercentage = value;
            }
        }

        internal float ImageOffsetX
        {
            get
            {
                return offsetX;
            }
            set
            {
                offsetX = value;
            }
        }

        internal float ImageOffsetY
        {
            get
            {
                return offsetY;
            }
            set
            {
                offsetY = value;
            }
        }

        internal string Value
        {
            set
            {
                this.value = value;
            }
        }

        internal string Encoding => encoding;

        internal int Length => value.Length;

        internal int LengthUtf32
        {
            get
            {
                if (!"Identity-H".Equals(encoding))
                {
                    return value.Length;
                }

                int num = 0;
                int length = value.Length;
                for (int i = 0; i < length; i++)
                {
                    if (Utilities.IsSurrogateHigh(value[i]))
                    {
                        i++;
                    }

                    num++;
                }

                return num;
            }
        }

        public virtual bool ChangeLeading => changeLeading;

        public virtual float Leading => leading;

        static PdfChunk()
        {
            singleSpace = new char[1] { ' ' };
            thisChunk = new PdfChunk[1];
            keysAttributes = new Dictionary<string, object>();
            keysNoStroke = new Dictionary<string, object>();
            keysAttributes.Add("ACTION", null);
            keysAttributes.Add("UNDERLINE", null);
            keysAttributes.Add("REMOTEGOTO", null);
            keysAttributes.Add("LOCALGOTO", null);
            keysAttributes.Add("LOCALDESTINATION", null);
            keysAttributes.Add("GENERICTAG", null);
            keysAttributes.Add("NEWPAGE", null);
            keysAttributes.Add("IMAGE", null);
            keysAttributes.Add("BACKGROUND", null);
            keysAttributes.Add("PDFANNOTATION", null);
            keysAttributes.Add("SKEW", null);
            keysAttributes.Add("HSCALE", null);
            keysAttributes.Add("SEPARATOR", null);
            keysAttributes.Add("TAB", null);
            keysAttributes.Add("TABSETTINGS", null);
            keysAttributes.Add("CHAR_SPACING", null);
            keysAttributes.Add("WORD_SPACING", null);
            keysAttributes.Add("LINEHEIGHT", null);
            keysNoStroke.Add("SUBSUPSCRIPT", null);
            keysNoStroke.Add("SPLITCHARACTER", null);
            keysNoStroke.Add("HYPHENATION", null);
            keysNoStroke.Add("TEXTRENDERMODE", null);
        }

        internal PdfChunk(string str, PdfChunk other)
        {
            thisChunk[0] = this;
            value = str;
            font = other.font;
            attributes = other.attributes;
            noStroke = other.noStroke;
            baseFont = other.baseFont;
            changeLeading = other.changeLeading;
            leading = other.leading;
            object[] array = null;
            if (attributes.ContainsKey("IMAGE"))
            {
                array = (object[])attributes["IMAGE"];
            }

            if (array == null)
            {
                image = null;
            }
            else
            {
                image = (Image)array[0];
                offsetX = (float)array[1];
                offsetY = (float)array[2];
                changeLeading = (bool)array[3];
            }

            encoding = font.Font.Encoding;
            if (noStroke.ContainsKey("SPLITCHARACTER"))
            {
                splitCharacter = (ISplitCharacter)noStroke["SPLITCHARACTER"];
            }
            else
            {
                splitCharacter = DefaultSplitCharacter.DEFAULT;
            }

            accessibleElement = other.accessibleElement;
        }

        internal PdfChunk(Chunk chunk, PdfAction action)
        {
            thisChunk[0] = this;
            value = chunk.Content;
            Font font = chunk.Font;
            float num = font.Size;
            if (num == -1f)
            {
                num = 12f;
            }

            baseFont = font.BaseFont;
            int num2 = font.Style;
            if (num2 == -1)
            {
                num2 = 0;
            }

            if (baseFont == null)
            {
                baseFont = font.GetCalculatedBaseFont(specialEncoding: false);
            }
            else
            {
                if (((uint)num2 & (true ? 1u : 0u)) != 0)
                {
                    attributes["TEXTRENDERMODE"] = new object[3]
                    {
                        2,
                        num / 30f,
                        null
                    };
                }

                if (((uint)num2 & 2u) != 0)
                {
                    attributes["SKEW"] = new float[2] { 0f, 0.21256f };
                }
            }

            this.font = new PdfFont(baseFont, num);
            Dictionary<string, object> dictionary = chunk.Attributes;
            if (dictionary != null)
            {
                foreach (KeyValuePair<string, object> item3 in dictionary)
                {
                    string key = item3.Key;
                    if (keysAttributes.ContainsKey(key))
                    {
                        attributes[key] = item3.Value;
                    }
                    else if (keysNoStroke.ContainsKey(key))
                    {
                        noStroke[key] = item3.Value;
                    }
                }

                if (dictionary.ContainsKey("GENERICTAG") && "".Equals(dictionary["GENERICTAG"]))
                {
                    attributes["GENERICTAG"] = chunk.Content;
                }
            }

            if (font.IsUnderlined())
            {
                object[] item = new object[2]
                {
                    null,
                    new float[5]
                    {
                        0f,
                        71f / (339f * (float)Math.PI),
                        0f,
                        -0.333333343f,
                        0f
                    }
                };
                object[][] original = null;
                if (attributes.ContainsKey("UNDERLINE"))
                {
                    original = (object[][])attributes["UNDERLINE"];
                }

                object[][] array = Utilities.AddToArray(original, item);
                attributes["UNDERLINE"] = array;
            }

            if (font.IsStrikethru())
            {
                object[] item2 = new object[2]
                {
                    null,
                    new float[5]
                    {
                        0f,
                        71f / (339f * (float)Math.PI),
                        0f,
                        0.333333343f,
                        0f
                    }
                };
                object[][] original2 = null;
                if (attributes.ContainsKey("UNDERLINE"))
                {
                    original2 = (object[][])attributes["UNDERLINE"];
                }

                object[][] array2 = Utilities.AddToArray(original2, item2);
                attributes["UNDERLINE"] = array2;
            }

            if (action != null)
            {
                attributes["ACTION"] = action;
            }

            noStroke["COLOR"] = font.Color;
            noStroke["ENCODING"] = this.font.Font.Encoding;
            if (attributes.TryGetValue("LINEHEIGHT", out var obj))
            {
                changeLeading = true;
                leading = (float)obj;
            }

            object[] array3 = null;
            if (attributes.ContainsKey("IMAGE"))
            {
                array3 = (object[])attributes["IMAGE"];
            }

            if (array3 == null)
            {
                image = null;
            }
            else
            {
                attributes.Remove("HSCALE");
                image = (Image)array3[0];
                offsetX = (float)array3[1];
                offsetY = (float)array3[2];
                changeLeading = (bool)array3[3];
            }

            if (attributes.TryGetValue("HSCALE", out var obj2))
            {
                this.font.HorizontalScaling = (float)obj2;
            }

            encoding = this.font.Font.Encoding;
            if (noStroke.ContainsKey("SPLITCHARACTER"))
            {
                splitCharacter = (ISplitCharacter)noStroke["SPLITCHARACTER"];
            }
            else
            {
                splitCharacter = DefaultSplitCharacter.DEFAULT;
            }

            accessibleElement = chunk;
        }

        internal PdfChunk(Chunk chunk, PdfAction action, TabSettings tabSettings)
            : this(chunk, action)
        {
            if (tabSettings != null && (!attributes.ContainsKey("TABSETTINGS") || attributes["TABSETTINGS"] == null))
            {
                attributes["TABSETTINGS"] = tabSettings;
            }
        }

        public virtual int GetUnicodeEquivalent(int c)
        {
            return baseFont.GetUnicodeEquivalent(c);
        }

        protected virtual int GetWord(string text, int start)
        {
            int length = text.Length;
            while (start < length && char.IsLetter(text[start]))
            {
                start++;
            }

            return start;
        }

        internal PdfChunk Split(float width)
        {
            newlineSplit = false;
            if (image != null)
            {
                if (image.ScaledWidth > width)
                {
                    PdfChunk result = new PdfChunk("￼", this);
                    value = "";
                    attributes = new Dictionary<string, object>();
                    image = null;
                    font = PdfFont.DefaultFont;
                    return result;
                }

                return null;
            }

            IHyphenationEvent hyphenationEvent = null;
            if (noStroke.ContainsKey("HYPHENATION"))
            {
                hyphenationEvent = (IHyphenationEvent)noStroke["HYPHENATION"];
            }

            int i = 0;
            int num = -1;
            float num2 = 0f;
            int num3 = -1;
            float num4 = 0f;
            int length = value.Length;
            char[] array = value.ToCharArray();
            char c = '\0';
            BaseFont baseFont = font.Font;
            if (baseFont.FontType == 2 && baseFont.GetUnicodeEquivalent(32) != 32)
            {
                for (; i < length; i++)
                {
                    char c2 = array[i];
                    c = (char)baseFont.GetUnicodeEquivalent(c2);
                    if (c == '\n')
                    {
                        newlineSplit = true;
                        string str = value.Substring(i + 1);
                        value = value.Substring(0, i);
                        if (value.Length < 1)
                        {
                            value = "\u0001";
                        }

                        return new PdfChunk(str, this);
                    }

                    num2 += GetCharWidth(c2);
                    if (c == ' ')
                    {
                        num3 = i + 1;
                        num4 = num2;
                    }

                    if (num2 > width)
                    {
                        break;
                    }

                    if (splitCharacter.IsSplitCharacter(0, i, length, array, thisChunk))
                    {
                        num = i + 1;
                    }
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    c = array[i];
                    if (c == '\r' || c == '\n')
                    {
                        newlineSplit = true;
                        int num5 = 1;
                        if (c == '\r' && i + 1 < length && array[i + 1] == '\n')
                        {
                            num5 = 2;
                        }

                        string str2 = value.Substring(i + num5);
                        value = value.Substring(0, i);
                        if (value.Length < 1)
                        {
                            value = " ";
                        }

                        return new PdfChunk(str2, this);
                    }

                    bool num6 = Utilities.IsSurrogatePair(array, i);
                    num2 = ((!num6) ? (num2 + GetCharWidth(c)) : (num2 + GetCharWidth(Utilities.ConvertToUtf32(array[i], array[i + 1]))));
                    if (c == ' ')
                    {
                        num3 = i + 1;
                        num4 = num2;
                    }

                    if (num6)
                    {
                        i++;
                    }

                    if (num2 > width)
                    {
                        break;
                    }

                    if (splitCharacter.IsSplitCharacter(0, i, length, array, null))
                    {
                        num = i + 1;
                    }
                }
            }

            if (i == length)
            {
                return null;
            }

            if (num < 0)
            {
                string str3 = value;
                value = "";
                return new PdfChunk(str3, this);
            }

            if (num3 > num && splitCharacter.IsSplitCharacter(0, 0, 1, singleSpace, null))
            {
                num = num3;
            }

            if (hyphenationEvent != null && num3 >= 0 && num3 < i)
            {
                int word = GetWord(value, num3);
                if (word > num3)
                {
                    string hyphenatedWordPre = hyphenationEvent.GetHyphenatedWordPre(value.Substring(num3, word - num3), font.Font, font.Size, width - num4);
                    string hyphenatedWordPost = hyphenationEvent.HyphenatedWordPost;
                    if (hyphenatedWordPre.Length > 0)
                    {
                        string str4 = hyphenatedWordPost + value.Substring(word);
                        value = Trim(value.Substring(0, num3) + hyphenatedWordPre);
                        return new PdfChunk(str4, this);
                    }
                }
            }

            string str5 = value.Substring(num);
            value = Trim(value.Substring(0, num));
            return new PdfChunk(str5, this);
        }

        internal PdfChunk Truncate(float width)
        {
            if (image != null)
            {
                if (image.ScaledWidth > width)
                {
                    if (image.ScaleToFitLineWhenOverflow)
                    {
                        ImageScalePercentage = width / image.Width;
                        return null;
                    }

                    PdfChunk result = new PdfChunk("", this);
                    value = "";
                    attributes.Remove("IMAGE");
                    image = null;
                    font = PdfFont.DefaultFont;
                    return result;
                }

                return null;
            }

            int i = 0;
            float num = 0f;
            if (width < font.Width())
            {
                string str = value.Substring(1);
                value = value.Substring(0, 1);
                return new PdfChunk(str, this);
            }

            int length = value.Length;
            bool flag = false;
            for (; i < length; i++)
            {
                flag = Utilities.IsSurrogatePair(value, i);
                num = ((!flag) ? (num + GetCharWidth(value[i])) : (num + GetCharWidth(Utilities.ConvertToUtf32(value, i))));
                if (num > width)
                {
                    break;
                }

                if (flag)
                {
                    i++;
                }
            }

            if (i == length)
            {
                return null;
            }

            if (i == 0)
            {
                i = 1;
                if (flag)
                {
                    i++;
                }
            }

            string str2 = value.Substring(i);
            value = value.Substring(0, i);
            return new PdfChunk(str2, this);
        }

        internal float Width()
        {
            return Width(value);
        }

        internal float Width(string str)
        {
            if (IsAttribute("SEPARATOR"))
            {
                return 0f;
            }

            if (IsImage())
            {
                return ImageWidth;
            }

            float num = font.Width(str);
            if (IsAttribute("CHAR_SPACING"))
            {
                float num2 = (float)GetAttribute("CHAR_SPACING");
                num += (float)str.Length * num2;
            }

            if (IsAttribute("WORD_SPACING"))
            {
                int num3 = 0;
                int num4 = -1;
                while ((num4 = str.IndexOf(' ', num4 + 1)) >= 0)
                {
                    num3++;
                }

                float num5 = (float)GetAttribute("WORD_SPACING");
                num += (float)num3 * num5;
            }

            return num;
        }

        internal float Height()
        {
            if (!IsImage())
            {
                return font.Size;
            }

            return ImageHeight;
        }

        public virtual bool IsNewlineSplit()
        {
            return newlineSplit;
        }

        public virtual float GetWidthCorrected(float charSpacing, float wordSpacing)
        {
            if (image != null)
            {
                return image.ScaledWidth + charSpacing;
            }

            int num = 0;
            int num2 = -1;
            while ((num2 = value.IndexOf(' ', num2 + 1)) >= 0)
            {
                num++;
            }

            return font.Width(value) + (float)value.Length * charSpacing + (float)num * wordSpacing;
        }

        public virtual float TrimLastSpace()
        {
            BaseFont baseFont = font.Font;
            if (baseFont.FontType == 2 && baseFont.GetUnicodeEquivalent(32) != 32)
            {
                if (value.Length > 1 && value.EndsWith("\u0001"))
                {
                    value = value.Substring(0, value.Length - 1);
                    return font.Width(1);
                }
            }
            else if (value.Length > 1 && value.EndsWith(" "))
            {
                value = value.Substring(0, value.Length - 1);
                return font.Width(32);
            }

            return 0f;
        }

        public virtual float TrimFirstSpace()
        {
            BaseFont baseFont = font.Font;
            if (baseFont.FontType == 2 && baseFont.GetUnicodeEquivalent(32) != 32)
            {
                if (value.Length > 1 && value.StartsWith("\u0001"))
                {
                    value = value.Substring(1);
                    return font.Width(1);
                }
            }
            else if (value.Length > 1 && value.StartsWith(" "))
            {
                value = value.Substring(1);
                return font.Width(32);
            }

            return 0f;
        }

        internal object GetAttribute(string name)
        {
            if (attributes.ContainsKey(name))
            {
                return attributes[name];
            }

            if (noStroke.ContainsKey(name))
            {
                return noStroke[name];
            }

            return null;
        }

        internal bool IsAttribute(string name)
        {
            if (attributes.ContainsKey(name))
            {
                return true;
            }

            return noStroke.ContainsKey(name);
        }

        internal bool IsStroked()
        {
            return attributes.Count > 0;
        }

        internal bool IsSeparator()
        {
            return IsAttribute("SEPARATOR");
        }

        internal bool IsHorizontalSeparator()
        {
            if (IsAttribute("SEPARATOR"))
            {
                return !(bool)((object[])GetAttribute("SEPARATOR"))[1];
            }

            return false;
        }

        internal bool IsTab()
        {
            return IsAttribute("TAB");
        }

        [Obsolete]
        internal void AdjustLeft(float newValue)
        {
            if (attributes.ContainsKey("TAB"))
            {
                object[] array = (object[])attributes["TAB"];
                attributes["TAB"] = new object[4]
                {
                    array[0],
                    array[1],
                    array[2],
                    newValue
                };
            }
        }

        internal static TabStop GetTabStop(PdfChunk tab, float tabPosition)
        {
            TabStop result = null;
            if (tab.attributes.TryGetValue("TAB", out var obj))
            {
                float num = (float)((object[])obj)[0];
                if (float.IsNaN(num))
                {
                    tab.attributes.TryGetValue("TABSETTINGS", out var obj2);
                    result = TabSettings.getTabStopNewInstance(tabPosition, (TabSettings)obj2);
                }
                else
                {
                    result = TabStop.NewInstance(tabPosition, num);
                }
            }

            return result;
        }

        internal bool IsImage()
        {
            return image != null;
        }

        public override string ToString()
        {
            return value;
        }

        internal bool IsSpecialEncoding()
        {
            if (!encoding.Equals("UNICODEBIGUNMARKED"))
            {
                return encoding.Equals("Identity-H");
            }

            return true;
        }

        internal bool IsExtSplitCharacter(int start, int current, int end, char[] cc, PdfChunk[] ck)
        {
            return splitCharacter.IsSplitCharacter(start, current, end, cc, ck);
        }

        internal string Trim(string str)
        {
            BaseFont baseFont = font.Font;
            if (baseFont.FontType == 2 && baseFont.GetUnicodeEquivalent(32) != 32)
            {
                while (str.EndsWith("\u0001"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
            }
            else
            {
                while (str.EndsWith(" ") || str.EndsWith("\t"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
            }

            return str;
        }

        internal float GetCharWidth(int c)
        {
            if (NoPrint(c))
            {
                return 0f;
            }

            if (IsAttribute("CHAR_SPACING"))
            {
                float num = (float)GetAttribute("CHAR_SPACING");
                return font.Width(c) + num * font.HorizontalScaling;
            }

            if (IsImage())
            {
                return ImageWidth;
            }

            return font.Width(c);
        }

        public static bool NoPrint(int c)
        {
            if (c < 8203 || c > 8207)
            {
                if (c >= 8234)
                {
                    return c <= 8238;
                }

                return false;
            }

            return true;
        }
    }
}
