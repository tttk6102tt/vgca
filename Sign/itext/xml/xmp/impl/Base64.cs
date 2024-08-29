namespace Sign.itext.xml.xmp.impl
{
    public class Base64
    {
        private const byte INVALID = byte.MaxValue;

        private const byte WHITESPACE = 254;

        private const byte EQUAL = 253;

        private static readonly byte[] base64;

        private static readonly byte[] Ascii;

        static Base64()
        {
            base64 = new byte[64]
            {
                65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
                75, 76, 77, 78, 79, 80, 81, 82, 83, 84,
                85, 86, 87, 88, 89, 90, 97, 98, 99, 100,
                101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
                111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
                121, 122, 48, 49, 50, 51, 52, 53, 54, 55,
                56, 57, 43, 47
            };
            Ascii = new byte[255];
            for (int i = 0; i < 255; i++)
            {
                Ascii[i] = byte.MaxValue;
            }

            for (int j = 0; j < base64.Length; j++)
            {
                Ascii[base64[j]] = (byte)j;
            }

            Ascii[9] = 254;
            Ascii[10] = 254;
            Ascii[13] = 254;
            Ascii[32] = 254;
            Ascii[61] = 253;
        }

        public static byte[] Encode(byte[] src)
        {
            return Encode(src, 0);
        }

        public static byte[] Encode(byte[] src, int lineFeed)
        {
            lineFeed = lineFeed / 4 * 4;
            if (lineFeed < 0)
            {
                lineFeed = 0;
            }

            int num = (src.Length + 2) / 3 * 4;
            if (lineFeed > 0)
            {
                num += (num - 1) / lineFeed;
            }

            byte[] array = new byte[num];
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            while (num3 + 3 <= src.Length)
            {
                int num5 = ((src[num3++] & 0xFF) << 16) | ((src[num3++] & 0xFF) << 8) | (src[num3++] & 0xFF);
                int num6 = (num5 & 0xFC0000) >> 18;
                array[num2++] = base64[num6];
                num6 = (num5 & 0x3F000) >> 12;
                array[num2++] = base64[num6];
                num6 = (num5 & 0xFC0) >> 6;
                array[num2++] = base64[num6];
                num6 = num5 & 0x3F;
                array[num2++] = base64[num6];
                num4 += 4;
                if (num2 < num && lineFeed > 0 && num4 % lineFeed == 0)
                {
                    array[num2++] = 10;
                }
            }

            if (src.Length - num3 == 2)
            {
                int num7 = ((src[num3] & 0xFF) << 16) | ((src[num3 + 1] & 0xFF) << 8);
                int num6 = (num7 & 0xFC0000) >> 18;
                array[num2++] = base64[num6];
                num6 = (num7 & 0x3F000) >> 12;
                array[num2++] = base64[num6];
                num6 = (num7 & 0xFC0) >> 6;
                array[num2++] = base64[num6];
                array[num2] = 61;
            }
            else if (src.Length - num3 == 1)
            {
                int num8 = (src[num3] & 0xFF) << 16;
                int num6 = (num8 & 0xFC0000) >> 18;
                array[num2++] = base64[num6];
                num6 = (num8 & 0x3F000) >> 12;
                array[num2++] = base64[num6];
                array[num2++] = 61;
                array[num2] = 61;
            }

            return array;
        }

        public static string Encode(string src)
        {
            return GetString(Encode(GetBytes(src)));
        }

        public static byte[] Decode(byte[] src)
        {
            int num = 0;
            int i;
            for (i = 0; i < src.Length; i++)
            {
                byte b = Ascii[src[i]];
                if (b >= 0)
                {
                    src[num++] = b;
                }
                else if (b == byte.MaxValue)
                {
                    throw new ArgumentException("Invalid base 64 string");
                }
            }

            while (num > 0 && src[num - 1] == 253)
            {
                num--;
            }

            byte[] array = new byte[num * 3 / 4];
            i = 0;
            int j;
            for (j = 0; j < array.Length - 2; j += 3)
            {
                array[j] = (byte)(((uint)(src[i] << 2) & 0xFFu) | (((uint)src[i + 1] >> 4) & 3u));
                array[j + 1] = (byte)(((uint)(src[i + 1] << 4) & 0xFFu) | (((uint)src[i + 2] >> 2) & 0xFu));
                array[j + 2] = (byte)(((uint)(src[i + 2] << 6) & 0xFFu) | (src[i + 3] & 0x3Fu));
                i += 4;
            }

            if (j < array.Length)
            {
                array[j] = (byte)(((uint)(src[i] << 2) & 0xFFu) | (((uint)src[i + 1] >> 4) & 3u));
            }

            if (++j < array.Length)
            {
                array[j] = (byte)(((uint)(src[i + 1] << 4) & 0xFFu) | (((uint)src[i + 2] >> 2) & 0xFu));
            }

            return array;
        }

        public static string Decode(string src)
        {
            return GetString(Decode(GetBytes(src)));
        }

        private static byte[] GetBytes(string str)
        {
            byte[] array = new byte[str.Length * 2];
            Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
            return array;
        }

        private static string GetString(byte[] bytes)
        {
            char[] array = new char[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
            return new string(array);
        }
    }
}
