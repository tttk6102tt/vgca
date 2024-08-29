using Sign.Org.BouncyCastle.Crypto.Modes;
using Sign.Org.BouncyCastle.Crypto.Paddings;

namespace Sign.Org.BouncyCastle.Crypto.Macs
{
    public class CMac : IMac
    {
        private const byte CONSTANT_128 = 135;

        private const byte CONSTANT_64 = 27;

        private byte[] ZEROES;

        private byte[] mac;

        private byte[] buf;

        private int bufOff;

        private IBlockCipher cipher;

        private int macSize;

        private byte[] L;

        private byte[] Lu;

        private byte[] Lu2;

        public string AlgorithmName => cipher.AlgorithmName;

        public CMac(IBlockCipher cipher)
            : this(cipher, cipher.GetBlockSize() * 8)
        {
        }

        public CMac(IBlockCipher cipher, int macSizeInBits)
        {
            if (macSizeInBits % 8 != 0)
            {
                throw new ArgumentException("MAC size must be multiple of 8");
            }

            if (macSizeInBits > cipher.GetBlockSize() * 8)
            {
                throw new ArgumentException("MAC size must be less or equal to " + cipher.GetBlockSize() * 8);
            }

            if (cipher.GetBlockSize() != 8 && cipher.GetBlockSize() != 16)
            {
                throw new ArgumentException("Block size must be either 64 or 128 bits");
            }

            this.cipher = new CbcBlockCipher(cipher);
            macSize = macSizeInBits / 8;
            mac = new byte[cipher.GetBlockSize()];
            buf = new byte[cipher.GetBlockSize()];
            ZEROES = new byte[cipher.GetBlockSize()];
            bufOff = 0;
        }

        private static byte[] doubleLu(byte[] inBytes)
        {
            int num = (inBytes[0] & 0xFF) >> 7;
            byte[] array = new byte[inBytes.Length];
            for (int i = 0; i < inBytes.Length - 1; i++)
            {
                array[i] = (byte)((inBytes[i] << 1) + ((inBytes[i + 1] & 0xFF) >> 7));
            }

            array[inBytes.Length - 1] = (byte)(inBytes[^1] << 1);
            if (num == 1)
            {
                array[inBytes.Length - 1] ^= (byte)((inBytes.Length == 16) ? 135 : 27);
            }

            return array;
        }

        public void Init(ICipherParameters parameters)
        {
            if (parameters != null)
            {
                cipher.Init(forEncryption: true, parameters);
                L = new byte[ZEROES.Length];
                cipher.ProcessBlock(ZEROES, 0, L, 0);
                Lu = doubleLu(L);
                Lu2 = doubleLu(Lu);
            }

            Reset();
        }

        public int GetMacSize()
        {
            return macSize;
        }

        public void Update(byte input)
        {
            if (bufOff == buf.Length)
            {
                cipher.ProcessBlock(buf, 0, mac, 0);
                bufOff = 0;
            }

            buf[bufOff++] = input;
        }

        public void BlockUpdate(byte[] inBytes, int inOff, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException("Can't have a negative input length!");
            }

            int blockSize = cipher.GetBlockSize();
            int num = blockSize - bufOff;
            if (len > num)
            {
                Array.Copy(inBytes, inOff, buf, bufOff, num);
                cipher.ProcessBlock(buf, 0, mac, 0);
                bufOff = 0;
                len -= num;
                inOff += num;
                while (len > blockSize)
                {
                    cipher.ProcessBlock(inBytes, inOff, mac, 0);
                    len -= blockSize;
                    inOff += blockSize;
                }
            }

            Array.Copy(inBytes, inOff, buf, bufOff, len);
            bufOff += len;
        }

        public int DoFinal(byte[] outBytes, int outOff)
        {
            int blockSize = cipher.GetBlockSize();
            byte[] array;
            if (bufOff == blockSize)
            {
                array = Lu;
            }
            else
            {
                new ISO7816d4Padding().AddPadding(buf, bufOff);
                array = Lu2;
            }

            for (int i = 0; i < mac.Length; i++)
            {
                buf[i] ^= array[i];
            }

            cipher.ProcessBlock(buf, 0, mac, 0);
            Array.Copy(mac, 0, outBytes, outOff, macSize);
            Reset();
            return macSize;
        }

        public void Reset()
        {
            Array.Clear(buf, 0, buf.Length);
            bufOff = 0;
            cipher.Reset();
        }
    }
}
