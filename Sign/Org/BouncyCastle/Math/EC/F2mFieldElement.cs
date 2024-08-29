namespace Sign.Org.BouncyCastle.Math.EC
{
    public class F2mFieldElement : ECFieldElement
    {
        public const int Gnb = 1;

        public const int Tpb = 2;

        public const int Ppb = 3;

        private int representation;

        private int m;

        private int k1;

        private int k2;

        private int k3;

        private IntArray x;

        private readonly int t;

        public override string FieldName => "F2m";

        public override int FieldSize => m;

        public int Representation => representation;

        public int M => m;

        public int K1 => k1;

        public int K2 => k2;

        public int K3 => k3;

        public F2mFieldElement(int m, int k1, int k2, int k3, BigInteger x)
        {
            t = m + 31 >> 5;
            this.x = new IntArray(x, t);
            if (k2 == 0 && k3 == 0)
            {
                representation = 2;
            }
            else
            {
                if (k2 >= k3)
                {
                    throw new ArgumentException("k2 must be smaller than k3");
                }

                if (k2 <= 0)
                {
                    throw new ArgumentException("k2 must be larger than 0");
                }

                representation = 3;
            }

            if (x.SignValue < 0)
            {
                throw new ArgumentException("x value cannot be negative");
            }

            this.m = m;
            this.k1 = k1;
            this.k2 = k2;
            this.k3 = k3;
        }

        public F2mFieldElement(int m, int k, BigInteger x)
            : this(m, k, 0, 0, x)
        {
        }

        private F2mFieldElement(int m, int k1, int k2, int k3, IntArray x)
        {
            t = m + 31 >> 5;
            this.x = x;
            this.m = m;
            this.k1 = k1;
            this.k2 = k2;
            this.k3 = k3;
            if (k2 == 0 && k3 == 0)
            {
                representation = 2;
            }
            else
            {
                representation = 3;
            }
        }

        public override BigInteger ToBigInteger()
        {
            return x.ToBigInteger();
        }

        public static void CheckFieldElements(ECFieldElement a, ECFieldElement b)
        {
            if (!(a is F2mFieldElement) || !(b is F2mFieldElement))
            {
                throw new ArgumentException("Field elements are not both instances of F2mFieldElement");
            }

            F2mFieldElement f2mFieldElement = (F2mFieldElement)a;
            F2mFieldElement f2mFieldElement2 = (F2mFieldElement)b;
            if (f2mFieldElement.m != f2mFieldElement2.m || f2mFieldElement.k1 != f2mFieldElement2.k1 || f2mFieldElement.k2 != f2mFieldElement2.k2 || f2mFieldElement.k3 != f2mFieldElement2.k3)
            {
                throw new ArgumentException("Field elements are not elements of the same field F2m");
            }

            if (f2mFieldElement.representation != f2mFieldElement2.representation)
            {
                throw new ArgumentException("One of the field elements are not elements has incorrect representation");
            }
        }

        public override ECFieldElement Add(ECFieldElement b)
        {
            IntArray intArray = x.Copy();
            F2mFieldElement f2mFieldElement = (F2mFieldElement)b;
            intArray.AddShifted(f2mFieldElement.x, 0);
            return new F2mFieldElement(m, k1, k2, k3, intArray);
        }

        public override ECFieldElement Subtract(ECFieldElement b)
        {
            return Add(b);
        }

        public override ECFieldElement Multiply(ECFieldElement b)
        {
            F2mFieldElement f2mFieldElement = (F2mFieldElement)b;
            IntArray intArray = x.Multiply(f2mFieldElement.x, m);
            intArray.Reduce(m, new int[3] { k1, k2, k3 });
            return new F2mFieldElement(m, k1, k2, k3, intArray);
        }

        public override ECFieldElement Divide(ECFieldElement b)
        {
            ECFieldElement b2 = b.Invert();
            return Multiply(b2);
        }

        public override ECFieldElement Negate()
        {
            return this;
        }

        public override ECFieldElement Square()
        {
            IntArray intArray = x.Square(m);
            intArray.Reduce(m, new int[3] { k1, k2, k3 });
            return new F2mFieldElement(m, k1, k2, k3, intArray);
        }

        public override ECFieldElement Invert()
        {
            IntArray intArray = x.Copy();
            IntArray intArray2 = new IntArray(t);
            intArray2.SetBit(m);
            intArray2.SetBit(0);
            intArray2.SetBit(k1);
            if (representation == 3)
            {
                intArray2.SetBit(k2);
                intArray2.SetBit(k3);
            }

            IntArray intArray3 = new IntArray(t);
            intArray3.SetBit(0);
            IntArray intArray4 = new IntArray(t);
            while (intArray.GetUsedLength() > 0)
            {
                int num = intArray.BitLength - intArray2.BitLength;
                if (num < 0)
                {
                    IntArray intArray5 = intArray;
                    intArray = intArray2;
                    intArray2 = intArray5;
                    IntArray intArray6 = intArray3;
                    intArray3 = intArray4;
                    intArray4 = intArray6;
                    num = -num;
                }

                int shift = num >> 5;
                int n = num & 0x1F;
                IntArray other = intArray2.ShiftLeft(n);
                intArray.AddShifted(other, shift);
                IntArray other2 = intArray4.ShiftLeft(n);
                intArray3.AddShifted(other2, shift);
            }

            return new F2mFieldElement(m, k1, k2, k3, intArray4);
        }

        public override ECFieldElement Sqrt()
        {
            throw new ArithmeticException("Not implemented");
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            F2mFieldElement f2mFieldElement = obj as F2mFieldElement;
            if (f2mFieldElement == null)
            {
                return false;
            }

            return Equals(f2mFieldElement);
        }

        protected bool Equals(F2mFieldElement other)
        {
            if (m == other.m && k1 == other.k1 && k2 == other.k2 && k3 == other.k3 && representation == other.representation)
            {
                return Equals((ECFieldElement)other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m.GetHashCode() ^ k1.GetHashCode() ^ k2.GetHashCode() ^ k3.GetHashCode() ^ representation.GetHashCode() ^ base.GetHashCode();
        }
    }
}
