using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class StringUtils
    {
        private static readonly byte[] r = DocWriter.GetISOBytes("\\r");

        private static readonly byte[] n = DocWriter.GetISOBytes("\\n");

        private static readonly byte[] t = DocWriter.GetISOBytes("\\t");

        private static readonly byte[] b = DocWriter.GetISOBytes("\\b");

        private static readonly byte[] f = DocWriter.GetISOBytes("\\f");

        private StringUtils()
        {
        }

        public static byte[] EscapeString(byte[] b)
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            EscapeString(b, byteBuffer);
            return byteBuffer.ToByteArray();
        }

        public static void EscapeString(byte[] bytes, ByteBuffer content)
        {
            content.Append_i(40);
            foreach (byte b in bytes)
            {
                switch (b)
                {
                    case 13:
                        content.Append(r);
                        break;
                    case 10:
                        content.Append(n);
                        break;
                    case 9:
                        content.Append(t);
                        break;
                    case 8:
                        content.Append(StringUtils.b);
                        break;
                    case 12:
                        content.Append(f);
                        break;
                    case 40:
                    case 41:
                    case 92:
                        content.Append_i(92).Append_i(b);
                        break;
                    default:
                        content.Append_i(b);
                        break;
                }
            }

            content.Append(')');
        }
    }
}
