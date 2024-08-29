using Sign.itext.error_messages;

namespace Sign.itext.text.pdf
{
    public class LZWDecoder
    {
        private byte[][] stringTable;

        private byte[] data;

        private Stream uncompData;

        private int tableIndex;

        private int bitsToGet = 9;

        private int bytePointer;

        private int nextData;

        private int nextBits;

        internal int[] andTable = new int[4] { 511, 1023, 2047, 4095 };

        public virtual int NextCode
        {
            get
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
                catch
                {
                    return 257;
                }
            }
        }

        public virtual void Decode(byte[] data, Stream uncompData)
        {
            if (data[0] == 0 && data[1] == 1)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("lzw.flavour.not.supported"));
            }

            InitializeStringTable();
            this.data = data;
            this.uncompData = uncompData;
            bytePointer = 0;
            nextData = 0;
            nextBits = 0;
            int num = 0;
            int nextCode;
            while ((nextCode = NextCode) != 257)
            {
                if (nextCode == 256)
                {
                    InitializeStringTable();
                    nextCode = NextCode;
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
        }

        public virtual void InitializeStringTable()
        {
            stringTable = new byte[8192][];
            for (int i = 0; i < 256; i++)
            {
                stringTable[i] = new byte[1];
                stringTable[i][0] = (byte)i;
            }

            tableIndex = 258;
            bitsToGet = 9;
        }

        public virtual void WriteString(byte[] str)
        {
            uncompData.Write(str, 0, str.Length);
        }

        public virtual void AddStringToTable(byte[] oldstring, byte newstring)
        {
            int num = oldstring.Length;
            byte[] array = new byte[num + 1];
            Array.Copy(oldstring, 0, array, 0, num);
            array[num] = newstring;
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

        public virtual void AddStringToTable(byte[] str)
        {
            stringTable[tableIndex++] = str;
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

        public virtual byte[] ComposeString(byte[] oldstring, byte newstring)
        {
            int num = oldstring.Length;
            byte[] array = new byte[num + 1];
            Array.Copy(oldstring, 0, array, 0, num);
            array[num] = newstring;
            return array;
        }
    }
}
