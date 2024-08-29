using Sign.itext.pdf;

namespace Sign.itext.text.pdf.codec
{
    public class CCITTG4Encoder
    {
        private int rowbytes;

        private int rowpixels;

        private int bit = 8;

        private int data;

        private byte[] refline;

        private ByteBuffer outBuf = new ByteBuffer(1024);

        private byte[] dataBp;

        private int offsetData;

        private int sizeData;

        private static byte[] zeroruns = new byte[256]
        {
            8, 7, 6, 6, 5, 5, 5, 5, 4, 4,
            4, 4, 4, 4, 4, 4, 3, 3, 3, 3,
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            3, 3, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0
        };

        private static byte[] oneruns = new byte[256]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 3, 3, 3, 3, 3, 3,
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            4, 4, 4, 4, 4, 4, 4, 4, 5, 5,
            5, 5, 6, 6, 7, 8
        };

        private const int LENGTH = 0;

        private const int CODE = 1;

        private const int RUNLEN = 2;

        private const int EOL = 1;

        private const int G3CODE_EOL = -1;

        private const int G3CODE_INVALID = -2;

        private const int G3CODE_EOF = -3;

        private const int G3CODE_INCOMP = -4;

        private int[][] TIFFFaxWhiteCodes = new int[109][]
        {
            new int[3] { 8, 53, 0 },
            new int[3] { 6, 7, 1 },
            new int[3] { 4, 7, 2 },
            new int[3] { 4, 8, 3 },
            new int[3] { 4, 11, 4 },
            new int[3] { 4, 12, 5 },
            new int[3] { 4, 14, 6 },
            new int[3] { 4, 15, 7 },
            new int[3] { 5, 19, 8 },
            new int[3] { 5, 20, 9 },
            new int[3] { 5, 7, 10 },
            new int[3] { 5, 8, 11 },
            new int[3] { 6, 8, 12 },
            new int[3] { 6, 3, 13 },
            new int[3] { 6, 52, 14 },
            new int[3] { 6, 53, 15 },
            new int[3] { 6, 42, 16 },
            new int[3] { 6, 43, 17 },
            new int[3] { 7, 39, 18 },
            new int[3] { 7, 12, 19 },
            new int[3] { 7, 8, 20 },
            new int[3] { 7, 23, 21 },
            new int[3] { 7, 3, 22 },
            new int[3] { 7, 4, 23 },
            new int[3] { 7, 40, 24 },
            new int[3] { 7, 43, 25 },
            new int[3] { 7, 19, 26 },
            new int[3] { 7, 36, 27 },
            new int[3] { 7, 24, 28 },
            new int[3] { 8, 2, 29 },
            new int[3] { 8, 3, 30 },
            new int[3] { 8, 26, 31 },
            new int[3] { 8, 27, 32 },
            new int[3] { 8, 18, 33 },
            new int[3] { 8, 19, 34 },
            new int[3] { 8, 20, 35 },
            new int[3] { 8, 21, 36 },
            new int[3] { 8, 22, 37 },
            new int[3] { 8, 23, 38 },
            new int[3] { 8, 40, 39 },
            new int[3] { 8, 41, 40 },
            new int[3] { 8, 42, 41 },
            new int[3] { 8, 43, 42 },
            new int[3] { 8, 44, 43 },
            new int[3] { 8, 45, 44 },
            new int[3] { 8, 4, 45 },
            new int[3] { 8, 5, 46 },
            new int[3] { 8, 10, 47 },
            new int[3] { 8, 11, 48 },
            new int[3] { 8, 82, 49 },
            new int[3] { 8, 83, 50 },
            new int[3] { 8, 84, 51 },
            new int[3] { 8, 85, 52 },
            new int[3] { 8, 36, 53 },
            new int[3] { 8, 37, 54 },
            new int[3] { 8, 88, 55 },
            new int[3] { 8, 89, 56 },
            new int[3] { 8, 90, 57 },
            new int[3] { 8, 91, 58 },
            new int[3] { 8, 74, 59 },
            new int[3] { 8, 75, 60 },
            new int[3] { 8, 50, 61 },
            new int[3] { 8, 51, 62 },
            new int[3] { 8, 52, 63 },
            new int[3] { 5, 27, 64 },
            new int[3] { 5, 18, 128 },
            new int[3] { 6, 23, 192 },
            new int[3] { 7, 55, 256 },
            new int[3] { 8, 54, 320 },
            new int[3] { 8, 55, 384 },
            new int[3] { 8, 100, 448 },
            new int[3] { 8, 101, 512 },
            new int[3] { 8, 104, 576 },
            new int[3] { 8, 103, 640 },
            new int[3] { 9, 204, 704 },
            new int[3] { 9, 205, 768 },
            new int[3] { 9, 210, 832 },
            new int[3] { 9, 211, 896 },
            new int[3] { 9, 212, 960 },
            new int[3] { 9, 213, 1024 },
            new int[3] { 9, 214, 1088 },
            new int[3] { 9, 215, 1152 },
            new int[3] { 9, 216, 1216 },
            new int[3] { 9, 217, 1280 },
            new int[3] { 9, 218, 1344 },
            new int[3] { 9, 219, 1408 },
            new int[3] { 9, 152, 1472 },
            new int[3] { 9, 153, 1536 },
            new int[3] { 9, 154, 1600 },
            new int[3] { 6, 24, 1664 },
            new int[3] { 9, 155, 1728 },
            new int[3] { 11, 8, 1792 },
            new int[3] { 11, 12, 1856 },
            new int[3] { 11, 13, 1920 },
            new int[3] { 12, 18, 1984 },
            new int[3] { 12, 19, 2048 },
            new int[3] { 12, 20, 2112 },
            new int[3] { 12, 21, 2176 },
            new int[3] { 12, 22, 2240 },
            new int[3] { 12, 23, 2304 },
            new int[3] { 12, 28, 2368 },
            new int[3] { 12, 29, 2432 },
            new int[3] { 12, 30, 2496 },
            new int[3] { 12, 31, 2560 },
            new int[3] { 12, 1, -1 },
            new int[3] { 9, 1, -2 },
            new int[3] { 10, 1, -2 },
            new int[3] { 11, 1, -2 },
            new int[3] { 12, 0, -2 }
        };

