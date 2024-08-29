using System.Text;

namespace Sign.itext.xml.xmp.impl
{
    public class Latin1Converter
    {
        private const int STATE_START = 0;

        private const int STATE_UTF8CHAR = 11;

        private Latin1Converter()
        {
        }

        public static ByteBuffer Convert(ByteBuffer buffer)
        {
            if ("UTF-8".Equals(buffer.Encoding))
            {
                byte[] array = new byte[8];
                int num = 0;
                int num2 = 0;
                ByteBuffer byteBuffer = new ByteBuffer(buffer.Length * 4 / 3);
                int num3 = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    int num4 = buffer.ByteAt(i);
                    if (num3 == 0 || num3 != 11)
                    {
                        if (num4 < 127)
                        {
                            byteBuffer.Append((byte)num4);
                        }
                        else if (num4 >= 192)
                        {
                            num2 = -1;
                            int num5 = num4;
                            while (num2 < 8 && (num5 & 0x80) == 128)
                            {
                                num2++;
                                num5 <<= 1;
                            }

                            array[num++] = (byte)num4;
                            num3 = 11;
                        }
                        else
                        {
                            byte[] bytes = ConvertToUtf8((byte)num4);
                            byteBuffer.Append(bytes);
                        }
                    }
                    else if (num2 > 0 && (num4 & 0xC0) == 128)
                    {
                        array[num++] = (byte)num4;
                        num2--;
                        if (num2 == 0)
                        {
                            byteBuffer.Append(array, 0, num);
                            num = 0;
                            num3 = 0;
                        }
                    }
                    else
                    {
                        byte[] bytes2 = ConvertToUtf8(array[0]);
                        byteBuffer.Append(bytes2);
                        i -= num;
                        num = 0;
                        num3 = 0;
                    }
                }

                if (num3 == 11)
                {
                    for (int j = 0; j < num; j++)
                    {
                        byte[] bytes3 = ConvertToUtf8(array[j]);
                        byteBuffer.Append(bytes3);
                    }
                }

                return byteBuffer;
            }

            return buffer;
        }

        private static byte[] ConvertToUtf8(byte ch)
        {
            int num = ch & 0xFF;
            try
            {
                if (num >= 128)
                {
                    if (num == 129 || num == 141 || num == 143 || num == 144 || num == 157)
                    {
                        return new byte[1] { 32 };
                    }

                    string @string = Encoding.Default.GetString(new byte[1] { ch });
                    return Encoding.UTF8.GetBytes(@string);
                }
            }
            catch (Exception)
            {
            }

            return new byte[1] { ch };
        }
    }
}
