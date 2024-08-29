using Sign.itext.error_messages;
using Sign.itext.text;
using System.Text;

namespace Sign.itext.pdf.fonts.cmaps
{
    public class CMapToUnicode : AbstractCMap
    {
        private IDictionary<int, string> singleByteMappings = new Dictionary<int, string>();

        private IDictionary<int, string> doubleByteMappings = new Dictionary<int, string>();

        private Encoding enc = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);

        public virtual bool HasOneByteMappings()
        {
            return singleByteMappings.Count != 0;
        }

        public virtual bool HasTwoByteMappings()
        {
            return doubleByteMappings.Count != 0;
        }

        public virtual string Lookup(byte[] code, int offset, int length)
        {
            string value = null;
            int num = 0;
            switch (length)
            {
                case 1:
                    num = code[offset] & 0xFF;
                    singleByteMappings.TryGetValue(num, out value);
                    break;
                case 2:
                    {
                        int num2 = code[offset] & 0xFF;
                        num2 <<= 8;
                        num2 += code[offset + 1] & 0xFF;
                        doubleByteMappings.TryGetValue(num2, out value);
                        break;
                    }
            }

            return value;
        }

        public virtual IDictionary<int, int> CreateReverseMapping()
        {
            IDictionary<int, int> dictionary = new Dictionary<int, int>();
            foreach (KeyValuePair<int, string> singleByteMapping in singleByteMappings)
            {
                dictionary[ConvertToInt(singleByteMapping.Value)] = singleByteMapping.Key;
            }

            foreach (KeyValuePair<int, string> doubleByteMapping in doubleByteMappings)
            {
                dictionary[ConvertToInt(doubleByteMapping.Value)] = doubleByteMapping.Key;
            }

            return dictionary;
        }

        public virtual IDictionary<int, int> CreateDirectMapping()
        {
            IDictionary<int, int> dictionary = new Dictionary<int, int>();
            foreach (KeyValuePair<int, string> singleByteMapping in singleByteMappings)
            {
                dictionary[singleByteMapping.Key] = ConvertToInt(singleByteMapping.Value);
            }

            foreach (KeyValuePair<int, string> doubleByteMapping in doubleByteMappings)
            {
                dictionary[doubleByteMapping.Key] = ConvertToInt(doubleByteMapping.Value);
            }

            return dictionary;
        }

        private int ConvertToInt(string s)
        {
            byte[] bytes = new UnicodeEncoding(bigEndian: true, byteOrderMark: false).GetBytes(s);
            int num = 0;
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                num += bytes[i] & 0xFF;
                num <<= 8;
            }

            return num + (bytes[^1] & 0xFF);
        }

        internal void AddChar(int cid, string uni)
        {
            doubleByteMappings[cid] = uni;
        }

        internal override void AddChar(PdfString mark, PdfObject code)
        {
            byte[] bytes = mark.GetBytes();
            string value = CreateStringFromBytes(code.GetBytes());
            if (bytes.Length == 1)
            {
                singleByteMappings[bytes[0] & 0xFF] = value;
                return;
            }

            if (bytes.Length == 2)
            {
                int num = bytes[0] & 0xFF;
                num <<= 8;
                num |= bytes[1] & 0xFF;
                doubleByteMappings[num] = value;
                return;
            }

            throw new IOException(MessageLocalization.GetComposedMessage("mapping.code.should.be.1.or.two.bytes.and.not.1", bytes.Length));
        }

        private string CreateStringFromBytes(byte[] bytes)
        {
            string text = null;
            if (bytes.Length == 1)
            {
                return new string((char)(bytes[0] & 0xFFu), 1);
            }

            return enc.GetString(bytes);
        }

        public static CMapToUnicode GetIdentity()
        {
            CMapToUnicode cMapToUnicode = new CMapToUnicode();
            for (int i = 0; i < 65537; i++)
            {
                cMapToUnicode.AddChar(i, Utilities.ConvertFromUtf32(i));
            }

            return cMapToUnicode;
        }
    }
}
