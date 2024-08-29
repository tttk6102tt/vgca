using Sign.itext.error_messages;
using System.Globalization;
using System.Text;

namespace Sign.itext.pdf
{
    public class ByteBuffer : Stream
    {
        protected int count;

        protected byte[] buf;

        private static int byteCacheSize = 0;

        private static byte[][] byteCache = new byte[byteCacheSize][];

        public const byte ZERO = 48;

        private static char[] chars = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private static byte[] bytes = new byte[16]
        {
            48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
            97, 98, 99, 100, 101, 102
        };

        public static bool HIGH_PRECISION = false;

        public virtual int Size
        {
            get
            {
                return count;
            }
            set
            {
                if (value > count || value < 0)
                {
                    throw new ArgumentOutOfRangeException(MessageLocalization.GetComposedMessage("the.new.size.must.be.positive.and.lt.eq.of.the.current.size"));
                }

                count = value;
            }
        }

        public virtual byte[] Buffer => buf;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => count;

        public override long Position
        {
            get
            {
                return count;
            }
            set
            {
            }
        }

        public ByteBuffer()
            : this(128)
        {
        }

        public ByteBuffer(int size)
        {
            if (size < 1)
            {
                size = 128;
            }

            buf = new byte[size];
        }

        public static void SetCacheSize(int size)
        {
            if (size > 3276700)
            {
                size = 3276700;
            }

            if (size > byteCacheSize)
            {
                byte[][] destinationArray = new byte[size][];
                Array.Copy(byteCache, 0, destinationArray, 0, byteCacheSize);
                byteCache = destinationArray;
                byteCacheSize = size;
            }
        }

        public static void FillCache(int decimals)
        {
            int num = 1;
            switch (decimals)
            {
                case 0:
                    num = 100;
                    break;
                case 1:
                    num = 10;
                    break;
            }

            for (int i = 1; i < byteCacheSize; i += num)
            {
                if (byteCache[i] == null)
                {
                    byteCache[i] = ConvertToBytes(i);
                }
            }
        }

        private static byte[] ConvertToBytes(int i)
        {
            int num = (int)Math.Floor(Math.Log(i) / Math.Log(10.0));
            if (i % 100 != 0)
            {
                num += 2;
            }

            if (i % 10 != 0)
            {
                num++;
            }

            if (i < 100)
            {
                num++;
                if (i < 10)
                {
                    num++;
                }
            }

            num--;
            byte[] array = new byte[num];
            num--;
            if (i < 100)
            {
                array[0] = 48;
            }

            if (i % 10 != 0)
            {
                array[num--] = bytes[i % 10];
            }

            if (i % 100 != 0)
            {
                array[num--] = bytes[i / 10 % 10];
                array[num--] = 46;
            }

            num = (int)Math.Floor(Math.Log(i) / Math.Log(10.0)) - 1;
            for (int j = 0; j < num; j++)
            {
                array[j] = bytes[i / (int)Math.Pow(10.0, num - j + 1) % 10];
            }

            return array;
        }

        public virtual ByteBuffer Append_i(int b)
        {
            int num = count + 1;
            if (num > buf.Length)
            {
                byte[] destinationArray = new byte[Math.Max(buf.Length << 1, num)];
                Array.Copy(buf, 0, destinationArray, 0, count);
                buf = destinationArray;
            }

            buf[count] = (byte)b;
            count = num;
            return this;
        }

        public virtual ByteBuffer Append(byte[] b, int off, int len)
        {
            if (off < 0 || off > b.Length || len < 0 || off + len > b.Length || off + len < 0 || len == 0)
            {
                return this;
            }

            int num = count + len;
            if (num > buf.Length)
            {
                byte[] destinationArray = new byte[Math.Max(buf.Length << 1, num)];
                Array.Copy(buf, 0, destinationArray, 0, count);
                buf = destinationArray;
            }

            Array.Copy(b, off, buf, count, len);
            count = num;
            return this;
        }

        public virtual ByteBuffer Append(byte[] b)
        {
            return Append(b, 0, b.Length);
        }

        public virtual ByteBuffer Append(string str)
        {
            if (str != null)
            {
                return Append(DocWriter.GetISOBytes(str));
            }

            return this;
        }

