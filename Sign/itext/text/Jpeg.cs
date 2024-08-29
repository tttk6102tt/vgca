using Sign.itext.error_messages;
using Sign.itext.text.pdf;
using Sign.SystemItext.util;
using System.Net;
using System.Text;

namespace Sign.itext.text
{
    public class Jpeg : Image
    {
        public const int NOT_A_MARKER = -1;

        public const int VALID_MARKER = 0;

        public static int[] VALID_MARKERS = new int[3] { 192, 193, 194 };

        public const int UNSUPPORTED_MARKER = 1;

        public static int[] UNSUPPORTED_MARKERS = new int[11]
        {
            195, 197, 198, 199, 200, 201, 202, 203, 205, 206,
            207
        };

        public const int NOPARAM_MARKER = 2;

        public static int[] NOPARAM_MARKERS = new int[10] { 208, 209, 210, 211, 212, 213, 214, 215, 216, 1 };

        public const int M_APP0 = 224;

        public const int M_APP2 = 226;

        public const int M_APPE = 238;

        public const int M_APPD = 237;

        public static byte[] JFIF_ID = new byte[5] { 74, 70, 73, 70, 0 };

        public static readonly byte[] PS_8BIM_RESO = new byte[6] { 56, 66, 73, 77, 3, 237 };

        private byte[][] icc;

        public Jpeg(Image image)
            : base(image)
        {
        }

        public Jpeg(Uri Uri)
            : base(Uri)
        {
            ProcessParameters();
        }

        public Jpeg(byte[] img)
            : base((Uri)null)
        {
            rawData = img;
            originalData = img;
            ProcessParameters();
        }

        public Jpeg(byte[] img, float width, float height)
            : this(img)
        {
            scaledWidth = width;
            scaledHeight = height;
        }

        private static int GetShort(Stream istr)
        {
            return (istr.ReadByte() << 8) + istr.ReadByte();
        }

        private static int GetShortInverted(Stream istr)
        {
            return istr.ReadByte() + istr.ReadByte() << 8;
        }

        private static int MarkerType(int marker)
        {
            for (int i = 0; i < VALID_MARKERS.Length; i++)
            {
                if (marker == VALID_MARKERS[i])
                {
                    return 0;
                }
            }

            for (int j = 0; j < NOPARAM_MARKERS.Length; j++)
            {
                if (marker == NOPARAM_MARKERS[j])
                {
                    return 2;
                }
            }

            for (int k = 0; k < UNSUPPORTED_MARKERS.Length; k++)
            {
                if (marker == UNSUPPORTED_MARKERS[k])
                {
                    return 1;
                }
            }

            return -1;
        }

        private void ProcessParameters()
        {
            type = 32;
            originalType = 1;
            Stream stream = null;
            try
            {
                string text;
                if (rawData == null)
                {
                    WebRequest webRequest = WebRequest.Create(url);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    stream = webRequest.GetResponse().GetResponseStream();
                    text = url.ToString();
                }
                else
                {
                    stream = new MemoryStream(rawData);
                    text = "Byte array";
                }

                if (stream.ReadByte() != 255 || stream.ReadByte() != 216)
                {
                    throw new BadElementException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.jpeg.file", text));
                }

                bool flag = true;
                while (true)
                {
                    int num = stream.ReadByte();
                    if (num < 0)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("premature.eof.while.reading.jpg"));
                    }

                    if (num != 255)
                    {
                        continue;
                    }

                    int num2 = stream.ReadByte();
                    if (flag && num2 == 224)
                    {
                        flag = false;
                        int @short = GetShort(stream);
                        if (@short < 16)
                        {
                            Utilities.Skip(stream, @short - 2);
                            continue;
                        }

                        byte[] array = new byte[JFIF_ID.Length];
                        if (stream.Read(array, 0, array.Length) != array.Length)
                        {
                            throw new BadElementException(MessageLocalization.GetComposedMessage("1.corrupted.jfif.marker", text));
                        }

                        bool flag2 = true;
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i] != JFIF_ID[i])
                            {
                                flag2 = false;
                                break;
                            }
                        }

                        if (!flag2)
                        {
                            Utilities.Skip(stream, @short - 2 - array.Length);
                            continue;
                        }

                        Utilities.Skip(stream, 2);
                        int num3 = stream.ReadByte();
                        int short2 = GetShort(stream);
                        int short3 = GetShort(stream);
                        switch (num3)
                        {
                            case 1:
                                dpiX = short2;
                                dpiY = short3;
                                break;
                            case 2:
                                dpiX = (int)((float)short2 * 2.54f + 0.5f);
                                dpiY = (int)((float)short3 * 2.54f + 0.5f);
                                break;
                        }

