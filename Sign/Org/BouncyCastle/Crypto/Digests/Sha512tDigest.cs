﻿namespace Sign.Org.BouncyCastle.Crypto.Digests
{
    public class Sha512tDigest : LongDigest
    {
        private const ulong A5 = 11936128518282651045uL;

        private readonly int digestLength;

        private ulong H1t;

        private ulong H2t;

        private ulong H3t;

        private ulong H4t;

        private ulong H5t;

        private ulong H6t;

        private ulong H7t;

        private ulong H8t;

        public override string AlgorithmName => "SHA-512/" + digestLength * 8;

        public Sha512tDigest(int bitLength)
        {
            if (bitLength >= 512)
            {
                throw new ArgumentException("cannot be >= 512", "bitLength");
            }

            if (bitLength % 8 != 0)
            {
                throw new ArgumentException("needs to be a multiple of 8", "bitLength");
            }

            if (bitLength == 384)
            {
                throw new ArgumentException("cannot be 384 use SHA384 instead", "bitLength");
            }

            digestLength = bitLength / 8;
            tIvGenerate(digestLength * 8);
            Reset();
        }

        public Sha512tDigest(Sha512tDigest t)
            : base(t)
        {
            digestLength = t.digestLength;
            H1t = t.H1t;
            H2t = t.H2t;
            H3t = t.H3t;
            H4t = t.H4t;
            H5t = t.H5t;
            H6t = t.H6t;
            H7t = t.H7t;
            H8t = t.H8t;
        }

        public override int GetDigestSize()
        {
            return digestLength;
        }

        public override int DoFinal(byte[] output, int outOff)
        {
            Finish();
            UInt64_To_BE(H1, output, outOff, digestLength);
            UInt64_To_BE(H2, output, outOff + 8, digestLength - 8);
            UInt64_To_BE(H3, output, outOff + 16, digestLength - 16);
            UInt64_To_BE(H4, output, outOff + 24, digestLength - 24);
            UInt64_To_BE(H5, output, outOff + 32, digestLength - 32);
            UInt64_To_BE(H6, output, outOff + 40, digestLength - 40);
            UInt64_To_BE(H7, output, outOff + 48, digestLength - 48);
            UInt64_To_BE(H8, output, outOff + 56, digestLength - 56);
            Reset();
            return digestLength;
        }

        public override void Reset()
        {
            base.Reset();
            H1 = H1t;
            H2 = H2t;
            H3 = H3t;
            H4 = H4t;
            H5 = H5t;
            H6 = H6t;
            H7 = H7t;
            H8 = H8t;
        }

        private void tIvGenerate(int bitLength)
        {
            H1 = 14964410163792538797uL;
            H2 = 2216346199247487646uL;
            H3 = 11082046791023156622uL;
            H4 = 65953792586715988uL;
            H5 = 17630457682085488500uL;
            H6 = 4512832404995164602uL;
            H7 = 13413544941332994254uL;
            H8 = 18322165818757711068uL;
            Update(83);
            Update(72);
            Update(65);
            Update(45);
            Update(53);
            Update(49);
            Update(50);
            Update(47);
            if (bitLength > 100)
            {
                Update((byte)(bitLength / 100 + 48));
                bitLength %= 100;
                Update((byte)(bitLength / 10 + 48));
                bitLength %= 10;
                Update((byte)(bitLength + 48));
            }
            else if (bitLength > 10)
            {
                Update((byte)(bitLength / 10 + 48));
                bitLength %= 10;
                Update((byte)(bitLength + 48));
            }
            else
            {
                Update((byte)(bitLength + 48));
            }

            Finish();
            H1t = H1;
            H2t = H2;
            H3t = H3;
            H4t = H4;
            H5t = H5;
            H6t = H6;
            H7t = H7;
            H8t = H8;
        }

        private static void UInt64_To_BE(ulong n, byte[] bs, int off, int max)
        {
            if (max > 0)
            {
                UInt32_To_BE((uint)(n >> 32), bs, off, max);
                if (max > 4)
                {
                    UInt32_To_BE((uint)n, bs, off + 4, max - 4);
                }
            }
        }

        private static void UInt32_To_BE(uint n, byte[] bs, int off, int max)
        {
            int num = System.Math.Min(4, max);
            while (--num >= 0)
            {
                int num2 = 8 * (3 - num);
                bs[off + num] = (byte)(n >> num2);
            }
        }
    }
}
