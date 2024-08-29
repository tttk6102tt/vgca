using Sign.itext.pdf;
using Sign.itext.text;
using Sign.SystemItext.util;

namespace Sign.itext
{
    public class Font : IComparable<Font>
    {
        public enum FontFamily
        {
            COURIER = 0,
            HELVETICA = 1,
            TIMES_ROMAN = 2,
            SYMBOL = 3,
            ZAPFDINGBATS = 4,
            UNDEFINED = -1
        }

        public const int NORMAL = 0;

        public const int BOLD = 1;

        public const int ITALIC = 2;

        public const int UNDERLINE = 4;

        public const int STRIKETHRU = 8;

        public const int BOLDITALIC = 3;

        public const int UNDEFINED = -1;

        public const int DEFAULTSIZE = 12;

        private FontFamily family = FontFamily.UNDEFINED;

        private float size = -1f;

        private int style = -1;

        private BaseColor color;

        private BaseFont baseFont;

        public virtual FontFamily Family => family;

        public virtual string Familyname
        {
            get
            {
                string result = "unknown";
                switch (Family)
                {
                    case FontFamily.COURIER:
                        return "Courier";
                    case FontFamily.HELVETICA:
                        return "Helvetica";
                    case FontFamily.TIMES_ROMAN:
                        return "Times-Roman";
                    case FontFamily.SYMBOL:
                        return "Symbol";
                    case FontFamily.ZAPFDINGBATS:
                        return "ZapfDingbats";
                    default:
                        if (baseFont != null)
                        {
                            string[][] familyFontName = baseFont.FamilyFontName;
                            foreach (string[] array in familyFontName)
                            {
                                if ("0".Equals(array[2]))
                                {
                                    return array[3];
                                }

                                if ("1033".Equals(array[2]))
                                {
                                    result = array[3];
                                }

                                if ("".Equals(array[2]))
                                {
                                    result = array[3];
                                }
                            }
                        }

                        return result;
                }
            }
        }

        public virtual float Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        public virtual float CalculatedSize
        {
            get
            {
                float num = size;
                if (num == -1f)
                {
                    num = 12f;
                }

                return num;
            }
        }

        public virtual int Style => style;

        public virtual int CalculatedStyle
        {
            get
            {
                int num = style;
                if (num == -1)
                {
                    num = 0;
                }

                if (baseFont != null)
                {
                    return num;
                }

                if (family == FontFamily.SYMBOL || family == FontFamily.ZAPFDINGBATS)
                {
                    return num;
                }

                return num & -4;
            }
        }

        public virtual BaseColor Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public virtual BaseFont BaseFont => baseFont;

        public Font(Font other)
        {
            color = other.color;
            family = other.family;
            size = other.size;
            style = other.style;
            baseFont = other.baseFont;
        }

        public Font(FontFamily family, float size, int style, BaseColor color)
        {
            this.family = family;
            this.size = size;
            this.style = style;
            this.color = color;
        }

        public Font(BaseFont bf, float size, int style, BaseColor color)
        {
            baseFont = bf;
            this.size = size;
            this.style = style;
            this.color = color;
        }

        public Font(BaseFont bf, float size, int style)
            : this(bf, size, style, null)
        {
        }

        public Font(BaseFont bf, float size)
            : this(bf, size, -1, null)
        {
        }

        public Font(BaseFont bf)
            : this(bf, -1f, -1, null)
        {
        }

        public Font(FontFamily family, float size, int style)
            : this(family, size, style, null)
        {
        }

        public Font(FontFamily family, float size)
            : this(family, size, -1, null)
        {
        }

        public Font(FontFamily family)
            : this(family, -1f, -1, null)
        {
        }

        public Font()
            : this(FontFamily.UNDEFINED, -1f, -1, null)
        {
        }

        public virtual int CompareTo(Font font)
        {
            if (font == null)
            {
                return -1;
            }

            try
            {
                if (baseFont != null && !baseFont.Equals(font.BaseFont))
                {
                    return -2;
                }

                if (family != font.Family)
                {
                    return 1;
                }

                if (size != font.Size)
                {
                    return 2;
                }

                if (style != font.Style)
                {
                    return 3;
                }

                if (color == null)
                {
                    if (font.Color == null)
                    {
                        return 0;
                    }

                    return 4;
                }

                if (font.Color == null)
                {
                    return 4;
                }

                if (color.Equals(font.Color))
                {
                    return 0;
                }

                return 4;
            }
            catch
            {
                return -3;
            }
        }

        public virtual void SetFamily(string family)
        {
            this.family = GetFamilyIndex(family);
        }

