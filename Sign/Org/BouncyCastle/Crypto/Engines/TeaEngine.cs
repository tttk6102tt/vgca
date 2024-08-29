using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Crypto.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Engines
{
    public class TeaEngine : IBlockCipher
    {
        private const int rounds = 32;

        private const int block_size = 8;

        private const uint delta = 2654435769u;

        private const uint d_sum = 3337565984u;

        private uint _a;

        private uint _b;

        private uint _c;

        private uint _d;

        private bool _initialised;

        private bool _forEncryption;

        public string AlgorithmName => "TEA";

        public bool IsPartialBlockOkay => false;

        public TeaEngine()
        {
            _initialised = false;
        }

        public int GetBlockSize()
        {
            return 8;
        }

        public void Init(bool forEncryption, ICipherParameters parameters)
        {
            if (!(parameters is KeyParameter))
            {
                throw new ArgumentException("invalid parameter passed to TEA init - " + parameters.GetType().FullName);
            }

            _forEncryption = forEncryption;
            _initialised = true;
            KeyParameter keyParameter = (KeyParameter)parameters;
            setKey(keyParameter.GetKey());
        }

        public int ProcessBlock(byte[] inBytes, int inOff, byte[] outBytes, int outOff)
        {
            if (!_initialised)
            {
                throw new InvalidOperationException(AlgorithmName + " not initialised");
            }

            if (inOff + 8 > inBytes.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            if (outOff + 8 > outBytes.Length)
            {
                throw new DataLengthException("output buffer too short");
            }

            if (!_forEncryption)
            {
                return decryptBlock(inBytes, inOff, outBytes, outOff);
            }

            return encryptBlock(inBytes, inOff, outBytes, outOff);
        }

        public void Reset()
        {
        }

        private void setKey(byte[] key)
        {
            _a = Pack.BE_To_UInt32(key, 0);
            _b = Pack.BE_To_UInt32(key, 4);
            _c = Pack.BE_To_UInt32(key, 8);
            _d = Pack.BE_To_UInt32(key, 12);
        }

        private int encryptBlock(byte[] inBytes, int inOff, byte[] outBytes, int outOff)
        {
            uint num = Pack.BE_To_UInt32(inBytes, inOff);
            uint num2 = Pack.BE_To_UInt32(inBytes, inOff + 4);
            uint num3 = 0u;
            for (int i = 0; i != 32; i++)
            {
                num3 += 2654435769u;
                num += ((num2 << 4) + _a) ^ (num2 + num3) ^ ((num2 >> 5) + _b);
                num2 += ((num << 4) + _c) ^ (num + num3) ^ ((num >> 5) + _d);
            }

            Pack.UInt32_To_BE(num, outBytes, outOff);
            Pack.UInt32_To_BE(num2, outBytes, outOff + 4);
            return 8;
        }

        private int decryptBlock(byte[] inBytes, int inOff, byte[] outBytes, int outOff)
        {
            uint num = Pack.BE_To_UInt32(inBytes, inOff);
            uint num2 = Pack.BE_To_UInt32(inBytes, inOff + 4);
            uint num3 = 3337565984u;
            for (int i = 0; i != 32; i++)
            {
                num2 -= ((num << 4) + _c) ^ (num + num3) ^ ((num >> 5) + _d);
                num -= ((num2 << 4) + _a) ^ (num2 + num3) ^ ((num2 >> 5) + _b);
                num3 -= 2654435769u;
            }

            Pack.UInt32_To_BE(num, outBytes, outOff);
            Pack.UInt32_To_BE(num2, outBytes, outOff + 4);
            return 8;
        }
    }
}
