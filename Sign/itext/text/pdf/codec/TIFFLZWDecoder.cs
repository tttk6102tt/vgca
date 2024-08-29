using Sign.itext.error_messages;

namespace Sign.itext.text.pdf.codec
{
    public class TIFFLZWDecoder
    {
        private byte[][] stringTable;

        private byte[] data;

        private byte[] uncompData;

        private int tableIndex;

        private int bitsToGet = 9;

        private int bytePointer;

        private int dstIndex;

        private int w;

        private int h;

        private int predictor;

        private int samplesPerPixel;

        private int nextData;

        private int nextBits;

        private int[] andTable = new int[4] { 511, 1023, 2047, 4095 };

        public TIFFLZWDecoder(int w, int predictor, int samplesPerPixel)
        {
            this.w = w;
            this.predictor = predictor;
            this.samplesPerPixel = samplesPerPixel;
        }

        public virtual byte[] Decode(byte[] data, byte[] uncompData, int h)
        {
            if (data[0] == 0 && data[1] == 1)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("tiff.5.0.style.lzw.codes.are.not.supported"));
            }

            InitializeStringTable();
            this.data = data;
            this.h = h;
            this.uncompData = uncompData;
            bytePointer = 0;
            dstIndex = 0;
            nextData = 0;
            nextBits = 0;
            int num = 0;
            int nextCode;
            while ((nextCode = GetNextCode()) != 257 && dstIndex < uncompData.Length)
            {
                if (nextCode == 256)
                {
                    InitializeStringTable();
                    nextCode = GetNextCode();
                    if (nextCode == 257)
                    {
                        break;
                    }

                    WriteString(stringTable[nextCode]);
                    num = nextCode;
                }
                else if (nextCode < tableIndex)
                {
                    byte[] array = stringTable[nextCode];
                    WriteString(array);
                    AddStringToTable(stringTable[num], array[0]);
                    num = nextCode;
                }
                else
                {
                    byte[] array = stringTable[num];
                    array = ComposeString(array, array[0]);
                    WriteString(array);
                    AddStringToTable(array);
                    num = nextCode;
                }
            }

            if (predictor == 2)
            {
                for (int i = 0; i < h; i++)
                {
                    int num2 = samplesPerPixel * (i * w + 1);
                    for (int j = samplesPerPixel; j < w * samplesPerPixel; j++)
                    {
                        uncompData[num2] += uncompData[num2 - samplesPerPixel];
                        num2++;
                    }
                }
            }

            return uncompData;
        }

        public virtual void InitializeStringTable()
        {
            stringTable = new byte[4096][];
            for (int i = 0; i < 256; i++)
            {
                stringTable[i] = new byte[1];
                stringTable[i][0] = (byte)i;
            }

            tableIndex = 258;
            bitsToGet = 9;
        }

        public virtual void WriteString(byte[] strn)
        {
            int num = uncompData.Length - dstIndex;
            if (strn.Length < num)
            {
                num = strn.Length;
            }

            Array.Copy(strn, 0, uncompData, dstIndex, num);
            dstIndex += num;
        }

        public virtual void AddStringToTable(byte[] oldString, byte newString)
        {
            int num = oldString.Length;
            byte[] array = new byte[num + 1];
            Array.Copy(oldString, 0, array, 0, num);
            array[num] = newString;
            stringTable[tableIndex++] = array;
            if (tableIndex == 511)
            {
                bitsToGet = 10;
            }
            else if (tableIndex == 1023)
            {
                bitsToGet = 11;
            }
            else if (tableIndex == 2047)
            {
                bitsToGet = 12;
            }
        }

        public virtual void AddStringToTable(byte[] strn)
        {
            stringTable[tableIndex++] = strn;
            if (tableIndex == 511)
            {
                bitsToGet = 10;
            }
            else if (tableIndex == 1023)
            {
                bitsToGet = 11;
            }
            else if (tableIndex == 2047)
            {
                bitsToGet = 12;
            }
        }

        public virtual byte[] ComposeString(byte[] oldString, byte newString)
        {
            int num = oldString.Length;
            byte[] array = new byte[num + 1];
            Array.Copy(oldString, 0, array, 0, num);
            array[num] = newString;
            return array;
        }

        public virtual int GetNextCode()
        {
            try
            {
                nextData = (nextData << 8) | (data[bytePointer++] & 0xFF);
                nextBits += 8;
                if (nextBits < bitsToGet)
                {
                    nextData = (nextData << 8) | (data[bytePointer++] & 0xFF);
                    nextBits += 8;
                }

                int result = (nextData >> nextBits - bitsToGet) & andTable[bitsToGet - 9];
                nextBits -= bitsToGet;
                return result;
            }
            catch (IndexOutOfRangeException)
            {
                return 257;
            }
        }
    }
}
