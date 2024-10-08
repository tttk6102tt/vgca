﻿using Sign.Org.BouncyCastle.Crypto.Utilities;
using Sign.Org.BouncyCastle.Utilities;

namespace Sign.Org.BouncyCastle.Crypto.Modes.Gcm
{
    internal abstract class GcmUtilities
    {
        internal static byte[] OneAsBytes()
        {
            return new byte[16]
            {
                128, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0
            };
        }

        internal static uint[] OneAsUints()
        {
            return new uint[4] { 2147483648u, 0u, 0u, 0u };
        }

        internal static uint[] AsUints(byte[] bs)
        {
            uint[] array = new uint[4];
            Pack.BE_To_UInt32(bs, 0, array);
            return array;
        }

        internal static void AsUints(byte[] bs, uint[] output)
        {
            Pack.BE_To_UInt32(bs, 0, output);
        }

        internal static void Multiply(byte[] block, byte[] val)
        {
            byte[] array = Arrays.Clone(block);
            byte[] array2 = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                byte b = val[i];
                for (int num = 7; num >= 0; num--)
                {
                    if ((b & (1 << num)) != 0)
                    {
                        Xor(array2, array);
                    }

                    bool num2 = (array[15] & 1) != 0;
                    ShiftRight(array);
                    if (num2)
                    {
                        array[0] ^= 225;
                    }
                }
            }

            Array.Copy(array2, 0, block, 0, 16);
        }

        internal static void MultiplyP(uint[] x)
        {
            bool num = (x[3] & 1) != 0;
            ShiftRight(x);
            if (num)
            {
                x[0] ^= 3774873600u;
            }
        }

        internal static void MultiplyP(uint[] x, uint[] output)
        {
            bool num = (x[3] & 1) != 0;
            ShiftRight(x, output);
            if (num)
            {
                output[0] ^= 3774873600u;
            }
        }

        internal static void MultiplyP8(uint[] x)
        {
            uint num = x[3];
            ShiftRightN(x, 8);
            for (int num2 = 7; num2 >= 0; num2--)
            {
                if ((num & (1 << num2)) != 0L)
                {
                    x[0] ^= 3774873600u >> 7 - num2;
                }
            }
        }

        internal static void MultiplyP8(uint[] x, uint[] output)
        {
            uint num = x[3];
            ShiftRightN(x, 8, output);
            for (int num2 = 7; num2 >= 0; num2--)
            {
                if ((num & (1 << num2)) != 0L)
                {
                    output[0] ^= 3774873600u >> 7 - num2;
                }
            }
        }

        internal static void ShiftRight(byte[] block)
        {
            int num = 0;
            byte b = 0;
            while (true)
            {
                byte b2 = block[num];
                block[num] = (byte)((b2 >> 1) | b);
                if (++num != 16)
                {
                    b = (byte)(b2 << 7);
                    continue;
                }

                break;
            }
        }

        private static void ShiftRight(byte[] block, byte[] output)
        {
            int num = 0;
            byte b = 0;
            while (true)
            {
                byte b2 = block[num];
                output[num] = (byte)((b2 >> 1) | b);
                if (++num != 16)
                {
                    b = (byte)(b2 << 7);
                    continue;
                }

                break;
            }
        }

        internal static void ShiftRight(uint[] block)
        {
            int num = 0;
            uint num2 = 0u;
            while (true)
            {
                uint num3 = block[num];
                block[num] = (num3 >> 1) | num2;
                if (++num != 4)
                {
                    num2 = num3 << 31;
                    continue;
                }

                break;
            }
        }

        internal static void ShiftRight(uint[] block, uint[] output)
        {
            int num = 0;
            uint num2 = 0u;
            while (true)
            {
                uint num3 = block[num];
                output[num] = (num3 >> 1) | num2;
                if (++num != 4)
                {
                    num2 = num3 << 31;
                    continue;
                }

                break;
            }
        }

        internal static void ShiftRightN(uint[] block, int n)
        {
            int num = 0;
            uint num2 = 0u;
            while (true)
            {
                uint num3 = block[num];
                block[num] = (num3 >> n) | num2;
                if (++num != 4)
                {
                    num2 = num3 << 32 - n;
                    continue;
                }

                break;
            }
        }

        internal static void ShiftRightN(uint[] block, int n, uint[] output)
        {
            int num = 0;
            uint num2 = 0u;
            while (true)
            {
                uint num3 = block[num];
                output[num] = (num3 >> n) | num2;
                if (++num != 4)
                {
                    num2 = num3 << 32 - n;
                    continue;
                }

                break;
            }
        }

        internal static void Xor(byte[] block, byte[] val)
        {
            for (int num = 15; num >= 0; num--)
            {
                block[num] ^= val[num];
            }
        }

        internal static void Xor(byte[] block, byte[] val, int off, int len)
        {
            while (--len >= 0)
            {
                block[len] ^= val[off + len];
            }
        }

        internal static void Xor(byte[] block, byte[] val, byte[] output)
        {
            for (int num = 15; num >= 0; num--)
            {
                output[num] = (byte)(block[num] ^ val[num]);
            }
        }

        internal static void Xor(uint[] block, uint[] val)
        {
            for (int num = 3; num >= 0; num--)
            {
                block[num] ^= val[num];
            }
        }

        internal static void Xor(uint[] block, uint[] val, uint[] output)
        {
            for (int num = 3; num >= 0; num--)
            {
                output[num] = block[num] ^ val[num];
            }
        }
    }
}
