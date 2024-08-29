namespace Sign.Org.BouncyCastle.Asn1.Utilities.Encoders
{
    public class HexEncoder : IEncoder
    {
        private static readonly byte[] encodingTable = new byte[16]
        {
            48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
            97, 98, 99, 100, 101, 102
        };

        private static readonly byte[] decodingTable = ConstructDecodingTable(encodingTable);

        private static byte[] ConstructDecodingTable(byte[] et)
        {
            byte[] array = new byte[128];
            for (int i = 0; i < et.Length; i++)
            {
                array[et[i]] = (byte)i;
            }

            array[65] = array[97];
            array[66] = array[98];
            array[67] = array[99];
            array[68] = array[100];
            array[69] = array[101];
            array[70] = array[102];
            return array;
        }

        public int Encode(byte[] data, int off, int length, Stream outStream)
        {
            for (int i = off; i < off + length; i++)
            {
                int num = data[i];
                outStream.WriteByte(encodingTable[num >> 4]);
                outStream.WriteByte(encodingTable[num & 0xF]);
            }

            return length * 2;
        }

        private static bool Ignore(char c)
        {
            if (c != '\n' && c != '\r' && c != '\t')
            {
                return c == ' ';
            }

            return true;
        }

        public int Decode(byte[] data, int off, int length, Stream outStream)
        {
            int num = 0;
            int num2 = off + length;
            while (num2 > off && Ignore((char)data[num2 - 1]))
            {
                num2--;
            }

            int i = off;
            while (i < num2)
            {
                for (; i < num2 && Ignore((char)data[i]); i++)
                {
                }

                byte b = decodingTable[data[i++]];
                for (; i < num2 && Ignore((char)data[i]); i++)
                {
                }

                byte b2 = decodingTable[data[i++]];
                outStream.WriteByte((byte)((b << 4) | b2));
                num++;
            }

            return num;
        }

        public int DecodeString(string data, Stream outStream)
        {
            int num = 0;
            int num2 = data.Length;
            while (num2 > 0 && Ignore(data[num2 - 1]))
            {
                num2--;
            }

            int i = 0;
            while (i < num2)
            {
                for (; i < num2 && Ignore(data[i]); i++)
                {
                }

                byte b = decodingTable[(uint)data[i++]];
                for (; i < num2 && Ignore(data[i]); i++)
                {
                }

                byte b2 = decodingTable[(uint)data[i++]];
                outStream.WriteByte((byte)((b << 4) | b2));
                num++;
            }

            return num;
        }
    }
}
