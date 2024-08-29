using Sign.Org.BouncyCastle.Crypto.Macs;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Modes
{
    public class CcmBlockCipher : IAeadBlockCipher
    {
        private static readonly int BlockSize = 16;

        private readonly IBlockCipher cipher;

        private readonly byte[] macBlock;

        private bool forEncryption;

        private byte[] nonce;

        private byte[] initialAssociatedText;

        private int macSize;

        private ICipherParameters keyParam;

        private readonly MemoryStream associatedText = new MemoryStream();

        private readonly MemoryStream data = new MemoryStream();

        public virtual string AlgorithmName => cipher.AlgorithmName + "/CCM";

        public CcmBlockCipher(IBlockCipher cipher)
        {
            this.cipher = cipher;
            macBlock = new byte[BlockSize];
            if (cipher.GetBlockSize() != BlockSize)
            {
                throw new ArgumentException("cipher required with a block size of " + BlockSize + ".");
            }
        }

        public virtual IBlockCipher GetUnderlyingCipher()
        {
            return cipher;
        }

        public virtual void Init(bool forEncryption, ICipherParameters parameters)
        {
            this.forEncryption = forEncryption;
            if (parameters is AeadParameters)
            {
                AeadParameters aeadParameters = (AeadParameters)parameters;
                nonce = aeadParameters.GetNonce();
                initialAssociatedText = aeadParameters.GetAssociatedText();
                macSize = aeadParameters.MacSize / 8;
                keyParam = aeadParameters.Key;
            }
            else
            {
                if (!(parameters is ParametersWithIV))
                {
                    throw new ArgumentException("invalid parameters passed to CCM");
                }

                ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
                nonce = parametersWithIV.GetIV();
                initialAssociatedText = null;
                macSize = macBlock.Length / 2;
                keyParam = parametersWithIV.Parameters;
            }

            if (nonce == null || nonce.Length < 7 || nonce.Length > 13)
            {
                throw new ArgumentException("nonce must have length from 7 to 13 octets");
            }
        }

        public virtual int GetBlockSize()
        {
            return cipher.GetBlockSize();
        }

        public virtual void ProcessAadByte(byte input)
        {
            associatedText.WriteByte(input);
        }

        public virtual void ProcessAadBytes(byte[] inBytes, int inOff, int len)
        {
            associatedText.Write(inBytes, inOff, len);
        }

        public virtual int ProcessByte(byte input, byte[] outBytes, int outOff)
        {
            data.WriteByte(input);
            return 0;
        }

        public virtual int ProcessBytes(byte[] inBytes, int inOff, int inLen, byte[] outBytes, int outOff)
        {
            data.Write(inBytes, inOff, inLen);
            return 0;
        }

        public virtual int DoFinal(byte[] outBytes, int outOff)
        {
            byte[] array = ProcessPacket(data.GetBuffer(), 0, (int)data.Position);
            Array.Copy(array, 0, outBytes, outOff, array.Length);
            Reset();
            return array.Length;
        }

        public virtual void Reset()
        {
            cipher.Reset();
            associatedText.SetLength(0L);
            data.SetLength(0L);
        }

        public virtual byte[] GetMac()
        {
            byte[] array = new byte[macSize];
            Array.Copy(macBlock, 0, array, 0, array.Length);
            return array;
        }

        public virtual int GetUpdateOutputSize(int len)
        {
            return 0;
        }

        public int GetOutputSize(int len)
        {
            int num = (int)data.Length + len;
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

        public byte[] ProcessPacket(byte[] input, int inOff, int inLen)
        {
            if (keyParam == null)
            {
                throw new InvalidOperationException("CCM cipher unitialized.");
            }

            int num = nonce.Length;
            int num2 = 15 - num;
            if (num2 < 4)
            {
                int num3 = 1 << 8 * num2;
                if (inLen >= num3)
                {
                    throw new InvalidOperationException("CCM packet too large for choice of q.");
                }
            }

            byte[] array = new byte[BlockSize];
            array[0] = (byte)((uint)(num2 - 1) & 7u);
            nonce.CopyTo(array, 1);
            IBlockCipher blockCipher = new SicBlockCipher(cipher);
            blockCipher.Init(forEncryption, new ParametersWithIV(keyParam, array));
            int i = inOff;
            int num4 = 0;
            byte[] array2;
            if (forEncryption)
            {
                array2 = new byte[inLen + macSize];
                calculateMac(input, inOff, inLen, macBlock);
                blockCipher.ProcessBlock(macBlock, 0, macBlock, 0);
                for (; i < inLen - BlockSize; i += BlockSize)
                {
                    blockCipher.ProcessBlock(input, i, array2, num4);
                    num4 += BlockSize;
                }

                byte[] array3 = new byte[BlockSize];
                Array.Copy(input, i, array3, 0, inLen - i);
                blockCipher.ProcessBlock(array3, 0, array3, 0);
                Array.Copy(array3, 0, array2, num4, inLen - i);
                num4 += inLen - i;
                Array.Copy(macBlock, 0, array2, num4, array2.Length - num4);
            }
            else
            {
                array2 = new byte[inLen - macSize];
                Array.Copy(input, inOff + inLen - macSize, macBlock, 0, macSize);
                blockCipher.ProcessBlock(macBlock, 0, macBlock, 0);
                for (int j = macSize; j != macBlock.Length; j++)
                {
                    macBlock[j] = 0;
                }

                while (num4 < array2.Length - BlockSize)
                {
                    blockCipher.ProcessBlock(input, i, array2, num4);
                    num4 += BlockSize;
                    i += BlockSize;
                }

                byte[] array4 = new byte[BlockSize];
                Array.Copy(input, i, array4, 0, array2.Length - num4);
                blockCipher.ProcessBlock(array4, 0, array4, 0);
                Array.Copy(array4, 0, array2, num4, array2.Length - num4);
                byte[] b = new byte[BlockSize];
                calculateMac(array2, 0, array2.Length, b);
                if (!Arrays.ConstantTimeAreEqual(macBlock, b))
                {
                    throw new InvalidCipherTextException("mac check in CCM failed");
                }
            }

            return array2;
        }

        private int calculateMac(byte[] data, int dataOff, int dataLen, byte[] macBlock)
        {
            IMac mac = new CbcBlockCipherMac(cipher, macSize * 8);
            mac.Init(keyParam);
            byte[] array = new byte[16];
            if (HasAssociatedText())
            {
                array[0] |= 64;
            }

            array[0] |= (byte)((((mac.GetMacSize() - 2) / 2) & 7) << 3);
            array[0] |= (byte)((15 - nonce.Length - 1) & 7);
            Array.Copy(nonce, 0, array, 1, nonce.Length);
            int num = dataLen;
            int num2 = 1;
            while (num > 0)
            {
                array[^num2] = (byte)((uint)num & 0xFFu);
                num >>= 8;
                num2++;
            }

            mac.BlockUpdate(array, 0, array.Length);
            if (HasAssociatedText())
            {
                int associatedTextLength = GetAssociatedTextLength();
                int num3;
                if (associatedTextLength < 65280)
                {
                    mac.Update((byte)(associatedTextLength >> 8));
                    mac.Update((byte)associatedTextLength);
                    num3 = 2;
                }
                else
                {
                    mac.Update(byte.MaxValue);
                    mac.Update(254);
                    mac.Update((byte)(associatedTextLength >> 24));
                    mac.Update((byte)(associatedTextLength >> 16));
                    mac.Update((byte)(associatedTextLength >> 8));
                    mac.Update((byte)associatedTextLength);
                    num3 = 6;
                }

                if (initialAssociatedText != null)
                {
                    mac.BlockUpdate(initialAssociatedText, 0, initialAssociatedText.Length);
                }

                if (associatedText.Position > 0)
                {
                    mac.BlockUpdate(associatedText.GetBuffer(), 0, (int)associatedText.Position);
                }

                num3 = (num3 + associatedTextLength) % 16;
                if (num3 != 0)
                {
                    for (int i = num3; i < 16; i++)
                    {
                        mac.Update(0);
                    }
                }
            }

            mac.BlockUpdate(data, dataOff, dataLen);
            return mac.DoFinal(macBlock, 0);
        }

        private int GetAssociatedTextLength()
        {
            return (int)associatedText.Length + ((initialAssociatedText != null) ? initialAssociatedText.Length : 0);
        }

        private bool HasAssociatedText()
        {
            return GetAssociatedTextLength() > 0;
        }
    }
}