        private int[][] TIFFFaxBlackCodes = new int[109][]
        {
            new int[3] { 10, 55, 0 },
            new int[3] { 3, 2, 1 },
            new int[3] { 2, 3, 2 },
            new int[3] { 2, 2, 3 },
            new int[3] { 3, 3, 4 },
            new int[3] { 4, 3, 5 },
            new int[3] { 4, 2, 6 },
            new int[3] { 5, 3, 7 },
            new int[3] { 6, 5, 8 },
            new int[3] { 6, 4, 9 },
            new int[3] { 7, 4, 10 },
            new int[3] { 7, 5, 11 },
            new int[3] { 7, 7, 12 },
            new int[3] { 8, 4, 13 },
            new int[3] { 8, 7, 14 },
            new int[3] { 9, 24, 15 },
            new int[3] { 10, 23, 16 },
            new int[3] { 10, 24, 17 },
            new int[3] { 10, 8, 18 },
            new int[3] { 11, 103, 19 },
            new int[3] { 11, 104, 20 },
            new int[3] { 11, 108, 21 },
            new int[3] { 11, 55, 22 },
            new int[3] { 11, 40, 23 },
            new int[3] { 11, 23, 24 },
            new int[3] { 11, 24, 25 },
            new int[3] { 12, 202, 26 },
            new int[3] { 12, 203, 27 },
            new int[3] { 12, 204, 28 },
            new int[3] { 12, 205, 29 },
            new int[3] { 12, 104, 30 },
            new int[3] { 12, 105, 31 },
            new int[3] { 12, 106, 32 },
            new int[3] { 12, 107, 33 },
            new int[3] { 12, 210, 34 },
            new int[3] { 12, 211, 35 },
            new int[3] { 12, 212, 36 },
            new int[3] { 12, 213, 37 },
            new int[3] { 12, 214, 38 },
            new int[3] { 12, 215, 39 },
            new int[3] { 12, 108, 40 },
            new int[3] { 12, 109, 41 },
            new int[3] { 12, 218, 42 },
            new int[3] { 12, 219, 43 },
            new int[3] { 12, 84, 44 },
            new int[3] { 12, 85, 45 },
            new int[3] { 12, 86, 46 },
            new int[3] { 12, 87, 47 },
            new int[3] { 12, 100, 48 },
            new int[3] { 12, 101, 49 },
            new int[3] { 12, 82, 50 },
            new int[3] { 12, 83, 51 },
            new int[3] { 12, 36, 52 },
            new int[3] { 12, 55, 53 },
            new int[3] { 12, 56, 54 },
            new int[3] { 12, 39, 55 },
            new int[3] { 12, 40, 56 },
            new int[3] { 12, 88, 57 },
            new int[3] { 12, 89, 58 },
            new int[3] { 12, 43, 59 },
            new int[3] { 12, 44, 60 },
            new int[3] { 12, 90, 61 },
            new int[3] { 12, 102, 62 },
            new int[3] { 12, 103, 63 },
            new int[3] { 10, 15, 64 },
            new int[3] { 12, 200, 128 },
            new int[3] { 12, 201, 192 },
            new int[3] { 12, 91, 256 },
            new int[3] { 12, 51, 320 },
            new int[3] { 12, 52, 384 },
            new int[3] { 12, 53, 448 },
            new int[3] { 13, 108, 512 },
            new int[3] { 13, 109, 576 },
            new int[3] { 13, 74, 640 },
            new int[3] { 13, 75, 704 },
            new int[3] { 13, 76, 768 },
            new int[3] { 13, 77, 832 },
            new int[3] { 13, 114, 896 },
            new int[3] { 13, 115, 960 },
            new int[3] { 13, 116, 1024 },
            new int[3] { 13, 117, 1088 },
            new int[3] { 13, 118, 1152 },
            new int[3] { 13, 119, 1216 },
            new int[3] { 13, 82, 1280 },
            new int[3] { 13, 83, 1344 },
            new int[3] { 13, 84, 1408 },
            new int[3] { 13, 85, 1472 },
            new int[3] { 13, 90, 1536 },
            new int[3] { 13, 91, 1600 },
            new int[3] { 13, 100, 1664 },
            new int[3] { 13, 101, 1728 },
            new int[3] { 11, 8, 1792 },
            new int[3] { 11, 12, 1856 },
            new int[3] { 11, 13, 1920 },
            new int[3] { 12, 18, 1984 },
            new int[3] { 12, 19, 2048 },
            new int[3] { 12, 20, 2112 },
            new int[3] { 12, 21, 2176 },
            new int[3] { 12, 22, 2240 },
            new int[3] { 12, 23, 2304 },
            new int[3] { 12, 28, 2368 },
            new int[3] { 12, 29, 2432 },
            new int[3] { 12, 30, 2496 },
            new int[3] { 12, 31, 2560 },
            new int[3] { 12, 1, -1 },
            new int[3] { 9, 1, -2 },
            new int[3] { 10, 1, -2 },
            new int[3] { 11, 1, -2 },
            new int[3] { 12, 0, -2 }
        };

