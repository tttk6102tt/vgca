using System.Text;

namespace Sign.Org.BouncyCastle.Math.EC
{
    internal class IntArray
    {
        private int[] m_ints;

        public int BitLength
        {
            get
            {
                int usedLength = GetUsedLength();
                if (usedLength == 0)
                {
                    return 0;
                }

                int num = usedLength - 1;
                uint num2 = (uint)m_ints[num];
                int num3 = (num << 5) + 1;
                if (num2 > 65535)
                {
                    if (num2 > 16777215)
                    {
                        num3 += 24;
                        num2 >>= 24;
                    }
                    else
                    {
                        num3 += 16;
                        num2 >>= 16;
                    }
                }
                else if (num2 > 255)
                {
                    num3 += 8;
                    num2 >>= 8;
                }

                while (num2 > 1)
                {
                    num3++;
                    num2 >>= 1;
                }

                return num3;
            }
        }

        public int Length => m_ints.Length;

        public IntArray(int intLen)
        {
            m_ints = new int[intLen];
        }

        private IntArray(int[] ints)
        {
            m_ints = ints;
        }

        public IntArray(BigInteger bigInt)
            : this(bigInt, 0)
        {
        }

        public IntArray(BigInteger bigInt, int minIntLen)
        {
            if (bigInt.SignValue == -1)
            {
                throw new ArgumentException("Only positive Integers allowed", "bigint");
            }

            if (bigInt.SignValue == 0)
            {
                m_ints = new int[1];
                return;
            }

            byte[] array = bigInt.ToByteArrayUnsigned();
            int num = array.Length;
            int num2 = (num + 3) / 4;
            m_ints = new int[System.Math.Max(num2, minIntLen)];
            int num3 = num % 4;
            int num4 = 0;
            if (0 < num3)
            {
                int num5 = array[num4++];
                while (num4 < num3)
                {
                    num5 = (num5 << 8) | array[num4++];
                }

                m_ints[--num2] = num5;
            }

            while (num2 > 0)
            {
                int num6 = array[num4++];
                for (int i = 1; i < 4; i++)
                {
                    num6 = (num6 << 8) | array[num4++];
                }

                m_ints[--num2] = num6;
            }
        }

        public int GetUsedLength()
        {
            int num = m_ints.Length;
            if (num < 1)
            {
                return 0;
            }

            if (m_ints[0] != 0)
            {
                while (m_ints[--num] == 0)
                {
                }

                return num + 1;
            }

            do
            {
                if (m_ints[--num] != 0)
                {
                    return num + 1;
                }
            }
            while (num > 0);
            return 0;
        }

        private int[] resizedInts(int newLen)
        {
            int[] array = new int[newLen];
            int num = m_ints.Length;
            int length = ((num < newLen) ? num : newLen);
            Array.Copy(m_ints, 0, array, 0, length);
            return array;
        }

        public BigInteger ToBigInteger()
        {
            int usedLength = GetUsedLength();
            if (usedLength == 0)
            {
                return BigInteger.Zero;
            }

            int num = m_ints[usedLength - 1];
            byte[] array = new byte[4];
            int num2 = 0;
            bool flag = false;
            for (int num3 = 3; num3 >= 0; num3--)
            {
                byte b = (byte)((uint)num >> 8 * num3);
                if (flag || b != 0)
                {
                    flag = true;
                    array[num2++] = b;
                }
            }

            byte[] array2 = new byte[4 * (usedLength - 1) + num2];
            for (int i = 0; i < num2; i++)
            {
                array2[i] = array[i];
            }

            for (int num4 = usedLength - 2; num4 >= 0; num4--)
            {
                for (int num5 = 3; num5 >= 0; num5--)
                {
                    array2[num2++] = (byte)((uint)m_ints[num4] >> 8 * num5);
                }
            }

            return new BigInteger(1, array2);
        }

        public void ShiftLeft()
        {
            int num = GetUsedLength();
            if (num == 0)
            {
                return;
            }

            if (m_ints[num - 1] < 0)
            {
                num++;
                if (num > m_ints.Length)
                {
                    m_ints = resizedInts(m_ints.Length + 1);
                }
            }

            bool flag = false;
            for (int i = 0; i < num; i++)
            {
                bool num2 = m_ints[i] < 0;
                m_ints[i] <<= 1;
                if (flag)
                {
                    m_ints[i] |= 1;
                }

                flag = num2;
            }
        }

        public IntArray ShiftLeft(int n)
        {
            int usedLength = GetUsedLength();
            if (usedLength == 0)
            {
                return this;
            }

            if (n == 0)
            {
                return this;
            }

            if (n > 31)
            {
                throw new ArgumentException("shiftLeft() for max 31 bits , " + n + "bit shift is not possible", "n");
            }

            int[] array = new int[usedLength + 1];
            int num = 32 - n;
            array[0] = m_ints[0] << n;
            for (int i = 1; i < usedLength; i++)
            {
                array[i] = (m_ints[i] << n) | (int)((uint)m_ints[i - 1] >> num);
            }

            array[usedLength] = (int)((uint)m_ints[usedLength - 1] >> num);
            return new IntArray(array);
        }

