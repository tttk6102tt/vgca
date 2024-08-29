using System.Text;

namespace Sign.itext.xml
{
    public static class XMLUtil
    {
        public static string EscapeXML(string s, bool onlyASCII)
        {
            char[] array = s.ToCharArray();
            int num = array.Length;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < num; i++)
            {
                int num2 = array[i];
                switch (num2)
                {
                    case 60:
                        stringBuilder.Append("&lt;");
                        continue;
                    case 62:
                        stringBuilder.Append("&gt;");
                        continue;
                    case 38:
                        stringBuilder.Append("&amp;");
                        continue;
                    case 34:
                        stringBuilder.Append("&quot;");
                        continue;
                    case 39:
                        stringBuilder.Append("&apos;");
                        continue;
                }

                if (IsValidCharacterValue(num2))
                {
                    if (onlyASCII && num2 > 127)
                    {
                        stringBuilder.Append("&#").Append(num2).Append(';');
                    }
                    else
                    {
                        stringBuilder.Append((char)num2);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public static string UnescapeXML(string s)
        {
            char[] array = s.ToCharArray();
            int num = array.Length;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < num; i++)
            {
                int num2 = array[i];
                if (num2 == 38)
                {
                    int num3 = FindInArray(';', array, i + 3);
                    if (num3 > -1)
                    {
                        string text = new string(array, i + 1, num3 - i - 1);
                        if (text.StartsWith("#"))
                        {
                            text = text.Substring(1);
                            if (!IsValidCharacterValue(text))
                            {
                                i = num3;
                                continue;
                            }

                            num2 = (ushort)int.Parse(text);
                            i = num3;
                        }
                        else
                        {
                            int num4 = Unescape(text);
                            if (num4 > 0)
                            {
                                num2 = num4;
                                i = num3;
                            }
                        }
                    }
                }

                stringBuilder.Append((char)num2);
            }

            return stringBuilder.ToString();
        }

        public static int Unescape(string s)
        {
            if ("apos".Equals(s))
            {
                return 39;
            }

            if ("quot".Equals(s))
            {
                return 34;
            }

            if ("lt".Equals(s))
            {
                return 60;
            }

            if ("gt".Equals(s))
            {
                return 62;
            }

            if ("amp".Equals(s))
            {
                return 38;
            }

            return -1;
        }

        public static bool IsValidCharacterValue(string s)
        {
            try
            {
                return IsValidCharacterValue(int.Parse(s));
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidCharacterValue(int c)
        {
            if (c != 9 && c != 10 && c != 13 && (c < 32 || c > 55295) && (c < 57344 || c > 65533))
            {
                if (c >= 65536)
                {
                    return c <= 1114111;
                }

                return false;
            }

            return true;
        }

        public static int FindInArray(char needle, char[] haystack, int start)
        {
            for (int i = start; i < haystack.Length; i++)
            {
                if (haystack[i] == ';')
                {
                    return i;
                }
            }

            return -1;
        }

        public static string GetEncodingName(byte[] b4)
        {
            int num = b4[0] & 0xFF;
            int num2 = b4[1] & 0xFF;
            if (num == 254 && num2 == 255)
            {
                return "UTF-16BE";
            }

            if (num == 255 && num2 == 254)
            {
                return "UTF-16LE";
            }

            int num3 = b4[2] & 0xFF;
            if (num == 239 && num2 == 187 && num3 == 191)
            {
                return "UTF-8";
            }

            int num4 = b4[3] & 0xFF;
            if (num == 0 && num2 == 0 && num3 == 0 && num4 == 60)
            {
                return "ISO-10646-UCS-4";
            }

            if (num == 60 && num2 == 0 && num3 == 0 && num4 == 0)
            {
                return "ISO-10646-UCS-4";
            }

            if (num == 0 && num2 == 0 && num3 == 60 && num4 == 0)
            {
                return "ISO-10646-UCS-4";
            }

            if (num == 0 && num2 == 60 && num3 == 0 && num4 == 0)
            {
                return "ISO-10646-UCS-4";
            }

            if (num == 0 && num2 == 60 && num3 == 0 && num4 == 63)
            {
                return "UTF-16BE";
            }

            if (num == 60 && num2 == 0 && num3 == 63 && num4 == 0)
            {
                return "UTF-16LE";
            }

            if (num == 76 && num2 == 111 && num3 == 167 && num4 == 148)
            {
                return "CP037";
            }

            return "UTF-8";
        }
    }
}
