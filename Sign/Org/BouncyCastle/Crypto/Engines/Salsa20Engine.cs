using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Crypto.Utilities;
using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Engines
{
    public class Salsa20Engine : IStreamCipher
    {
        private const int StateSize = 16;

        private static readonly byte[] sigma = Strings.ToAsciiByteArray("expand 32-byte k");

        private static readonly byte[] tau = Strings.ToAsciiByteArray("expand 16-byte k");

        private int index;

        private uint[] engineState = new uint[16];

        private uint[] x = new uint[16];

        private byte[] keyStream = new byte[64];

        private byte[] workingKey;

        private byte[] workingIV;

        private bool initialised;

        private uint cW0;

        private uint cW1;

        private uint cW2;

        public string AlgorithmName => "Salsa20";

        public void Init(bool forEncryption, ICipherParameters parameters)
        {
            ParametersWithIV obj = (parameters as ParametersWithIV) ?? throw new ArgumentException("Salsa20 Init requires an IV", "parameters");
            byte[] iV = obj.GetIV();
            if (iV == null || iV.Length != 8)
            {
                throw new ArgumentException("Salsa20 requires exactly 8 bytes of IV");
            }

            KeyParameter keyParameter = obj.Parameters as KeyParameter;
            if (keyParameter == null)
            {
                throw new ArgumentException("Salsa20 Init requires a key", "parameters");
            }

            workingKey = keyParameter.GetKey();
            workingIV = iV;
            SetKey(workingKey, workingIV);
        }

        public byte ReturnByte(byte input)
        {
            if (LimitExceeded())
            {
                throw new MaxBytesExceededException("2^70 byte limit per IV; Change IV");
            }

            if (index == 0)
            {
                GenerateKeyStream(keyStream);
                if (++engineState[8] == 0)
                {
                    engineState[9]++;
                }
            }

            byte result = (byte)(keyStream[index] ^ input);
            index = (index + 1) & 0x3F;
            return result;
        }

        public void ProcessBytes(byte[] inBytes, int inOff, int len, byte[] outBytes, int outOff)
        {
            if (!initialised)
            {
                throw new InvalidOperationException(AlgorithmName + " not initialised");
            }

            if (inOff + len > inBytes.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            if (outOff + len > outBytes.Length)
            {
                throw new DataLengthException("output buffer too short");
            }

            if (LimitExceeded((uint)len))
            {
                throw new MaxBytesExceededException("2^70 byte limit per IV would be exceeded; Change IV");
            }

            for (int i = 0; i < len; i++)
            {
                if (index == 0)
                {
                    GenerateKeyStream(keyStream);
                    if (++engineState[8] == 0)
                    {
                        engineState[9]++;
                    }
                }

                outBytes[i + outOff] = (byte)(keyStream[index] ^ inBytes[i + inOff]);
                index = (index + 1) & 0x3F;
            }
        }

        public void Reset()
        {
            SetKey(workingKey, workingIV);
        }

        private void SetKey(byte[] keyBytes, byte[] ivBytes)
        {
            workingKey = keyBytes;
            workingIV = ivBytes;
            index = 0;
            ResetCounter();
            int num = 0;
            engineState[1] = Pack.LE_To_UInt32(workingKey, 0);
            engineState[2] = Pack.LE_To_UInt32(workingKey, 4);
            engineState[3] = Pack.LE_To_UInt32(workingKey, 8);
            engineState[4] = Pack.LE_To_UInt32(workingKey, 12);
            byte[] bs;
            if (workingKey.Length == 32)
            {
                bs = sigma;
                num = 16;
            }
            else
            {
                bs = tau;
            }

            engineState[11] = Pack.LE_To_UInt32(workingKey, num);
            engineState[12] = Pack.LE_To_UInt32(workingKey, num + 4);
            engineState[13] = Pack.LE_To_UInt32(workingKey, num + 8);
            engineState[14] = Pack.LE_To_UInt32(workingKey, num + 12);
            engineState[0] = Pack.LE_To_UInt32(bs, 0);
            engineState[5] = Pack.LE_To_UInt32(bs, 4);
            engineState[10] = Pack.LE_To_UInt32(bs, 8);
            engineState[15] = Pack.LE_To_UInt32(bs, 12);
            engineState[6] = Pack.LE_To_UInt32(workingIV, 0);
            engineState[7] = Pack.LE_To_UInt32(workingIV, 4);
            engineState[8] = (engineState[9] = 0u);
            initialised = true;
        }

        private void GenerateKeyStream(byte[] output)
        {
            SalsaCore(20, engineState, x);
            Pack.UInt32_To_LE(x, output, 0);
        }

        internal static void SalsaCore(int rounds, uint[] state, uint[] x)
        {
            Array.Copy(state, 0, x, 0, state.Length);
            for (int num = rounds; num > 0; num -= 2)
            {
                x[4] ^= R(x[0] + x[12], 7);
                x[8] ^= R(x[4] + x[0], 9);
                x[12] ^= R(x[8] + x[4], 13);
                x[0] ^= R(x[12] + x[8], 18);
                x[9] ^= R(x[5] + x[1], 7);
                x[13] ^= R(x[9] + x[5], 9);
                x[1] ^= R(x[13] + x[9], 13);
                x[5] ^= R(x[1] + x[13], 18);
                x[14] ^= R(x[10] + x[6], 7);
                x[2] ^= R(x[14] + x[10], 9);
                x[6] ^= R(x[2] + x[14], 13);
                x[10] ^= R(x[6] + x[2], 18);
                x[3] ^= R(x[15] + x[11], 7);
                x[7] ^= R(x[3] + x[15], 9);
                x[11] ^= R(x[7] + x[3], 13);
                x[15] ^= R(x[11] + x[7], 18);
                x[1] ^= R(x[0] + x[3], 7);
                x[2] ^= R(x[1] + x[0], 9);
                x[3] ^= R(x[2] + x[1], 13);
                x[0] ^= R(x[3] + x[2], 18);
                x[6] ^= R(x[5] + x[4], 7);
                x[7] ^= R(x[6] + x[5], 9);
                x[4] ^= R(x[7] + x[6], 13);
                x[5] ^= R(x[4] + x[7], 18);
                x[11] ^= R(x[10] + x[9], 7);
                x[8] ^= R(x[11] + x[10], 9);
                x[9] ^= R(x[8] + x[11], 13);
                x[10] ^= R(x[9] + x[8], 18);
                x[12] ^= R(x[15] + x[14], 7);
                x[13] ^= R(x[12] + x[15], 9);
                x[14] ^= R(x[13] + x[12], 13);
                x[15] ^= R(x[14] + x[13], 18);
            }

            for (int i = 0; i < 16; i++)
            {
                x[i] += state[i];
            }
        }

        private static uint R(uint x, int y)
        {
            return (x << y) | (x >> 32 - y);
        }

        private void ResetCounter()
        {
            cW0 = 0u;
            cW1 = 0u;
            cW2 = 0u;
        }

        private bool LimitExceeded()
        {
            if (++cW0 == 0 && ++cW1 == 0)
            {
                return (++cW2 & 0x20) != 0;
            }

            return false;
        }

        private bool LimitExceeded(uint len)
        {
            uint num = cW0;
            cW0 += len;
            if (cW0 < num && ++cW1 == 0)
            {
                return (++cW2 & 0x20) != 0;
            }

            return false;
        }
    }
}