        public void AddShifted(IntArray other, int shift)
        {
            int usedLength = other.GetUsedLength();
            int num = usedLength + shift;
            if (num > m_ints.Length)
            {
                m_ints = resizedInts(num);
            }

            for (int i = 0; i < usedLength; i++)
            {
                m_ints[i + shift] ^= other.m_ints[i];
            }
        }

        public bool TestBit(int n)
        {
            int num = n >> 5;
            int num2 = n & 0x1F;
            int num3 = 1 << num2;
            return (m_ints[num] & num3) != 0;
        }

        public void FlipBit(int n)
        {
            int num = n >> 5;
            int num2 = n & 0x1F;
            int num3 = 1 << num2;
            m_ints[num] ^= num3;
        }

        public void SetBit(int n)
        {
            int num = n >> 5;
            int num2 = n & 0x1F;
            int num3 = 1 << num2;
            m_ints[num] |= num3;
        }

        public IntArray Multiply(IntArray other, int m)
        {
            int num = m + 31 >> 5;
            if (m_ints.Length < num)
            {
                m_ints = resizedInts(num);
            }

            IntArray intArray = new IntArray(other.resizedInts(other.Length + 1));
            IntArray intArray2 = new IntArray(m + m + 31 >> 5);
            int num2 = 1;
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    if ((m_ints[j] & num2) != 0)
                    {
                        intArray2.AddShifted(intArray, j);
                    }
                }

                num2 <<= 1;
                intArray.ShiftLeft();
            }

            return intArray2;
        }

        public void Reduce(int m, int[] redPol)
        {
            for (int num = m + m - 2; num >= m; num--)
            {
                if (TestBit(num))
                {
                    int num2 = num - m;
                    FlipBit(num2);
                    FlipBit(num);
                    int num3 = redPol.Length;
                    while (--num3 >= 0)
                    {
                        FlipBit(redPol[num3] + num2);
                    }
                }
            }

            m_ints = resizedInts(m + 31 >> 5);
        }

        public IntArray Square(int m)
        {
            int[] array = new int[16]
            {
                0, 1, 4, 5, 16, 17, 20, 21, 64, 65,
                68, 69, 80, 81, 84, 85
            };
            int num = m + 31 >> 5;
            if (m_ints.Length < num)
            {
                m_ints = resizedInts(num);
            }

            IntArray intArray = new IntArray(num + num);
            for (int i = 0; i < num; i++)
            {
                int num2 = 0;
                for (int j = 0; j < 4; j++)
                {
                    num2 = (int)((uint)num2 >> 8);
                    int num3 = (int)(((uint)m_ints[i] >> j * 4) & 0xF);
                    int num4 = array[num3] << 24;
                    num2 |= num4;
                }

                intArray.m_ints[i + i] = num2;
                num2 = 0;
                int num5 = (int)((uint)m_ints[i] >> 16);
                for (int k = 0; k < 4; k++)
                {
                    num2 = (int)((uint)num2 >> 8);
                    int num6 = (int)(((uint)num5 >> k * 4) & 0xF);
                    int num7 = array[num6] << 24;
                    num2 |= num7;
                }

                intArray.m_ints[i + i + 1] = num2;
            }

            return intArray;
        }

        public override bool Equals(object o)
        {
            if (!(o is IntArray))
            {
                return false;
            }

            IntArray intArray = (IntArray)o;
            int usedLength = GetUsedLength();
            if (intArray.GetUsedLength() != usedLength)
            {
                return false;
            }

            for (int i = 0; i < usedLength; i++)
            {
                if (m_ints[i] != intArray.m_ints[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int num = GetUsedLength();
            int num2 = num;
            while (--num >= 0)
            {
                num2 *= 17;
                num2 ^= m_ints[num];
            }

            return num2;
        }

        internal IntArray Copy()
        {
            return new IntArray((int[])m_ints.Clone());
        }

        public override string ToString()
        {
            int usedLength = GetUsedLength();
            if (usedLength == 0)
            {
                return "0";
            }

            StringBuilder stringBuilder = new StringBuilder(Convert.ToString(m_ints[usedLength - 1], 2));
            for (int num = usedLength - 2; num >= 0; num--)
            {
                string text = Convert.ToString(m_ints[num], 2);
                for (int i = text.Length; i < 8; i++)
                {
                    text = "0" + text;
                }

                stringBuilder.Append(text);
            }

            return stringBuilder.ToString();
        }
    }
}