        public static FontFamily GetFamilyIndex(string family)
        {
            if (Util.EqualsIgnoreCase(family, "Courier"))
            {
                return FontFamily.COURIER;
            }

            if (Util.EqualsIgnoreCase(family, "Helvetica"))
            {
                return FontFamily.HELVETICA;
            }

            if (Util.EqualsIgnoreCase(family, "Times-Roman"))
            {
                return FontFamily.TIMES_ROMAN;
            }

            if (Util.EqualsIgnoreCase(family, "Symbol"))
            {
                return FontFamily.SYMBOL;
            }

            if (Util.EqualsIgnoreCase(family, "ZapfDingbats"))
            {
                return FontFamily.ZAPFDINGBATS;
            }

            return FontFamily.UNDEFINED;
        }

        public virtual float GetCalculatedLeading(float linespacing)
        {
            return linespacing * CalculatedSize;
        }

        public virtual bool IsBold()
        {
            if (style == -1)
            {
                return false;
            }

            return (style & 1) == 1;
        }

        public virtual bool IsItalic()
        {
            if (style == -1)
            {
                return false;
            }

            return (style & 2) == 2;
        }

        public virtual bool IsUnderlined()
        {
            if (style == -1)
            {
                return false;
            }

            return (style & 4) == 4;
        }

        public virtual bool IsStrikethru()
        {
            if (style == -1)
            {
                return false;
            }

            return (style & 8) == 8;
        }

        public virtual void SetStyle(string style)
        {
            if (this.style == -1)
            {
                this.style = 0;
            }

            this.style |= GetStyleValue(style);
        }

        public virtual void SetStyle(int style)
        {
            this.style = style;
        }

        public static int GetStyleValue(string style)
        {
            int num = 0;
            if (style.IndexOf("normal") != -1)
            {
                num |= 0;
            }

            if (style.IndexOf("bold") != -1)
            {
                num |= 1;
            }

            if (style.IndexOf("italic") != -1)
            {
                num |= 2;
            }

            if (style.IndexOf("oblique") != -1)
            {
                num |= 2;
            }

            if (style.IndexOf("underline") != -1)
            {
                num |= 4;
            }

            if (style.IndexOf("line-through") != -1)
            {
                num |= 8;
            }

            return num;
        }

        public virtual void SetColor(int red, int green, int blue)
        {
            color = new BaseColor(red, green, blue);
        }

        public virtual BaseFont GetCalculatedBaseFont(bool specialEncoding)
        {
            if (baseFont != null)
            {
                return baseFont;
            }

            int num = style;
            if (num == -1)
            {
                num = 0;
            }

            string text = "Helvetica";
            string encoding = "Cp1252";
            switch (family)
            {
                case FontFamily.COURIER:
                    text = (num & 3) switch
                    {
                        1 => "Courier-Bold",
                        2 => "Courier-Oblique",
                        3 => "Courier-BoldOblique",
                        _ => "Courier",
                    };
                    break;
                case FontFamily.TIMES_ROMAN:
                    text = (num & 3) switch
                    {
                        1 => "Times-Bold",
                        2 => "Times-Italic",
                        3 => "Times-BoldItalic",
                        _ => "Times-Roman",
                    };
                    break;
                case FontFamily.SYMBOL:
                    text = "Symbol";
                    if (specialEncoding)
                    {
                        encoding = "Symbol";
                    }

                    break;
                case FontFamily.ZAPFDINGBATS:
                    text = "ZapfDingbats";
                    if (specialEncoding)
                    {
                        encoding = "ZapfDingbats";
                    }

                    break;
                default:
                    text = (num & 3) switch
                    {
                        1 => "Helvetica-Bold",
                        2 => "Helvetica-Oblique",
                        3 => "Helvetica-BoldOblique",
                        _ => "Helvetica",
                    };
                    break;
            }

            return BaseFont.CreateFont(text, encoding, embedded: false);
        }

        public virtual bool IsStandardFont()
        {
            if (family == FontFamily.UNDEFINED && size == -1f && style == -1 && color == null)
            {
                return baseFont == null;
            }

            return false;
        }

        public virtual Font Difference(Font font)
        {
            if (font == null)
            {
                return this;
            }

            float num = font.size;
            if (num == -1f)
            {
                num = size;
            }

            int num2 = -1;
            int num3 = Style;
            int num4 = font.Style;
            if (num3 != -1 || num4 != -1)
            {
                if (num3 == -1)
                {
                    num3 = 0;
                }

                if (num4 == -1)
                {
                    num4 = 0;
                }

                num2 = num3 | num4;
            }

            BaseColor baseColor = font.Color;
            if (baseColor == null)
            {
                baseColor = Color;
            }

            if (font.baseFont != null)
            {
                return new Font(font.BaseFont, num, num2, baseColor);
            }

            if (font.Family != FontFamily.UNDEFINED)
            {
                return new Font(font.Family, num, num2, baseColor);
            }

            if (baseFont != null)
            {
                if (num2 == num3)
                {
                    return new Font(BaseFont, num, num2, baseColor);
                }

                return FontFactory.GetFont(Familyname, num, num2, baseColor);
            }

            return new Font(Family, num, num2, baseColor);
        }
    }
}
