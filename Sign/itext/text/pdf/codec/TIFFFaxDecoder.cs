using Sign.itext.error_messages;
using Sign.itext.text.exceptions;

namespace Sign.itext.text.pdf.codec
{
    public class TIFFFaxDecoder
    {
        private int bitPointer;

        private int bytePointer;

        private byte[] data;

        private int w;

        private int h;

        private int fillOrder;

        private int changingElemSize;

        private int[] prevChangingElems;

        private int[] currChangingElems;

        private int lastChangingElement;

        private int compression = 2;

        private int uncompressedMode;

        private int fillBits;

        private int oneD;

        private bool recoverFromImageError;

        private static int[] table1 = new int[9] { 0, 1, 3, 7, 15, 31, 63, 127, 255 };

        private static int[] table2 = new int[9] { 0, 128, 192, 224, 240, 248, 252, 254, 255 };

        internal static byte[] flipTable = new byte[256]
        {
            0, 128, 64, 192, 32, 160, 96, 224, 16, 144,
            80, 208, 48, 176, 112, 240, 8, 136, 72, 200,
            40, 168, 104, 232, 24, 152, 88, 216, 56, 184,
            120, 248, 4, 132, 68, 196, 36, 164, 100, 228,
            20, 148, 84, 212, 52, 180, 116, 244, 12, 140,
            76, 204, 44, 172, 108, 236, 28, 156, 92, 220,
            60, 188, 124, 252, 2, 130, 66, 194, 34, 162,
            98, 226, 18, 146, 82, 210, 50, 178, 114, 242,
            10, 138, 74, 202, 42, 170, 106, 234, 26, 154,
            90, 218, 58, 186, 122, 250, 6, 134, 70, 198,
            38, 166, 102, 230, 22, 150, 86, 214, 54, 182,
            118, 246, 14, 142, 78, 206, 46, 174, 110, 238,
            30, 158, 94, 222, 62, 190, 126, 254, 1, 129,
            65, 193, 33, 161, 97, 225, 17, 145, 81, 209,
            49, 177, 113, 241, 9, 137, 73, 201, 41, 169,
            105, 233, 25, 153, 89, 217, 57, 185, 121, 249,
            5, 133, 69, 197, 37, 165, 101, 229, 21, 149,
            85, 213, 53, 181, 117, 245, 13, 141, 77, 205,
            45, 173, 109, 237, 29, 157, 93, 221, 61, 189,
            125, 253, 3, 131, 67, 195, 35, 163, 99, 227,
            19, 147, 83, 211, 51, 179, 115, 243, 11, 139,
            75, 203, 43, 171, 107, 235, 27, 155, 91, 219,
            59, 187, 123, 251, 7, 135, 71, 199, 39, 167,
            103, 231, 23, 151, 87, 215, 55, 183, 119, 247,
            15, 143, 79, 207, 47, 175, 111, 239, 31, 159,
            95, 223, 63, 191, 127, 255
        };

