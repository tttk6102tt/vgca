using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Crypto.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Macs
{
    public class SipHash : IMac
    {
        protected readonly int c;

        protected readonly int d;

        protected long k0;

        protected long k1;

        protected long v0;

        protected long v1;

        protected long v2;

        protected long v3;

        protected long v4;

        protected byte[] buf = new byte[8];

        protected int bufPos;

        protected int wordCount;

        public virtual string AlgorithmName => "SipHash-" + c + "-" + d;

        public SipHash()
            : this(2, 4)
        {
        }

        public SipHash(int c, int d)
        {
            this.c = c;
            this.d = d;
        }

        public virtual int GetMacSize()
        {
            return 8;
        }

        public virtual void Init(ICipherParameters parameters)
        {
            byte[] key = ((parameters as KeyParameter) ?? throw new ArgumentException("must be an instance of KeyParameter", "parameters")).GetKey();
            if (key.Length != 16)
            {
                throw new ArgumentException("must be a 128-bit key", "parameters");
            }

            k0 = (long)Pack.LE_To_UInt64(key, 0);
            k1 = (long)Pack.LE_To_UInt64(key, 8);
            Reset();
        }

        public virtual void Update(byte input)
        {
            buf[bufPos] = input;
            if (++bufPos == buf.Length)
            {
                ProcessMessageWord();
                bufPos = 0;
            }
        }

        public virtual void BlockUpdate(byte[] input, int offset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                buf[bufPos] = input[offset + i];
                if (++bufPos == buf.Length)
                {
                    ProcessMessageWord();
                    bufPos = 0;
                }
            }
        }

        public virtual long DoFinal()
        {
            buf[7] = (byte)((wordCount << 3) + bufPos);
            while (bufPos < 7)
            {
                buf[bufPos++] = 0;
            }

            ProcessMessageWord();
            v2 ^= 255L;
            ApplySipRounds(d);
            long result = v0 ^ v1 ^ v2 ^ v3;
            Reset();
            return result;
        }

        public virtual int DoFinal(byte[] output, int outOff)
        {
            Pack.UInt64_To_LE((ulong)DoFinal(), output, outOff);
            return 8;
        }

        public virtual void Reset()
        {
            v0 = k0 ^ 0x736F6D6570736575L;
            v1 = k1 ^ 0x646F72616E646F6DL;
            v2 = k0 ^ 0x6C7967656E657261L;
            v3 = k1 ^ 0x7465646279746573L;
            Array.Clear(buf, 0, buf.Length);
            bufPos = 0;
            wordCount = 0;
        }

        protected virtual void ProcessMessageWord()
        {
            wordCount++;
            long num = (long)Pack.LE_To_UInt64(buf, 0);
            v3 ^= num;
            ApplySipRounds(c);
            v0 ^= num;
        }

        protected virtual void ApplySipRounds(int n)
        {
            for (int i = 0; i < n; i++)
            {
                v0 += v1;
                v2 += v3;
                v1 = RotateLeft(v1, 13);
                v3 = RotateLeft(v3, 16);
                v1 ^= v0;
                v3 ^= v2;
                v0 = RotateLeft(v0, 32);
                v2 += v1;
                v0 += v3;
                v1 = RotateLeft(v1, 17);
                v3 = RotateLeft(v3, 21);
                v1 ^= v2;
                v3 ^= v0;
                v2 = RotateLeft(v2, 32);
            }
        }

        protected static long RotateLeft(long x, int n)
        {
            return (x << n) | (long)((ulong)x >> 64 - n);
        }
    }
}