        private int[] horizcode = new int[3] { 3, 1, 0 };

        private int[] passcode = new int[3] { 4, 1, 0 };

        private int[][] vcodes = new int[7][]
        {
            new int[3] { 7, 3, 0 },
            new int[3] { 6, 3, 0 },
            new int[3] { 3, 3, 0 },
            new int[3] { 1, 1, 0 },
            new int[3] { 3, 2, 0 },
            new int[3] { 6, 2, 0 },
            new int[3] { 7, 2, 0 }
        };

        private int[] msbmask = new int[9] { 0, 1, 3, 7, 15, 31, 63, 127, 255 };

        public CCITTG4Encoder(int width)
        {
            rowpixels = width;
            rowbytes = (rowpixels + 7) / 8;
            refline = new byte[rowbytes];
        }

        public virtual void Fax4Encode(byte[] data, int offset, int size)
        {
            dataBp = data;
            offsetData = offset;
            for (sizeData = size; sizeData > 0; sizeData -= rowbytes)
            {
                Fax3Encode2DRow();
                Array.Copy(dataBp, offsetData, refline, 0, rowbytes);
                offsetData += rowbytes;
            }
        }

        public static byte[] Compress(byte[] data, int width, int height)
        {
            CCITTG4Encoder cCITTG4Encoder = new CCITTG4Encoder(width);
            cCITTG4Encoder.Fax4Encode(data, 0, cCITTG4Encoder.rowbytes * height);
            return cCITTG4Encoder.Close();
        }

        public virtual void Fax4Encode(byte[] data, int height)
        {
            Fax4Encode(data, 0, rowbytes * height);
        }

        private void Putcode(int[] table)
        {
            PutBits(table[1], table[0]);
        }

        private void Putspan(int span, int[][] tab)
        {
            int bits;
            int length;
            while (span >= 2624)
            {
                int[] array = tab[103];
                bits = array[1];
                length = array[0];
                PutBits(bits, length);
                span -= array[2];
            }

            if (span >= 64)
            {
                int[] array2 = tab[63 + (span >> 6)];
                bits = array2[1];
                length = array2[0];
                PutBits(bits, length);
                span -= array2[2];
            }

            bits = tab[span][1];
            length = tab[span][0];
            PutBits(bits, length);
        }

        private void PutBits(int bits, int length)
        {
            while (length > bit)
            {
                data |= bits >> length - bit;
                length -= bit;
                outBuf.Append((byte)data);
                data = 0;
                bit = 8;
            }

            data |= (bits & msbmask[length]) << bit - length;
            bit -= length;
            if (bit == 0)
            {
                outBuf.Append((byte)data);
                data = 0;
                bit = 8;
            }
        }