        private static short[] white = new short[1024]
        {
            6430, 6400, 6400, 6400, 3225, 3225, 3225, 3225, 944, 944,
            944, 944, 976, 976, 976, 976, 1456, 1456, 1456, 1456,
            1488, 1488, 1488, 1488, 718, 718, 718, 718, 718, 718,
            718, 718, 750, 750, 750, 750, 750, 750, 750, 750,
            1520, 1520, 1520, 1520, 1552, 1552, 1552, 1552, 428, 428,
            428, 428, 428, 428, 428, 428, 428, 428, 428, 428,
            428, 428, 428, 428, 654, 654, 654, 654, 654, 654,
            654, 654, 1072, 1072, 1072, 1072, 1104, 1104, 1104, 1104,
            1136, 1136, 1136, 1136, 1168, 1168, 1168, 1168, 1200, 1200,
            1200, 1200, 1232, 1232, 1232, 1232, 622, 622, 622, 622,
            622, 622, 622, 622, 1008, 1008, 1008, 1008, 1040, 1040,
            1040, 1040, 44, 44, 44, 44, 44, 44, 44, 44,
            44, 44, 44, 44, 44, 44, 44, 44, 396, 396,
            396, 396, 396, 396, 396, 396, 396, 396, 396, 396,
            396, 396, 396, 396, 1712, 1712, 1712, 1712, 1744, 1744,
            1744, 1744, 846, 846, 846, 846, 846, 846, 846, 846,
            1264, 1264, 1264, 1264, 1296, 1296, 1296, 1296, 1328, 1328,
            1328, 1328, 1360, 1360, 1360, 1360, 1392, 1392, 1392, 1392,
            1424, 1424, 1424, 1424, 686, 686, 686, 686, 686, 686,
            686, 686, 910, 910, 910, 910, 910, 910, 910, 910,
            1968, 1968, 1968, 1968, 2000, 2000, 2000, 2000, 2032, 2032,
            2032, 2032, 16, 16, 16, 16, 10257, 10257, 10257, 10257,
            12305, 12305, 12305, 12305, 330, 330, 330, 330, 330, 330,
            330, 330, 330, 330, 330, 330, 330, 330, 330, 330,
            330, 330, 330, 330, 330, 330, 330, 330, 330, 330,
            330, 330, 330, 330, 330, 330, 362, 362, 362, 362,
            362, 362, 362, 362, 362, 362, 362, 362, 362, 362,
            362, 362, 362, 362, 362, 362, 362, 362, 362, 362,
            362, 362, 362, 362, 362, 362, 362, 362, 878, 878,
            878, 878, 878, 878, 878, 878, 1904, 1904, 1904, 1904,
            1936, 1936, 1936, 1936, -18413, -18413, -16365, -16365, -14317, -14317,
            -10221, -10221, 590, 590, 590, 590, 590, 590, 590, 590,
            782, 782, 782, 782, 782, 782, 782, 782, 1584, 1584,
            1584, 1584, 1616, 1616, 1616, 1616, 1648, 1648, 1648, 1648,
            1680, 1680, 1680, 1680, 814, 814, 814, 814, 814, 814,
            814, 814, 1776, 1776, 1776, 1776, 1808, 1808, 1808, 1808,
            1840, 1840, 1840, 1840, 1872, 1872, 1872, 1872, 6157, 6157,
            6157, 6157, 6157, 6157, 6157, 6157, 6157, 6157, 6157, 6157,
            6157, 6157, 6157, 6157, -12275, -12275, -12275, -12275, -12275, -12275,
            -12275, -12275, -12275, -12275, -12275, -12275, -12275, -12275, -12275, -12275,
            14353, 14353, 14353, 14353, 16401, 16401, 16401, 16401, 22547, 22547,
            24595, 24595, 20497, 20497, 20497, 20497, 18449, 18449, 18449, 18449,
            26643, 26643, 28691, 28691, 30739, 30739, -32749, -32749, -30701, -30701,
            -28653, -28653, -26605, -26605, -24557, -24557, -22509, -22509, -20461, -20461,
            8207, 8207, 8207, 8207, 8207, 8207, 8207, 8207, 72, 72,
            72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
            72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
            72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
            72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
            72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
            72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
            72, 72, 104, 104, 104, 104, 104, 104, 104, 104,
            104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
            104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
            104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
            104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
            104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
            104, 104, 104, 104, 104, 104, 4107, 4107, 4107, 4107,
            4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107,
            4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107,
            4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 266, 266,
            266, 266, 266, 266, 266, 266, 266, 266, 266, 266,
            266, 266, 266, 266, 266, 266, 266, 266, 266, 266,
            266, 266, 266, 266, 266, 266, 266, 266, 266, 266,
            298, 298, 298, 298, 298, 298, 298, 298, 298, 298,
            298, 298, 298, 298, 298, 298, 298, 298, 298, 298,
            298, 298, 298, 298, 298, 298, 298, 298, 298, 298,
            298, 298, 524, 524, 524, 524, 524, 524, 524, 524,
            524, 524, 524, 524, 524, 524, 524, 524, 556, 556,
            556, 556, 556, 556, 556, 556, 556, 556, 556, 556,
            556, 556, 556, 556, 136, 136, 136, 136, 136, 136,
            136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
            136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
            136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
            136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
            136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
            136, 136, 136, 136, 136, 136, 136, 136, 168, 168,
            168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
            168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
            168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
            168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
            168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
            168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
            168, 168, 460, 460, 460, 460, 460, 460, 460, 460,
            460, 460, 460, 460, 460, 460, 460, 460, 492, 492,
            492, 492, 492, 492, 492, 492, 492, 492, 492, 492,
            492, 492, 492, 492, 2059, 2059, 2059, 2059, 2059, 2059,
            2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059,
            2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059,
            2059, 2059, 2059, 2059, 2059, 2059, 200, 200, 200, 200,
            200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
            200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
            200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
            200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
            200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
            200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
            232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
            232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
            232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
            232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
            232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
            232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
            232, 232, 232, 232
        };

