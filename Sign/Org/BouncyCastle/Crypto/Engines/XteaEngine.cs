﻿using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Crypto.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Engines
{
    public class XteaEngine : IBlockCipher
    {
        private const int rounds = 32;

        private const int block_size = 8;

        private const int delta = -1640531527;

        private uint[] _S = new uint[4];

        private uint[] _sum0 = new uint[32];

        private uint[] _sum1 = new uint[32];

        private bool _initialised;

        private bool _forEncryption;

        public string AlgorithmName => "XTEA";

        public bool IsPartialBlockOkay => false;

        public XteaEngine()
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
            int num;
            int num2 = (num = 0);
            while (num2 < 4)
            {
                _S[num2] = Pack.BE_To_UInt32(key, num);
                num2++;
                num += 4;
            }

            for (num2 = (num = 0); num2 < 32; num2++)
            {
                _sum0[num2] = (uint)num + _S[num & 3];
                num += -1640531527;
                _sum1[num2] = (uint)num + _S[(num >> 11) & 3];
            }
        }

        private int encryptBlock(byte[] inBytes, int inOff, byte[] outBytes, int outOff)
        {
            uint num = Pack.BE_To_UInt32(inBytes, inOff);
            uint num2 = Pack.BE_To_UInt32(inBytes, inOff + 4);
            for (int i = 0; i < 32; i++)
            {
                num += (((num2 << 4) ^ (num2 >> 5)) + num2) ^ _sum0[i];
                num2 += (((num << 4) ^ (num >> 5)) + num) ^ _sum1[i];
            }

            Pack.UInt32_To_BE(num, outBytes, outOff);
            Pack.UInt32_To_BE(num2, outBytes, outOff + 4);
            return 8;
        }

        private int decryptBlock(byte[] inBytes, int inOff, byte[] outBytes, int outOff)
        {
            uint num = Pack.BE_To_UInt32(inBytes, inOff);
            uint num2 = Pack.BE_To_UInt32(inBytes, inOff + 4);
            for (int num3 = 31; num3 >= 0; num3--)
            {
                num2 -= (((num << 4) ^ (num >> 5)) + num) ^ _sum1[num3];
                num -= (((num2 << 4) ^ (num2 >> 5)) + num2) ^ _sum0[num3];
            }

            Pack.UInt32_To_BE(num, outBytes, outOff);
            Pack.UInt32_To_BE(num2, outBytes, outOff + 4);
            return 8;
        }
    }
}
