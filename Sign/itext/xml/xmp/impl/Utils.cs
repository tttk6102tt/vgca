using System.Text;

namespace Sign.itext.xml.xmp.impl
{
    public class Utils : XmpConst
    {
        public const int UUID_SEGMENT_COUNT = 4;

        public static readonly int UUID_LENGTH;

        private static bool[] _xmlNameStartChars;

        private static bool[] _xmlNameChars;

        static Utils()
        {
            UUID_LENGTH = 36;
            InitCharTables();
        }

        private Utils()
        {
        }

        public static string NormalizeLangValue(string value)
        {
            if ("x-default".Equals(value))
            {
                return value;
            }

            int num = 1;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '-':
                    case '_':
                        stringBuilder.Append('-');
                        num++;
                        break;
                    default:
                        stringBuilder.Append((num != 2) ? char.ToLower(value[i]) : char.ToUpper(value[i]));
                        break;
                    case ' ':
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        internal static string[] SplitNameAndValue(string selector)
        {
            int num = selector.IndexOf('=');
            int num2 = 1;
            if (selector[num2] == '?')
            {
                num2++;
            }

            string text = selector.Substring(num2, num - num2);
            num2 = num + 1;
            char c = selector[num2];
            num2++;
            int num3 = selector.Length - 2;
            StringBuilder stringBuilder = new StringBuilder(num3 - num);
            while (num2 < num3)
            {
                stringBuilder.Append(selector[num2]);
                num2++;
                if (selector[num2] == c)
                {
                    num2++;
                }
            }

            return new string[2]
            {
                text,
                stringBuilder.ToString()
            };
        }

        internal static bool IsInternalProperty(string schema, string prop)
        {
            bool result = false;
            if ("http://purl.org/dc/elements/1.1/".Equals(schema))
            {
                if ("dc:format".Equals(prop) || "dc:language".Equals(prop))
                {
                    result = true;
                }
            }
            else if ("http://ns.adobe.com/xap/1.0/".Equals(schema))
            {
                if ("xmp:BaseURL".Equals(prop) || "xmp:CreatorTool".Equals(prop) || "xmp:Format".Equals(prop) || "xmp:Locale".Equals(prop) || "xmp:MetadataDate".Equals(prop) || "xmp:ModifyDate".Equals(prop))
                {
                    result = true;
                }
            }
            else if ("http://ns.adobe.com/pdf/1.3/".Equals(schema))
            {
                if ("pdf:BaseURL".Equals(prop) || "pdf:Creator".Equals(prop) || "pdf:ModDate".Equals(prop) || "pdf:PDFVersion".Equals(prop) || "pdf:Producer".Equals(prop))
                {
                    result = true;
                }
            }
            else if ("http://ns.adobe.com/tiff/1.0/".Equals(schema))
            {
                result = true;
                if ("tiff:ImageDescription".Equals(prop) || "tiff:Artist".Equals(prop) || "tiff:Copyright".Equals(prop))
                {
                    result = false;
                }
            }
            else if ("http://ns.adobe.com/exif/1.0/".Equals(schema))
            {
                result = true;
                if ("exif:UserComment".Equals(prop))
                {
                    result = false;
                }
            }
            else if ("http://ns.adobe.com/exif/1.0/aux/".Equals(schema))
            {
                result = true;
            }
            else if ("http://ns.adobe.com/photoshop/1.0/".Equals(schema))
            {
                if ("photoshop:ICCProfile".Equals(prop))
                {
                    result = true;
                }
            }
            else if ("http://ns.adobe.com/camera-raw-settings/1.0/".Equals(schema))
            {
                if ("crs:Version".Equals(prop) || "crs:RawFileName".Equals(prop) || "crs:ToneCurveName".Equals(prop))
                {
                    result = true;
                }
            }
            else if ("http://ns.adobe.com/StockPhoto/1.0/".Equals(schema))
            {
                result = true;
            }
            else if ("http://ns.adobe.com/xap/1.0/mm/".Equals(schema))
            {
                result = true;
            }
            else if ("http://ns.adobe.com/xap/1.0/t/".Equals(schema))
            {
                result = true;
            }
            else if ("http://ns.adobe.com/xap/1.0/t/pg/".Equals(schema))
            {
                result = true;
            }
            else if ("http://ns.adobe.com/xap/1.0/g/".Equals(schema))
            {
                result = true;
            }
            else if ("http://ns.adobe.com/xap/1.0/g/img/".Equals(schema))
            {
                result = true;
            }
            else if ("http://ns.adobe.com/xap/1.0/sType/Font#".Equals(schema))
            {
                result = true;
            }

            return result;
        }

        internal static bool CheckUuidFormat(string uuid)
        {
            bool flag = true;
            int num = 0;
            if (uuid == null)
            {
                return false;
            }

            int i;
            for (i = 0; i < uuid.Length; i++)
            {
                if (uuid[i] == '-')
                {
                    num++;
                    flag = flag && (i == 8 || i == 13 || i == 18 || i == 23);
                }
            }

            if (flag && 4 == num)
            {
                return UUID_LENGTH == i;
            }

            return false;
        }

        public static bool IsXmlName(string name)
        {
            if (name.Length > 0 && !IsNameStartChar(name[0]))
            {
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                if (!IsNameChar(name[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsXmlNameNs(string name)
        {
            if (name.Length > 0 && (!IsNameStartChar(name[0]) || name[0] == ':'))
            {
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                if (!IsNameChar(name[i]) || name[i] == ':')
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsControlChar(char c)
        {
            if ((c <= '\u001f' || c == '\u007f') && c != '\t' && c != '\n')
            {
                return c != '\r';
            }

            return false;
        }

        public static string EscapeXml(string value, bool forAttribute, bool escapeWhitespaces)
        {
            bool flag = false;
            foreach (char c in value)
            {
                if (c == '<' || c == '>' || c == '&' || (escapeWhitespaces && (c == '\t' || c == '\n' || c == '\r')) || (forAttribute && c == '"'))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                return value;
            }

            StringBuilder stringBuilder = new StringBuilder(value.Length * 4 / 3);
            foreach (char c2 in value)
            {
                if (!escapeWhitespaces || (c2 != '\t' && c2 != '\n' && c2 != '\r'))
                {
                    switch (c2)
                    {
                        case '<':
                            stringBuilder.Append("&lt;");
                            break;
                        case '>':
                            stringBuilder.Append("&gt;");
                            break;
                        case '&':
                            stringBuilder.Append("&amp;");
                            break;
                        case '"':
                            stringBuilder.Append(forAttribute ? "&quot;" : "\"");
                            break;
                        default:
                            stringBuilder.Append(c2);
                            break;
                    }
                }
                else
                {
                    stringBuilder.Append("&#x");
                    int num = c2;
                    stringBuilder.Append(num.ToString("X").ToUpper());
                    stringBuilder.Append(';');
                }
            }

            return stringBuilder.ToString();
        }

        internal static string RemoveControlChars(string value)
        {
            StringBuilder stringBuilder = new StringBuilder(value);
            for (int i = 0; i < stringBuilder.Length; i++)
            {
                if (IsControlChar(stringBuilder[i]))
                {
                    stringBuilder[i] = ' ';
                }
            }

            return stringBuilder.ToString();
        }

        private static bool IsNameStartChar(char ch)
        {
            if ((ch > 'ÿ' || !_xmlNameStartChars[(uint)ch]) && (ch < 'Ā' || ch > '\u02ff') && (ch < 'Ͱ' || ch > 'ͽ') && (ch < 'Ϳ' || ch > '\u1fff') && (ch < '\u200c' || ch > '\u200d') && (ch < '⁰' || ch > '\u218f') && (ch < 'Ⰰ' || ch > '\u2fef') && (ch < '、' || ch > '\ud7ff') && (ch < '豈' || ch > '\ufdcf') && (ch < 'ﷰ' || ch > '\ufffd'))
            {
                if (ch >= 65536)
                {
                    return ch <= 983039;
                }

                return false;
            }

            return true;
        }

        private static bool IsNameChar(char ch)
        {
            if ((ch > 'ÿ' || !_xmlNameChars[(uint)ch]) && !IsNameStartChar(ch) && (ch < '\u0300' || ch > '\u036f'))
            {
                if (ch >= '\u203f')
                {
                    return ch <= '\u2040';
                }

                return false;
            }

            return true;
        }

        private static void InitCharTables()
        {
            _xmlNameChars = new bool[256];
            _xmlNameStartChars = new bool[256];
            for (char c = '\0'; c < _xmlNameChars.Length; c = (char)(c + 1))
            {
                _xmlNameStartChars[(uint)c] = c == ':' || ('A' <= c && c <= 'Z') || c == '_' || ('a' <= c && c <= 'z') || ('À' <= c && c <= 'Ö') || ('Ø' <= c && c <= 'ö') || ('ø' <= c && c <= 'ÿ');
                _xmlNameChars[(uint)c] = _xmlNameStartChars[(uint)c] || c == '-' || c == '.' || ('0' <= c && c <= '9') || c == '·';
            }
        }
    }
}
