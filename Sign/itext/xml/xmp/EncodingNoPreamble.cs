using System.Text;

namespace Sign.itext.xml.xmp
{
    public class EncodingNoPreamble : Encoding
    {
        private Encoding encoding;

        private static byte[] emptyPreamble = new byte[0];

        public override string BodyName => encoding.BodyName;

        public override int CodePage => encoding.CodePage;

        public override string EncodingName => encoding.EncodingName;

        public override string HeaderName => encoding.HeaderName;

        public override bool IsBrowserDisplay => encoding.IsBrowserDisplay;

        public override bool IsBrowserSave => encoding.IsBrowserSave;

        public override bool IsMailNewsDisplay => encoding.IsMailNewsDisplay;

        public override bool IsMailNewsSave => encoding.IsMailNewsSave;

        public override string WebName => encoding.WebName;

        public override int WindowsCodePage => encoding.WindowsCodePage;

        public EncodingNoPreamble(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return encoding.GetByteCount(chars, index, count);
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return encoding.GetCharCount(bytes, index, count);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override int GetMaxByteCount(int charCount)
        {
            return encoding.GetMaxByteCount(charCount);
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return encoding.GetMaxCharCount(byteCount);
        }

        public override Decoder GetDecoder()
        {
            return encoding.GetDecoder();
        }

        public override Encoder GetEncoder()
        {
            return encoding.GetEncoder();
        }

        public override byte[] GetPreamble()
        {
            return emptyPreamble;
        }
    }
}
