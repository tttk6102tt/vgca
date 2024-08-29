using Sign.itext.io;
using Sign.SystemItext.util;
using System.Globalization;

namespace Sign.itext.pdf
{
    public class GlyphList
    {
        private static Dictionary<int, string> unicode2names;

        private static Dictionary<string, int[]> names2unicode;

        static GlyphList()
        {
            unicode2names = new Dictionary<int, string>();
            names2unicode = new Dictionary<string, int[]>();
            Stream stream = null;
            try
            {
                stream = StreamUtil.GetResourceStream("text.pdf.fonts.glyphlist.txt");
                if (stream == null)
                {
                    throw new Exception("glyphlist.txt not found as resource.");
                }

                byte[] array = new byte[1024];
                MemoryStream memoryStream = new MemoryStream();
                while (true)
                {
                    int num = stream.Read(array, 0, array.Length);
                    if (num == 0)
                    {
                        break;
                    }

                    memoryStream.Write(array, 0, num);
                }

                stream.Close();
                stream = null;
                StringTokenizer stringTokenizer = new StringTokenizer(PdfEncodings.ConvertToString(memoryStream.ToArray(), null), "\r\n");
                while (stringTokenizer.HasMoreTokens())
                {
                    string text = stringTokenizer.NextToken();
                    if (text.StartsWith("#"))
                    {
                        continue;
                    }

                    StringTokenizer stringTokenizer2 = new StringTokenizer(text, " ;\r\n\t\f");
                    string text2 = null;
                    if (stringTokenizer2.HasMoreTokens())
                    {
                        text2 = stringTokenizer2.NextToken();
                        if (stringTokenizer2.HasMoreTokens())
                        {
                            int num2 = int.Parse(stringTokenizer2.NextToken(), NumberStyles.HexNumber);
                            unicode2names[num2] = text2;
                            names2unicode[text2] = new int[1] { num2 };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("glyphlist.txt loading error: " + ex.Message);
            }
            finally
            {
                if (stream != null)
                {
                    try
                    {
                        stream.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static int[] NameToUnicode(string name)
        {
            names2unicode.TryGetValue(name, out var value);
            if (value == null && name.Length == 7 && name.ToLowerInvariant().StartsWith("uni"))
            {
                try
                {
                    return new int[1] { int.Parse(name.Substring(3), NumberStyles.HexNumber) };
                }
                catch
                {
                    return value;
                }
            }

            return value;
        }

        public static string UnicodeToName(int num)
        {
            unicode2names.TryGetValue(num, out var value);
            return value;
        }
    }
}