                        Utilities.Skip(stream, @short - 2 - array.Length - 7);
                        continue;
                    }

                    switch (num2)
                    {
                        case 238:
                            {
                                int @short = GetShort(stream) - 2;
                                byte[] array4 = new byte[@short];
                                for (int m = 0; m < @short; m++)
                                {
                                    array4[m] = (byte)stream.ReadByte();
                                }

                                if (array4.Length >= 12 && Util.EqualsIgnoreCase(Encoding.ASCII.GetString(array4, 0, 5), "adobe"))
                                {
                                    invert = true;
                                }

                                continue;
                            }
                        case 226:
                            {
                                int @short = GetShort(stream) - 2;
                                byte[] array3 = new byte[@short];
                                for (int l = 0; l < @short; l++)
                                {
                                    array3[l] = (byte)stream.ReadByte();
                                }

                                if (array3.Length >= 14 && Encoding.ASCII.GetString(array3, 0, 11).Equals("ICC_PROFILE"))
                                {
                                    int num9 = array3[12] & 0xFF;
                                    int num10 = array3[13] & 0xFF;
                                    if (num9 < 1)
                                    {
                                        num9 = 1;
                                    }

                                    if (num10 < 1)
                                    {
                                        num10 = 1;
                                    }

                                    if (icc == null)
                                    {
                                        icc = new byte[num10][];
                                    }

                                    icc[num9 - 1] = array3;
                                }

                                continue;
                            }
                        case 237:
                            {
                                int @short = GetShort(stream) - 2;
                                byte[] array2 = new byte[@short];
                                for (int j = 0; j < @short; j++)
                                {
                                    array2[j] = (byte)stream.ReadByte();
                                }

                                int num4 = 0;
                                for (num4 = 0; num4 < @short - PS_8BIM_RESO.Length; num4++)
                                {
                                    bool flag3 = true;
                                    for (int k = 0; k < PS_8BIM_RESO.Length; k++)
                                    {
                                        if (array2[num4 + k] != PS_8BIM_RESO[k])
                                        {
                                            flag3 = false;
                                            break;
                                        }
                                    }

                                    if (flag3)
                                    {
                                        break;
                                    }
                                }

                                num4 += PS_8BIM_RESO.Length;
                                if (num4 >= @short - PS_8BIM_RESO.Length)
                                {
                                    continue;
                                }

                                byte b = array2[num4];
                                b = (byte)(b + 1);
                                if ((int)b % 2 == 1)
                                {
                                    b = (byte)(b + 1);
                                }

                                num4 += b;
                                if ((array2[num4] << 24) + (array2[num4 + 1] << 16) + (array2[num4 + 2] << 8) + array2[num4 + 3] != 16)
                                {
                                    continue;
                                }

                                num4 += 4;
                                int num5 = (array2[num4] << 8) + (array2[num4 + 1] & 0xFF);
                                num4 += 2;
                                num4 += 2;
                                int num6 = (array2[num4] << 8) + (array2[num4 + 1] & 0xFF);
                                num4 += 2;
                                num4 += 2;
                                int num7 = (array2[num4] << 8) + (array2[num4 + 1] & 0xFF);
                                num4 += 2;
                                num4 += 2;
                                int num8 = (array2[num4] << 8) + (array2[num4 + 1] & 0xFF);
                                if (num6 == 1 || num6 == 2)
                                {
                                    num5 = ((num6 == 2) ? ((int)((float)num5 * 2.54f + 0.5f)) : num5);
                                    if (dpiX == 0 || dpiX == num5)
                                    {
                                        dpiX = num5;
                                    }
                                }

                                if (num8 == 1 || num8 == 2)
                                {
                                    num7 = ((num8 == 2) ? ((int)((float)num7 * 2.54f + 0.5f)) : num7);
                                    if (dpiY == 0 || dpiY == num7)
                                    {
                                        dpiY = num7;
                                    }
                                }

                                continue;
                            }
                    }

                    flag = false;
                    switch (MarkerType(num2))
                    {
                        case 2:
                            break;
                        case 0:
                            Utilities.Skip(stream, 2);
                            if (stream.ReadByte() != 8)
                            {
                                throw new BadElementException(MessageLocalization.GetComposedMessage("1.must.have.8.bits.per.component", text));
                            }

                            scaledHeight = GetShort(stream);
                            Top = scaledHeight;
                            scaledWidth = GetShort(stream);
                            Right = scaledWidth;
                            colorspace = stream.ReadByte();
                            bpc = 8;
                            goto end_IL_0011;
                        case 1:
                            throw new BadElementException(MessageLocalization.GetComposedMessage("1.unsupported.jpeg.marker.2", text, num2));
                        default:
                            Utilities.Skip(stream, GetShort(stream) - 2);
                            break;
                    }
                }

                end_IL_0011:;
            }
            finally
            {
                stream?.Close();
            }

            plainWidth = Width;
            plainHeight = Height;
            if (icc == null)
            {
                return;
            }

            int num11 = 0;
            for (int n = 0; n < icc.Length; n++)
            {
                if (icc[n] == null)
                {
                    icc = null;
                    return;
                }

                num11 += icc[n].Length - 14;
            }

            byte[] array5 = new byte[num11];
            num11 = 0;
            for (int num12 = 0; num12 < icc.Length; num12++)
            {
                Array.Copy(icc[num12], 14, array5, num11, icc[num12].Length - 14);
                num11 += icc[num12].Length - 14;
            }

            try
            {
                ICC_Profile iCC_Profile = (TagICC = ICC_Profile.GetInstance(array5, colorspace));
            }
            catch
            {
            }

            icc = null;
        }
    }
}