        private static short[] additionalMakeup = new short[16]
        {
            28679, 28679, 31752, -32759, -31735, -30711, -29687, -28663, 29703, 29703,
            30727, 30727, -27639, -26615, -25591, -24567
        };

        private static short[] initBlack = new short[16]
        {
            3226, 6412, 200, 168, 38, 38, 134, 134, 100, 100,
            100, 100, 68, 68, 68, 68
        };

        private static short[] twoBitBlack = new short[4] { 292, 260, 226, 226 };

        private static short[] black = new short[512]
        {
            62, 62, 30, 30, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225,
            3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225,
            3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225,
            3225, 3225, 3225, 3225, 588, 588, 588, 588, 588, 588,
            588, 588, 1680, 1680, 20499, 22547, 24595, 26643, 1776, 1776,
            1808, 1808, -24557, -22509, -20461, -18413, 1904, 1904, 1936, 1936,
            -16365, -14317, 782, 782, 782, 782, 814, 814, 814, 814,
            -12269, -10221, 10257, 10257, 12305, 12305, 14353, 14353, 16403, 18451,
            1712, 1712, 1744, 1744, 28691, 30739, -32749, -30701, -28653, -26605,
            2061, 2061, 2061, 2061, 2061, 2061, 2061, 2061, 424, 424,
            424, 424, 424, 424, 424, 424, 424, 424, 424, 424,
            424, 424, 424, 424, 424, 424, 424, 424, 424, 424,
            424, 424, 424, 424, 424, 424, 424, 424, 424, 424,
            750, 750, 750, 750, 1616, 1616, 1648, 1648, 1424, 1424,
            1456, 1456, 1488, 1488, 1520, 1520, 1840, 1840, 1872, 1872,
            1968, 1968, 8209, 8209, 524, 524, 524, 524, 524, 524,
            524, 524, 556, 556, 556, 556, 556, 556, 556, 556,
            1552, 1552, 1584, 1584, 2000, 2000, 2032, 2032, 976, 976,
            1008, 1008, 1040, 1040, 1072, 1072, 1296, 1296, 1328, 1328,
            718, 718, 718, 718, 456, 456, 456, 456, 456, 456,
            456, 456, 456, 456, 456, 456, 456, 456, 456, 456,
            456, 456, 456, 456, 456, 456, 456, 456, 456, 456,
            456, 456, 456, 456, 456, 456, 326, 326, 326, 326,
            326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
            326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
            326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
            326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
            326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
            326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
            358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
            358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
            358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
            358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
            358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
            358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
            358, 358, 358, 358, 490, 490, 490, 490, 490, 490,
            490, 490, 490, 490, 490, 490, 490, 490, 490, 490,
            4113, 4113, 6161, 6161, 848, 848, 880, 880, 912, 912,
            944, 944, 622, 622, 622, 622, 654, 654, 654, 654,
            1104, 1104, 1136, 1136, 1168, 1168, 1200, 1200, 1232, 1232,
            1264, 1264, 686, 686, 686, 686, 1360, 1360, 1392, 1392,
            12, 12, 12, 12, 12, 12, 12, 12, 390, 390,
            390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
            390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
            390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
            390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
            390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
            390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
            390, 390
        };

        private static byte[] twoDCodes = new byte[128]
        {
            80, 88, 23, 71, 30, 30, 62, 62, 4, 4,
            4, 4, 4, 4, 4, 4, 11, 11, 11, 11,
            11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
            11, 11, 35, 35, 35, 35, 35, 35, 35, 35,
            35, 35, 35, 35, 35, 35, 35, 35, 51, 51,
            51, 51, 51, 51, 51, 51, 51, 51, 51, 51,
            51, 51, 51, 51, 41, 41, 41, 41, 41, 41,
            41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
            41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
            41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
            41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
            41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
            41, 41, 41, 41, 41, 41, 41, 41
        };

        public virtual bool RecoverFromImageError
        {
            set
            {
                recoverFromImageError = value;
            }
        }