        private void Fax3Encode2DRow()
        {
            int num = 0;
            int num2 = ((Pixel(dataBp, offsetData, 0) == 0) ? Finddiff(dataBp, offsetData, 0, rowpixels, 0) : 0);
            int num3 = ((Pixel(refline, 0, 0) == 0) ? Finddiff(refline, 0, 0, rowpixels, 0) : 0);
            while (true)
            {
                int num4 = Finddiff2(refline, 0, num3, rowpixels, Pixel(refline, 0, num3));
                if (num4 >= num2)
                {
                    int num5 = num3 - num2;
                    if (-3 > num5 || num5 > 3)
                    {
                        int num6 = Finddiff2(dataBp, offsetData, num2, rowpixels, Pixel(dataBp, offsetData, num2));
                        Putcode(horizcode);
                        if (num + num2 == 0 || Pixel(dataBp, offsetData, num) == 0)
                        {
                            Putspan(num2 - num, TIFFFaxWhiteCodes);
                            Putspan(num6 - num2, TIFFFaxBlackCodes);
                        }
                        else
                        {
                            Putspan(num2 - num, TIFFFaxBlackCodes);
                            Putspan(num6 - num2, TIFFFaxWhiteCodes);
                        }

                        num = num6;
                    }
                    else
                    {
                        Putcode(vcodes[num5 + 3]);
                        num = num2;
                    }
                }
                else
                {
                    Putcode(passcode);
                    num = num4;
                }

                if (num < rowpixels)
                {
                    num2 = Finddiff(dataBp, offsetData, num, rowpixels, Pixel(dataBp, offsetData, num));
                    num3 = Finddiff(refline, 0, num, rowpixels, Pixel(dataBp, offsetData, num) ^ 1);
                    num3 = Finddiff(refline, 0, num3, rowpixels, Pixel(dataBp, offsetData, num));
                    continue;
                }

                break;
            }
        }

        private void Fax4PostEncode()
        {
            PutBits(1, 12);
            PutBits(1, 12);
            if (bit != 8)
            {
                outBuf.Append((byte)data);
                data = 0;
                bit = 8;
            }
        }

        public virtual byte[] Close()
        {
            Fax4PostEncode();
            return outBuf.ToByteArray();
        }

        private int Pixel(byte[] data, int offset, int bit)
        {
            if (bit >= rowpixels)
            {
                return 0;
            }

            return ((data[offset + (bit >> 3)] & 0xFF) >> 7 - (bit & 7)) & 1;
        }

        private static int Find1span(byte[] bp, int offset, int bs, int be)
        {
            int num = be - bs;
            int num2 = offset + (bs >> 3);
            int num4;
            int num3;
            if (num > 0 && (num3 = bs & 7) != 0)
            {
                num4 = oneruns[(bp[num2] << num3) & 0xFF];
                if (num4 > 8 - num3)
                {
                    num4 = 8 - num3;
                }

                if (num4 > num)
                {
                    num4 = num;
                }

                if (num3 + num4 < 8)
                {
                    return num4;
                }

                num -= num4;
                num2++;
            }
            else
            {
                num4 = 0;
            }

            while (num >= 8)
            {
                if (bp[num2] != byte.MaxValue)
                {
                    return num4 + oneruns[bp[num2] & 0xFF];
                }

                num4 += 8;
                num -= 8;
                num2++;
            }

            if (num > 0)
            {
                num3 = oneruns[bp[num2] & 0xFF];
                num4 += ((num3 > num) ? num : num3);
            }

            return num4;
        }

        private static int Find0span(byte[] bp, int offset, int bs, int be)
        {
            int num = be - bs;
            int num2 = offset + (bs >> 3);
            int num4;
            int num3;
            if (num > 0 && (num3 = bs & 7) != 0)
            {
                num4 = zeroruns[(bp[num2] << num3) & 0xFF];
                if (num4 > 8 - num3)
                {
                    num4 = 8 - num3;
                }

                if (num4 > num)
                {
                    num4 = num;
                }

                if (num3 + num4 < 8)
                {
                    return num4;
                }

                num -= num4;
                num2++;
            }
            else
            {
                num4 = 0;
            }

            while (num >= 8)
            {
                if (bp[num2] != 0)
                {
                    return num4 + zeroruns[bp[num2] & 0xFF];
                }

                num4 += 8;
                num -= 8;
                num2++;
            }

            if (num > 0)
            {
                num3 = zeroruns[bp[num2] & 0xFF];
                num4 += ((num3 > num) ? num : num3);
            }

            return num4;
        }

        private static int Finddiff(byte[] bp, int offset, int bs, int be, int color)
        {
            return bs + ((color != 0) ? Find1span(bp, offset, bs, be) : Find0span(bp, offset, bs, be));
        }

        private static int Finddiff2(byte[] bp, int offset, int bs, int be, int color)
        {
            if (bs >= be)
            {
                return be;
            }

            return Finddiff(bp, offset, bs, be, color);
        }
    }
}
