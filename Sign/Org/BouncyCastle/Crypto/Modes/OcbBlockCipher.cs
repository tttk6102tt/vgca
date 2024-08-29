using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Crypto.Modes
{
    public class OcbBlockCipher : IAeadBlockCipher
    {
        private const int BLOCK_SIZE = 16;

        private readonly IBlockCipher hashCipher;

        private readonly IBlockCipher mainCipher;

        private bool forEncryption;

        private int macSize;

        private byte[] initialAssociatedText;

        private IList L;

        private byte[] L_Asterisk;

        private byte[] L_Dollar;

        private byte[] OffsetMAIN_0;

        private byte[] hashBlock;

        private byte[] mainBlock;

        private int hashBlockPos;

        private int mainBlockPos;

        private long hashBlockCount;

        private long mainBlockCount;

        private byte[] OffsetHASH;

        private byte[] Sum;

        private byte[] OffsetMAIN;

        private byte[] Checksum;

        private byte[] macBlock;

        public virtual string AlgorithmName => mainCipher.AlgorithmName + "/OCB";

        public OcbBlockCipher(IBlockCipher hashCipher, IBlockCipher mainCipher)
        {
            if (hashCipher == null)
            {
                throw new ArgumentNullException("hashCipher");
            }

            if (hashCipher.GetBlockSize() != 16)
            {
                throw new ArgumentException("must have a block size of " + 16, "hashCipher");
            }

            if (mainCipher == null)
            {
                throw new ArgumentNullException("mainCipher");
            }

            if (mainCipher.GetBlockSize() != 16)
            {
                throw new ArgumentException("must have a block size of " + 16, "mainCipher");
            }

            if (!hashCipher.AlgorithmName.Equals(mainCipher.AlgorithmName))
            {
                throw new ArgumentException("'hashCipher' and 'mainCipher' must be the same algorithm");
            }

            this.hashCipher = hashCipher;
            this.mainCipher = mainCipher;
        }

        public virtual IBlockCipher GetUnderlyingCipher()
        {
            return mainCipher;
        }

        public virtual void Init(bool forEncryption, ICipherParameters parameters)
        {
            this.forEncryption = forEncryption;
            macBlock = null;
            byte[] array;
            KeyParameter parameters2;
            if (parameters is AeadParameters)
            {
                AeadParameters aeadParameters = (AeadParameters)parameters;
                array = aeadParameters.GetNonce();
                initialAssociatedText = aeadParameters.GetAssociatedText();
                int num = aeadParameters.MacSize;
                if (num < 64 || num > 128 || num % 8 != 0)
                {
                    throw new ArgumentException("Invalid value for MAC size: " + num);
                }

                macSize = num / 8;
                parameters2 = aeadParameters.Key;
            }
            else
            {
                if (!(parameters is ParametersWithIV))
                {
                    throw new ArgumentException("invalid parameters passed to OCB");
                }

                ParametersWithIV obj = (ParametersWithIV)parameters;
                array = obj.GetIV();
                initialAssociatedText = null;
                macSize = 16;
                parameters2 = (KeyParameter)obj.Parameters;
            }

            hashBlock = new byte[16];
            mainBlock = new byte[forEncryption ? 16 : (16 + macSize)];
            if (array == null)
            {
                array = new byte[0];
            }

            if (array.Length > 15)
            {
                throw new ArgumentException("IV must be no more than 15 bytes");
            }

            hashCipher.Init(forEncryption: true, parameters2);
            mainCipher.Init(forEncryption, parameters2);
            L_Asterisk = new byte[16];
            hashCipher.ProcessBlock(L_Asterisk, 0, L_Asterisk, 0);
            L_Dollar = OCB_double(L_Asterisk);
            L = Platform.CreateArrayList();
            L.Add(OCB_double(L_Dollar));
            byte[] array2 = new byte[16];
            Array.Copy(array, 0, array2, array2.Length - array.Length, array.Length);
            array2[0] = (byte)(macSize << 4);
            array2[15 - array.Length] |= 1;
            int num2 = array2[15] & 0x3F;
            byte[] array3 = new byte[16];
            array2[15] &= 192;
            hashCipher.ProcessBlock(array2, 0, array3, 0);
            byte[] array4 = new byte[24];
            Array.Copy(array3, 0, array4, 0, 16);
            for (int i = 0; i < 8; i++)
            {
                array4[16 + i] = (byte)(array3[i] ^ array3[i + 1]);
            }

            OffsetMAIN_0 = new byte[16];
            int num3 = num2 % 8;
            int num4 = num2 / 8;
            if (num3 == 0)
            {
                Array.Copy(array4, num4, OffsetMAIN_0, 0, 16);
            }
            else
            {
                for (int j = 0; j < 16; j++)
                {
                    uint num5 = array4[num4];
                    uint num6 = array4[++num4];
                    OffsetMAIN_0[j] = (byte)((num5 << num3) | (num6 >> 8 - num3));
                }
            }

            hashBlockPos = 0;
            mainBlockPos = 0;
            hashBlockCount = 0L;
            mainBlockCount = 0L;
            OffsetHASH = new byte[16];
            Sum = new byte[16];
            OffsetMAIN = Arrays.Clone(OffsetMAIN_0);
            Checksum = new byte[16];
            if (initialAssociatedText != null)
            {
                ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
            }
        }

        public virtual int GetBlockSize()
        {
            return 16;
        }

        public virtual byte[] GetMac()
        {
            return Arrays.Clone(macBlock);
        }

        public virtual int GetOutputSize(int len)
        {
            int num = len + mainBlockPos;
            if (forEncryption)
            {
                return num + macSize;
            }

            if (num >= macSize)
            {
                return num - macSize;
            }

            return 0;
        }

        public virtual int GetUpdateOutputSize(int len)
        {
            int num = len + mainBlockPos;
            if (!forEncryption)
            {
                if (num < macSize)
                {
                    return 0;
                }

                num -= macSize;
            }

            return num - num % 16;
        }

        public virtual void ProcessAadByte(byte input)
        {
            hashBlock[hashBlockPos] = input;
            if (++hashBlockPos == hashBlock.Length)
            {
                ProcessHashBlock();
            }
        }

        public virtual void ProcessAadBytes(byte[] input, int off, int len)
        {
            for (int i = 0; i < len; i++)
            {
                hashBlock[hashBlockPos] = input[off + i];
                if (++hashBlockPos == hashBlock.Length)
                {
                    ProcessHashBlock();
                }
            }
        }

        public virtual int ProcessByte(byte input, byte[] output, int outOff)
        {
            mainBlock[mainBlockPos] = input;
            if (++mainBlockPos == mainBlock.Length)
            {
                ProcessMainBlock(output, outOff);
                return 16;
            }

            return 0;
        }

        public virtual int ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
        {
            int num = 0;
            for (int i = 0; i < len; i++)
            {
                mainBlock[mainBlockPos] = input[inOff + i];
                if (++mainBlockPos == mainBlock.Length)
                {
                    ProcessMainBlock(output, outOff + num);
                    num += 16;
                }
            }

            return num;
        }

        public virtual int DoFinal(byte[] output, int outOff)
        {
            byte[] array = null;
            if (!forEncryption)
            {
                if (mainBlockPos < macSize)
                {
                    throw new InvalidCipherTextException("data too short");
                }

                mainBlockPos -= macSize;
                array = new byte[macSize];
                Array.Copy(mainBlock, mainBlockPos, array, 0, macSize);
            }

            if (hashBlockPos > 0)
            {
                OCB_extend(hashBlock, hashBlockPos);
                UpdateHASH(L_Asterisk);
            }

            if (mainBlockPos > 0)
            {
                if (forEncryption)
                {
                    OCB_extend(mainBlock, mainBlockPos);
                    Xor(Checksum, mainBlock);
                }

                Xor(OffsetMAIN, L_Asterisk);
                byte[] array2 = new byte[16];
                hashCipher.ProcessBlock(OffsetMAIN, 0, array2, 0);
                Xor(mainBlock, array2);
                Array.Copy(mainBlock, 0, output, outOff, mainBlockPos);
                if (!forEncryption)
                {
                    OCB_extend(mainBlock, mainBlockPos);
                    Xor(Checksum, mainBlock);
                }
            }

            Xor(Checksum, OffsetMAIN);
            Xor(Checksum, L_Dollar);
            hashCipher.ProcessBlock(Checksum, 0, Checksum, 0);
            Xor(Checksum, Sum);
            macBlock = new byte[macSize];
            Array.Copy(Checksum, 0, macBlock, 0, macSize);
            int num = mainBlockPos;
            if (forEncryption)
            {
                Array.Copy(macBlock, 0, output, outOff + num, macSize);
                num += macSize;
            }
            else if (!Arrays.ConstantTimeAreEqual(macBlock, array))
            {
                throw new InvalidCipherTextException("mac check in OCB failed");
            }

            Reset(clearMac: false);
            return num;
        }

        public virtual void Reset()
        {
            Reset(clearMac: true);
        }

        protected virtual void Clear(byte[] bs)
        {
            if (bs != null)
            {
                Array.Clear(bs, 0, bs.Length);
            }
        }

        protected virtual byte[] GetLSub(int n)
        {
            while (n >= L.Count)
            {
                L.Add(OCB_double((byte[])L[L.Count - 1]));
            }

            return (byte[])L[n];
        }

        protected virtual void ProcessHashBlock()
        {
            UpdateHASH(GetLSub(OCB_ntz(++hashBlockCount)));
            hashBlockPos = 0;
        }

        protected virtual void ProcessMainBlock(byte[] output, int outOff)
        {
            if (forEncryption)
            {
                Xor(Checksum, mainBlock);
                mainBlockPos = 0;
            }

            Xor(OffsetMAIN, GetLSub(OCB_ntz(++mainBlockCount)));
            Xor(mainBlock, OffsetMAIN);
            mainCipher.ProcessBlock(mainBlock, 0, mainBlock, 0);
            Xor(mainBlock, OffsetMAIN);
            Array.Copy(mainBlock, 0, output, outOff, 16);
            if (!forEncryption)
            {
                Xor(Checksum, mainBlock);
                Array.Copy(mainBlock, 16, mainBlock, 0, macSize);
                mainBlockPos = macSize;
            }
        }

        protected virtual void Reset(bool clearMac)
        {
            hashCipher.Reset();
            mainCipher.Reset();
            Clear(hashBlock);
            Clear(mainBlock);
            hashBlockPos = 0;
            mainBlockPos = 0;
            hashBlockCount = 0L;
            mainBlockCount = 0L;
            Clear(OffsetHASH);
            Clear(Sum);
            Array.Copy(OffsetMAIN_0, 0, OffsetMAIN, 0, 16);
            Clear(Checksum);
            if (clearMac)
            {
                macBlock = null;
            }

            if (initialAssociatedText != null)
            {
                ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
            }
        }

        protected virtual void UpdateHASH(byte[] LSub)
        {
            Xor(OffsetHASH, LSub);
            Xor(hashBlock, OffsetHASH);
            hashCipher.ProcessBlock(hashBlock, 0, hashBlock, 0);
            Xor(Sum, hashBlock);
        }

        protected static byte[] OCB_double(byte[] block)
        {
            byte[] array = new byte[16];
            int num = ShiftLeft(block, array);
            array[15] ^= (byte)(135 >> (1 - num << 3));
            return array;
        }

        protected static void OCB_extend(byte[] block, int pos)
        {
            block[pos] = 128;
            while (++pos < 16)
            {
                block[pos] = 0;
            }
        }

        protected static int OCB_ntz(long x)
        {
            if (x == 0L)
            {
                return 64;
            }

            int num = 0;
            while ((x & 1) == 0L)
            {
                num++;
                x >>= 1;
            }

            return num;
        }

        protected static int ShiftLeft(byte[] block, byte[] output)
        {
            int num = 16;
            uint num2 = 0u;
            while (--num >= 0)
            {
                uint num3 = block[num];
                output[num] = (byte)((num3 << 1) | num2);
                num2 = (num3 >> 7) & 1u;
            }

            return (int)num2;
        }

        protected static void Xor(byte[] block, byte[] val)
        {
            for (int num = 15; num >= 0; num--)
            {
                block[num] ^= val[num];
            }
        }
    }
}