        public TIFFFaxDecoder(int fillOrder, int w, int h)
        {
            this.fillOrder = fillOrder;
            this.w = w;
            this.h = h;
            bitPointer = 0;
            bytePointer = 0;
            prevChangingElems = new int[2 * w];
            currChangingElems = new int[2 * w];
        }

        public static void ReverseBits(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = flipTable[b[i] & 0xFF];
            }
        }

        public virtual void Decode1D(byte[] buffer, byte[] compData, int startX, int height)
        {
            data = compData;
            int num = 0;
            int num2 = (w + 7) / 8;
            bitPointer = 0;
            bytePointer = 0;
            for (int i = 0; i < height; i++)
            {
                DecodeNextScanline(buffer, num, startX);
                num += num2;
            }
        }

        public virtual void DecodeNextScanline(byte[] buffer, int lineOffset, int bitOffset)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            bool flag = true;
            changingElemSize = 0;
            while (bitOffset < w)
            {
                while (flag)
                {
                    int num4 = NextNBits(10);
                    int num5 = white[num4];
                    num3 = num5 & 1;
                    num = (num5 >> 1) & 0xF;
                    switch (num)
                    {
                        case 12:
                            {
                                int num6 = NextLesserThan8Bits(2);
                                num4 = ((num4 << 2) & 0xC) | num6;
                                num5 = additionalMakeup[num4];
                                num = (num5 >> 1) & 7;
                                num2 = (num5 >> 4) & 0xFFF;
                                bitOffset += num2;
                                UpdatePointer(4 - num);
                                break;
                            }
                        case 0:
                            throw new Exception(MessageLocalization.GetComposedMessage("invalid.code.encountered"));
                        case 15:
                            throw new Exception(MessageLocalization.GetComposedMessage("eol.code.word.encountered.in.white.run"));
                        default:
                            num2 = (num5 >> 5) & 0x7FF;
                            bitOffset += num2;
                            UpdatePointer(10 - num);
                            if (num3 == 0)
                            {
                                flag = false;
                                currChangingElems[changingElemSize++] = bitOffset;
                            }

                            break;
                    }
                }

                if (bitOffset == w)
                {
                    if (compression == 2)
                    {
                        AdvancePointer();
                    }

                    break;
                }

                while (!flag)
                {
                    int num4 = NextLesserThan8Bits(4);
                    int num5 = initBlack[num4];
                    num3 = num5 & 1;
                    num = (num5 >> 1) & 0xF;
                    num2 = (num5 >> 5) & 0x7FF;
                    switch (num2)
                    {
                        case 100:
                            num4 = NextNBits(9);
                            num5 = black[num4];
                            num3 = num5 & 1;
                            num = (num5 >> 1) & 0xF;
                            num2 = (num5 >> 5) & 0x7FF;
                            switch (num)
                            {
                                case 12:
                                    UpdatePointer(5);
                                    num4 = NextLesserThan8Bits(4);
                                    num5 = additionalMakeup[num4];
                                    num = (num5 >> 1) & 7;
                                    num2 = (num5 >> 4) & 0xFFF;
                                    SetToBlack(buffer, lineOffset, bitOffset, num2);
                                    bitOffset += num2;
                                    UpdatePointer(4 - num);
                                    break;
                                case 15:
                                    throw new Exception(MessageLocalization.GetComposedMessage("eol.code.word.encountered.in.black.run"));
                                default:
                                    SetToBlack(buffer, lineOffset, bitOffset, num2);
                                    bitOffset += num2;
                                    UpdatePointer(9 - num);
                                    if (num3 == 0)
                                    {
                                        flag = true;
                                        currChangingElems[changingElemSize++] = bitOffset;
                                    }

                                    break;
                            }

                            break;
                        case 200:
                            num4 = NextLesserThan8Bits(2);
                            num5 = twoBitBlack[num4];
                            num2 = (num5 >> 5) & 0x7FF;
                            num = (num5 >> 1) & 0xF;
                            SetToBlack(buffer, lineOffset, bitOffset, num2);
                            bitOffset += num2;
                            UpdatePointer(2 - num);
                            flag = true;
                            currChangingElems[changingElemSize++] = bitOffset;
                            break;
                        default:
                            SetToBlack(buffer, lineOffset, bitOffset, num2);
                            bitOffset += num2;
                            UpdatePointer(4 - num);
                            flag = true;
                            currChangingElems[changingElemSize++] = bitOffset;
                            break;
                    }
                }

                if (bitOffset == w)
                {
                    if (compression == 2)
                    {
                        AdvancePointer();
                    }

                    break;
                }
            }

