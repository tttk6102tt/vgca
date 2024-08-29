using Sign.itext.pdf;
using Sign.SystemItext.util;
using System.Text;


namespace Sign.itext.text
{
    public class Utilities
    {
        private static byte[] skipBuffer = new byte[4096];

        public static ICollection<K> GetKeySet<K, V>(Dictionary<K, V> table)
        {
            if (table != null)
            {
                return table.Keys;
            }

            return new List<K>();
        }

        public static object[][] AddToArray(object[][] original, object[] item)
        {
            if (original == null)
            {
                original = new object[1][] { item };
                return original;
            }

            object[][] array = new object[original.Length + 1][];
            Array.Copy(original, 0, array, 0, original.Length);
            array[original.Length] = item;
            return array;
        }

        public static bool CheckTrueOrFalse(Sign.SystemItext.util.Properties attributes, string key)
        {
            return Util.EqualsIgnoreCase("true", attributes[key]);
        }

        public static Uri ToURL(string filename)
        {
            try
            {
                return new Uri(filename);
            }
            catch
            {
                return new Uri(Path.GetFullPath(filename));
            }
        }

        public static string UnEscapeURL(string src)
        {
            StringBuilder stringBuilder = new StringBuilder();
            char[] array = src.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                char c = array[i];
                if (c == '%')
                {
                    if (i + 2 >= array.Length)
                    {
                        stringBuilder.Append(c);
                        continue;
                    }

                    int hex = PRTokeniser.GetHex(array[i + 1]);
                    int hex2 = PRTokeniser.GetHex(array[i + 2]);
                    if (hex < 0 || hex2 < 0)
                    {
                        stringBuilder.Append(c);
                        continue;
                    }

                    stringBuilder.Append((char)(hex * 16 + hex2));
                    i += 2;
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        public static void Skip(Stream istr, int size)
        {
            while (size > 0)
            {
                int num = istr.Read(skipBuffer, 0, Math.Min(skipBuffer.Length, size));
                if (num <= 0)
                {
                    break;
                }

                size -= num;
            }
        }

        public static float MillimetersToPoints(float value)
        {
            return InchesToPoints(MillimetersToInches(value));
        }

        public static float MillimetersToInches(float value)
        {
            return value / 25.4f;
        }

        public static float PointsToMillimeters(float value)
        {
            return InchesToMillimeters(PointsToInches(value));
        }

        public static float PointsToInches(float value)
        {
            return value / 72f;
        }

        public static float InchesToMillimeters(float value)
        {
            return value * 25.4f;
        }

        public static float InchesToPoints(float value)
        {
            return value * 72f;
        }

        public static bool IsSurrogateHigh(char c)
        {
            if (c >= '\ud800')
            {
                return c <= '\udbff';
            }

            return false;
        }

        public static bool IsSurrogateLow(char c)
        {
            if (c >= '\udc00')
            {
                return c <= '\udfff';
            }

            return false;
        }

        public static bool IsSurrogatePair(string text, int idx)
        {
            if (idx < 0 || idx > text.Length - 2)
            {
                return false;
            }

            if (IsSurrogateHigh(text[idx]))
            {
                return IsSurrogateLow(text[idx + 1]);
            }

            return false;
        }

        public static bool IsSurrogatePair(char[] text, int idx)
        {
            if (idx < 0 || idx > text.Length - 2)
            {
                return false;
            }

            if (IsSurrogateHigh(text[idx]))
            {
                return IsSurrogateLow(text[idx + 1]);
            }

            return false;
        }

        public static int ConvertToUtf32(char highSurrogate, char lowSurrogate)
        {
            return (highSurrogate - 55296) * 1024 + (lowSurrogate - 56320) + 65536;
        }

        public static int ConvertToUtf32(char[] text, int idx)
        {
            return (text[idx] - 55296) * 1024 + (text[idx + 1] - 56320) + 65536;
        }

        public static int ConvertToUtf32(string text, int idx)
        {
            return (text[idx] - 55296) * 1024 + (text[idx + 1] - 56320) + 65536;
        }

        public static string ConvertFromUtf32(int codePoint)
        {
            if (codePoint < 65536)
            {
                return char.ToString((char)codePoint);
            }

            codePoint -= 65536;
            return new string(new char[2]
            {
                (char)(codePoint / 1024 + 55296),
                (char)(codePoint % 1024 + 56320)
            });
        }

        public static string ReadFileToString(string path)
        {
            using StreamReader streamReader = new StreamReader(path, Encoding.Default);
            return streamReader.ReadToEnd();
        }

        public static string ConvertToHex(byte[] bytes)
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            foreach (byte b in bytes)
            {
                byteBuffer.AppendHex(b);
            }

            return PdfEncodings.ConvertToString(byteBuffer.ToByteArray(), null).ToUpper();
        }

        public static float ComputeTabSpace(float lx, float rx, float tab)
        {
            return ComputeTabSpace(rx - lx, tab);
        }

        public static float ComputeTabSpace(float width, float tab)
        {
            width = (float)Math.Round(width, 3);
            tab = (float)Math.Round(tab, 3);
            return tab - width % tab;
        }
    }
}