        public virtual ByteBuffer Append(char c)
        {
            return Append_i(c);
        }

        public virtual ByteBuffer Append(ByteBuffer buf)
        {
            return Append(buf.buf, 0, buf.count);
        }

        public virtual ByteBuffer Append(int i)
        {
            return Append((double)i);
        }

        public virtual ByteBuffer Append(long i)
        {
            return Append(i.ToString(CultureInfo.InvariantCulture));
        }

        public virtual ByteBuffer Append(byte b)
        {
            return Append_i(b);
        }

        public virtual ByteBuffer AppendHex(byte b)
        {
            Append(bytes[b >> 4 & 0xF]);
            return Append(bytes[b & 0xF]);
        }

        public virtual ByteBuffer Append(float i)
        {
            return Append((double)i);
        }

        public virtual ByteBuffer Append(double d)
        {
            Append(FormatDouble(d, this));
            return this;
        }

        public static string FormatDouble(double d)
        {
            return FormatDouble(d, null);
        }

        public static string FormatDouble(double d, ByteBuffer buf)
        {
            if (HIGH_PRECISION)
            {
                string text = d.ToString("0.######", CultureInfo.InvariantCulture);
                if (buf == null)
                {
                    return text;
                }

                buf.Append(text);
                return null;
            }

            bool flag = false;
            if (Math.Abs(d) < 1.5E-05)
            {
                if (buf != null)
                {
                    buf.Append((byte)48);
                    return null;
                }

                return "0";
            }

            if (d < 0.0)
            {
                flag = true;
                d = 0.0 - d;
            }

            if (d < 1.0)
            {
                d += 5E-06;
                if (d >= 1.0)
                {
                    if (flag)
                    {
                        if (buf != null)
                        {
                            buf.Append((byte)45);
                            buf.Append((byte)49);
                            return null;
                        }

                        return "-1";
                    }

                    if (buf != null)
                    {
                        buf.Append((byte)49);
                        return null;
                    }

                    return "1";
                }

                if (buf != null)
                {
                    int num = (int)(d * 100000.0);
                    if (flag)
                    {
                        buf.Append((byte)45);
                    }

                    buf.Append((byte)48);
                    buf.Append((byte)46);
                    buf.Append((byte)(num / 10000 + 48));
                    if (num % 10000 != 0)
                    {
                        buf.Append((byte)(num / 1000 % 10 + 48));
                        if (num % 1000 != 0)
                        {
                            buf.Append((byte)(num / 100 % 10 + 48));
                            if (num % 100 != 0)
                            {
                                buf.Append((byte)(num / 10 % 10 + 48));
                                if (num % 10 != 0)
                                {
                                    buf.Append((byte)(num % 10 + 48));
                                }
                            }
                        }
                    }

                    return null;
                }

                int num2 = 100000;
                int num3 = (int)(d * num2);
                StringBuilder stringBuilder = new StringBuilder();
                if (flag)
                {
                    stringBuilder.Append('-');
                }

                stringBuilder.Append("0.");
                while (num3 < num2 / 10)
                {
                    stringBuilder.Append('0');
                    num2 /= 10;
                }

                stringBuilder.Append(num3);
                int num4 = stringBuilder.Length - 1;
                while (stringBuilder[num4] == '0')
                {
                    num4--;
                }

                stringBuilder.Length = num4 + 1;
                return stringBuilder.ToString();
            }

            if (d <= 32767.0)
            {
                d += 0.005;
                int num5 = (int)(d * 100.0);
                if (num5 < byteCacheSize && byteCache[num5] != null)
                {
                    if (buf != null)
                    {
                        if (flag)
                        {
                            buf.Append((byte)45);
                        }

                        buf.Append(byteCache[num5]);
                        return null;
                    }

                    string text2 = PdfEncodings.ConvertToString(byteCache[num5], null);
                    if (flag)
                    {
                        text2 = "-" + text2;
                    }

                    return text2;
                }

                if (buf != null)
                {
                    if (num5 < byteCacheSize)
                    {
                        int num6 = 0;
                        if (num5 >= 1000000)
                        {
                            num6 += 5;
                        }
                        else if (num5 >= 100000)
                        {
                            num6 += 4;
                        }
                        else if (num5 >= 10000)
                        {
                            num6 += 3;
                        }
                        else if (num5 >= 1000)
                        {
                            num6 += 2;
                        }
                        else if (num5 >= 100)
                        {
                            num6++;
                        }

                        if (num5 % 100 != 0)
                        {
                            num6 += 2;
                        }

                        if (num5 % 10 != 0)
                        {
                            num6++;
                        }

                        byte[] array = new byte[num6];
                        int num7 = 0;
                        if (num5 >= 1000000)
                        {
                            array[num7++] = bytes[num5 / 1000000];
                        }

                        if (num5 >= 100000)
                        {
                            array[num7++] = bytes[num5 / 100000 % 10];
                        }

                        if (num5 >= 10000)
                        {
                            array[num7++] = bytes[num5 / 10000 % 10];
                        }

                        if (num5 >= 1000)
                        {
                            array[num7++] = bytes[num5 / 1000 % 10];
                        }

                        if (num5 >= 100)
                        {
                            array[num7++] = bytes[num5 / 100 % 10];
                        }

                        if (num5 % 100 != 0)
                        {
                            array[num7++] = 46;
                            array[num7++] = bytes[num5 / 10 % 10];
                            if (num5 % 10 != 0)
                            {
                                array[num7++] = bytes[num5 % 10];
                            }
                        }

                        byteCache[num5] = array;
                    }

                    if (flag)
                    {
                        buf.Append((byte)45);
                    }

                    if (num5 >= 1000000)
                    {
                        buf.Append(bytes[num5 / 1000000]);
                    }

                    if (num5 >= 100000)
                    {
                        buf.Append(bytes[num5 / 100000 % 10]);
                    }

                    if (num5 >= 10000)
                    {
                        buf.Append(bytes[num5 / 10000 % 10]);
                    }

                    if (num5 >= 1000)
                    {
                        buf.Append(bytes[num5 / 1000 % 10]);
                    }

                    if (num5 >= 100)
                    {
                        buf.Append(bytes[num5 / 100 % 10]);
                    }

                    if (num5 % 100 != 0)
                    {
                        buf.Append((byte)46);
                        buf.Append(bytes[num5 / 10 % 10]);
                        if (num5 % 10 != 0)
                        {
                            buf.Append(bytes[num5 % 10]);
                        }
                    }

                    return null;
                }

                StringBuilder stringBuilder2 = new StringBuilder();
                if (flag)
                {
                    stringBuilder2.Append('-');
                }

                if (num5 >= 1000000)
                {
                    stringBuilder2.Append(chars[num5 / 1000000]);
                }

                if (num5 >= 100000)
                {
                    stringBuilder2.Append(chars[num5 / 100000 % 10]);
                }

                if (num5 >= 10000)
                {
                    stringBuilder2.Append(chars[num5 / 10000 % 10]);
                }

                if (num5 >= 1000)
                {
                    stringBuilder2.Append(chars[num5 / 1000 % 10]);
                }

                if (num5 >= 100)
                {
                    stringBuilder2.Append(chars[num5 / 100 % 10]);
                }

                if (num5 % 100 != 0)
                {
                    stringBuilder2.Append('.');
                    stringBuilder2.Append(chars[num5 / 10 % 10]);
                    if (num5 % 10 != 0)
                    {
                        stringBuilder2.Append(chars[num5 % 10]);
                    }
                }

                return stringBuilder2.ToString();
            }

            d += 0.5;
            long num8 = (long)d;
            if (flag)
            {
                return "-" + num8.ToString(CultureInfo.InvariantCulture);
            }

            return num8.ToString(CultureInfo.InvariantCulture);
        }

        public virtual void Reset()
        {
            count = 0;
        }

        public virtual byte[] ToByteArray()
        {
            byte[] array = new byte[count];
            Array.Copy(buf, 0, array, 0, count);
            return array;
        }

        public override string ToString()
        {
            return new string(ConvertToChar(buf), 0, count);
        }

        public virtual void WriteTo(Stream str)
        {
            str.Write(buf, 0, count);
        }

        private char[] ConvertToChar(byte[] buf)
        {
            char[] array = new char[count + 1];
            for (int i = 0; i <= count; i++)
            {
                array[i] = (char)buf[i];
            }

            return array;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0L;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Append(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            Append(value);
        }
    }
}