            currChangingElems[changingElemSize++] = bitOffset;
        }

        public virtual void Decode2D(byte[] buffer, byte[] compData, int startX, int height, long tiffT4Options)
        {
            data = compData;
            compression = 3;
            bitPointer = 0;
            bytePointer = 0;
            int num = (w + 7) / 8;
            int[] array = new int[2];
            int num2 = 0;
            oneD = (int)(tiffT4Options & 1);
            uncompressedMode = (int)((tiffT4Options & 2) >> 1);
            fillBits = (int)((tiffT4Options & 4) >> 2);
            if (ReadEOL(isFirstEOL: true) != 1)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("first.scanline.must.be.1d.encoded"));
            }

            int num3 = 0;
            DecodeNextScanline(buffer, num3, startX);
            num3 += num;
            for (int i = 1; i < height; i++)
            {
                if (ReadEOL(isFirstEOL: false) == 0)
                {
                    int[] array2 = prevChangingElems;
                    prevChangingElems = currChangingElems;
                    currChangingElems = array2;
                    num2 = 0;
                    int a = -1;
                    bool flag = true;
                    int num4 = startX;
                    lastChangingElement = 0;
                    while (num4 < w)
                    {
                        GetNextChangingElement(a, flag, array);
                        int num5 = array[0];
                        int num6 = array[1];
                        int num7 = NextLesserThan8Bits(7);
                        num7 = twoDCodes[num7] & 0xFF;
                        int num8 = (num7 & 0x78) >> 3;
                        int num9 = num7 & 7;
                        if (num8 == 0)
                        {
                            if (!flag)
                            {
                                SetToBlack(buffer, num3, num4, num6 - num4);
                            }

                            num4 = (a = num6);
                            UpdatePointer(7 - num9);
                        }
                        else if (num8 == 1)
                        {
                            UpdatePointer(7 - num9);
                            if (flag)
                            {
                                int num10 = DecodeWhiteCodeWord();
                                num4 += num10;
                                currChangingElems[num2++] = num4;
                                num10 = DecodeBlackCodeWord();
                                SetToBlack(buffer, num3, num4, num10);
                                num4 += num10;
                                currChangingElems[num2++] = num4;
                            }
                            else
                            {
                                int num10 = DecodeBlackCodeWord();
                                SetToBlack(buffer, num3, num4, num10);
                                num4 += num10;
                                currChangingElems[num2++] = num4;
                                num10 = DecodeWhiteCodeWord();
                                num4 += num10;
                                currChangingElems[num2++] = num4;
                            }

                            a = num4;
                        }
                        else
                        {
                            if (num8 > 8)
                            {
                                throw new Exception(MessageLocalization.GetComposedMessage("invalid.code.encountered.while.decoding.2d.group.3.compressed.data"));
                            }

                            int num11 = num5 + (num8 - 5);
                            currChangingElems[num2++] = num11;
                            if (!flag)
                            {
                                SetToBlack(buffer, num3, num4, num11 - num4);
                            }

                            num4 = (a = num11);
                            flag = !flag;
                            UpdatePointer(7 - num9);
                        }
                    }

                    currChangingElems[num2++] = num4;
                    changingElemSize = num2;
                }
                else
                {
                    DecodeNextScanline(buffer, num3, startX);
                }

                num3 += num;
            }
        }

        public virtual void DecodeT6(byte[] buffer, byte[] compData, int startX, int height, long tiffT6Options)
        {
            data = compData;
            compression = 4;
            bitPointer = 0;
            bytePointer = 0;
            int num = (w + 7) / 8;
            int[] array = new int[2];
            uncompressedMode = (int)((tiffT6Options & 2) >> 1);
            int[] array2 = currChangingElems;
            changingElemSize = 0;
            array2[changingElemSize++] = w;
            array2[changingElemSize++] = w;
            int num2 = 0;
            for (int i = 0; i < height; i++)
            {
                int a = -1;
                bool flag = true;
                int[] array3 = prevChangingElems;
                prevChangingElems = currChangingElems;
                array2 = (currChangingElems = array3);
                int num3 = 0;
                int num4 = startX;
                lastChangingElement = 0;
                while (num4 < w && bytePointer < data.Length)
                {
                    GetNextChangingElement(a, flag, array);
                    int num5 = array[0];
                    int num6 = array[1];
                    int num7 = NextLesserThan8Bits(7);
                    num7 = twoDCodes[num7] & 0xFF;
                    int num8 = (num7 & 0x78) >> 3;
                    int num9 = num7 & 7;
                    if (num8 == 0)
                    {
                        if (!flag)
                        {
                            SetToBlack(buffer, num2, num4, num6 - num4);
                        }

                        num4 = (a = num6);
                        UpdatePointer(7 - num9);
                    }
                    else if (num8 == 1)
                    {
                        UpdatePointer(7 - num9);
                        if (flag)
                        {
                            int num10 = DecodeWhiteCodeWord();
                            num4 += num10;
                            array2[num3++] = num4;
                            num10 = DecodeBlackCodeWord();
                            SetToBlack(buffer, num2, num4, num10);
                            num4 += num10;
                            array2[num3++] = num4;
                        }
                        else
                        {
                            int num10 = DecodeBlackCodeWord();
                            SetToBlack(buffer, num2, num4, num10);
                            num4 += num10;
                            array2[num3++] = num4;
                            num10 = DecodeWhiteCodeWord();
                            num4 += num10;
                            array2[num3++] = num4;
                        }

                        a = num4;
                    }
                    else if (num8 <= 8)
                    {
                        int num11 = num5 + (num8 - 5);
                        array2[num3++] = num11;
                        if (!flag)
                        {
                            SetToBlack(buffer, num2, num4, num11 - num4);
                        }

                        num4 = (a = num11);
                        flag = !flag;
                        UpdatePointer(7 - num9);
                    }
                    else if (num8 == 11)
                    {
                        if (NextLesserThan8Bits(3) != 7)
                        {
                            throw new InvalidImageException(MessageLocalization.GetComposedMessage("invalid.code.encountered.while.decoding.2d.group.4.compressed.data"));
                        }

                        int num12 = 0;
                        bool flag2 = false;
                        while (!flag2)
                        {
                            while (NextLesserThan8Bits(1) != 1)
                            {
                                num12++;
                            }

                            if (num12 > 5)
                            {
                                num12 -= 6;
                                if (!flag && num12 > 0)
                                {
                                    array2[num3++] = num4;
                                }

                                num4 += num12;
                                if (num12 > 0)
                                {
                                    flag = true;
                                }

                                if (NextLesserThan8Bits(1) == 0)
                                {
                                    if (!flag)
                                    {
                                        array2[num3++] = num4;
                                    }

                                    flag = true;
                                }
                                else
                                {
                                    if (flag)
                                    {
                                        array2[num3++] = num4;
                                    }

                                    flag = false;
                                }

                                flag2 = true;
                            }

                            if (num12 == 5)
                            {
                                if (!flag)
                                {
                                    array2[num3++] = num4;
                                }

                                num4 += num12;
                                flag = true;
                            }
                            else
                            {
                                num4 += num12;
                                array2[num3++] = num4;
                                SetToBlack(buffer, num2, num4, 1);
                                num4++;
                                flag = false;
                            }
                        }
                    }
                    else
                    {
                        num4 = w;
                        UpdatePointer(7 - num9);
                    }
                }

                if (num3 < array2.Length)
                {
                    array2[num3++] = num4;
                }

                changingElemSize = num3;
                num2 += num;
            }
        }

        private void SetToBlack(byte[] buffer, int lineOffset, int bitOffset, int numBits)
        {
            int i = 8 * lineOffset + bitOffset;
            int num = i + numBits;
            int num2 = i >> 3;
            int num3 = i & 7;
            if (num3 > 0)
            {
                int num4 = 1 << 7 - num3;
                byte b = buffer[num2];
                while (num4 > 0 && i < num)
                {
                    b = (byte)(b | (byte)num4);
                    num4 >>= 1;
                    i++;
                }

                buffer[num2] = b;
            }

            num2 = i >> 3;
            for (; i < num - 7; i += 8)
            {
                buffer[num2++] = byte.MaxValue;
            }

            for (; i < num; i++)
            {
                num2 = i >> 3;
                if (!recoverFromImageError || num2 < buffer.Length)
                {
                    buffer[num2] |= (byte)(1 << 7 - (i & 7));
                }
            }
        }

        private int DecodeWhiteCodeWord()
        {
            int num = -1;
            int num2 = 0;
            bool flag = true;
            while (flag)
            {
                int num3 = NextNBits(10);
                int num4 = white[num3];
                int num5 = num4 & 1;
                int num6 = (num4 >> 1) & 0xF;
                switch (num6)
                {
                    case 12:
                        {
                            int num7 = NextLesserThan8Bits(2);
                            num3 = ((num3 << 2) & 0xC) | num7;
                            num4 = additionalMakeup[num3];
                            num6 = (num4 >> 1) & 7;
                            num = (num4 >> 4) & 0xFFF;
                            num2 += num;
                            UpdatePointer(4 - num6);
                            break;
                        }
                    case 0:
                        throw new InvalidImageException(MessageLocalization.GetComposedMessage("invalid.code.encountered"));
                    case 15:
                        if (num2 == 0)
                        {
                            flag = false;
                            break;
                        }

                        throw new Exception(MessageLocalization.GetComposedMessage("eol.code.word.encountered.in.white.run"));
                    default:
                        num = (num4 >> 5) & 0x7FF;
                        num2 += num;
                        UpdatePointer(10 - num6);
                        if (num5 == 0)
                        {
                            flag = false;
                        }

                        break;
                }
            }

            return num2;
        }

        private int DecodeBlackCodeWord()
        {
            int num = -1;
            int num2 = 0;
            bool flag = false;
            while (!flag)
            {
                int num3 = NextLesserThan8Bits(4);
                short num4 = initBlack[num3];
                int num5 = num4 & 1;
                int num6 = (num4 >> 1) & 0xF;
                num = (num4 >> 5) & 0x7FF;
                switch (num)
                {
                    case 100:
                        {
                            num3 = NextNBits(9);
                            short num8 = black[num3];
                            num5 = num8 & 1;
                            num6 = (num8 >> 1) & 0xF;
                            num = (num8 >> 5) & 0x7FF;
                            switch (num6)
                            {
                                case 12:
                                    {
                                        UpdatePointer(5);
                                        num3 = NextLesserThan8Bits(4);
                                        short num9 = additionalMakeup[num3];
                                        num6 = (num9 >> 1) & 7;
                                        num = (num9 >> 4) & 0xFFF;
                                        num2 += num;
                                        UpdatePointer(4 - num6);
                                        break;
                                    }
                                case 15:
                                    throw new Exception(MessageLocalization.GetComposedMessage("eol.code.word.encountered.in.black.run"));
                                default:
                                    num2 += num;
                                    UpdatePointer(9 - num6);
                                    if (num5 == 0)
                                    {
                                        flag = true;
                                    }

                                    break;
                            }

                            break;
                        }
                    case 200:
                        {
                            num3 = NextLesserThan8Bits(2);
                            short num7 = twoBitBlack[num3];
                            num = (num7 >> 5) & 0x7FF;
                            num2 += num;
                            num6 = (num7 >> 1) & 0xF;
                            UpdatePointer(2 - num6);
                            flag = true;
                            break;
                        }
                    default:
                        num2 += num;
                        UpdatePointer(4 - num6);
                        flag = true;
                        break;
                }
            }

            return num2;
        }

        private int ReadEOL(bool isFirstEOL)
        {
            if (fillBits == 0)
            {
                int num = NextNBits(12);
                if (isFirstEOL && num == 0 && NextNBits(4) == 1)
                {
                    fillBits = 1;
                    return 1;
                }

                if (num != 1)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("scanline.must.begin.with.eol.code.word"));
                }
            }
            else if (fillBits == 1)
            {
                int num2 = 8 - bitPointer;
                if (NextNBits(num2) != 0)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("all.fill.bits.preceding.eol.code.must.be.0"));
                }

                if (num2 < 4 && NextNBits(8) != 0)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("all.fill.bits.preceding.eol.code.must.be.0"));
                }

                int num3;
                while ((num3 = NextNBits(8)) != 1)
                {
                    if (num3 != 0)
                    {
                        throw new Exception(MessageLocalization.GetComposedMessage("all.fill.bits.preceding.eol.code.must.be.0"));
                    }
                }
            }

            if (oneD == 0)
            {
                return 1;
            }

            return NextLesserThan8Bits(1);
        }

        private void GetNextChangingElement(int a0, bool isWhite, int[] ret)
        {
            int[] array = prevChangingElems;
            int num = changingElemSize;
            int num2 = ((lastChangingElement > 0) ? (lastChangingElement - 1) : 0);
            num2 = ((!isWhite) ? (num2 | 1) : (num2 & -2));
            int i;
            for (i = num2; i < num; i += 2)
            {
                int num3 = array[i];
                if (num3 > a0)
                {
                    lastChangingElement = i;
                    ret[0] = num3;
                    break;
                }
            }

            if (i + 1 < num)
            {
                ret[1] = array[i + 1];
            }
        }

        private int NextNBits(int bitsToGet)
        {
            int num = data.Length - 1;
            int num2 = bytePointer;
            byte b;
            byte b2;
            byte b3;
            if (fillOrder == 1)
            {
                b = data[num2];
                if (num2 == num)
                {
                    b2 = 0;
                    b3 = 0;
                }
                else if (num2 + 1 == num)
                {
                    b2 = data[num2 + 1];
                    b3 = 0;
                }
                else
                {
                    b2 = data[num2 + 1];
                    b3 = data[num2 + 2];
                }
            }
            else
            {
                if (fillOrder != 2)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("tiff.fill.order.tag.must.be.either.1.or.2"));
                }

                b = flipTable[data[num2] & 0xFF];
                if (num2 == num)
                {
                    b2 = 0;
                    b3 = 0;
                }
                else if (num2 + 1 == num)
                {
                    b2 = flipTable[data[num2 + 1] & 0xFF];
                    b3 = 0;
                }
                else
                {
                    b2 = flipTable[data[num2 + 1] & 0xFF];
                    b3 = flipTable[data[num2 + 2] & 0xFF];
                }
            }

            int num3 = 8 - bitPointer;
            int num4 = bitsToGet - num3;
            int num5 = 0;
            if (num4 > 8)
            {
                num5 = num4 - 8;
                num4 = 8;
            }

            bytePointer++;
            int num6 = (b & table1[num3]) << bitsToGet - num3;
            int num7 = (b2 & table2[num4]) >> 8 - num4;
            int num8 = 0;
            if (num5 != 0)
            {
                num7 <<= num5;
                num8 = (b3 & table2[num5]) >> 8 - num5;
                num7 |= num8;
                bytePointer++;
                bitPointer = num5;
            }
            else if (num4 == 8)
            {
                bitPointer = 0;
                bytePointer++;
            }
            else
            {
                bitPointer = num4;
            }

            return num6 | num7;
        }

        private int NextLesserThan8Bits(int bitsToGet)
        {
            byte b = 0;
            byte b2 = 0;
            int num = data.Length - 1;
            int num2 = bytePointer;
            if (fillOrder == 1)
            {
                b = data[num2];
                b2 = (byte)((num2 != num) ? data[num2 + 1] : 0);
            }
            else
            {
                if (fillOrder != 2)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("tiff.fill.order.tag.must.be.either.1.or.2"));
                }

                if (!recoverFromImageError || num2 < data.Length)
                {
                    b = flipTable[data[num2] & 0xFF];
                    b2 = (byte)((num2 != num) ? flipTable[data[num2 + 1] & 0xFF] : 0);
                }
            }

            int num3 = 8 - bitPointer;
            int num4 = bitsToGet - num3;
            int num5 = num3 - bitsToGet;
            int result;
            if (num5 >= 0)
            {
                result = (b & table1[num3]) >> num5;
                bitPointer += bitsToGet;
                if (bitPointer == 8)
                {
                    bitPointer = 0;
                    bytePointer++;
                }
            }
            else
            {
                result = (b & table1[num3]) << -num5;
                int num6 = (b2 & table2[num4]) >> 8 - num4;
                result |= num6;
                bytePointer++;
                bitPointer = num4;
            }

            return result;
        }

        private void UpdatePointer(int bitsToMoveBack)
        {
            int num = bitPointer - bitsToMoveBack;
            if (num < 0)
            {
                bytePointer--;
                bitPointer = 8 + num;
            }
            else
            {
                bitPointer = num;
            }
        }

        private bool AdvancePointer()
        {
            if (bitPointer != 0)
            {
                bytePointer++;
                bitPointer = 0;
            }

            return true;
        }
    }
}
