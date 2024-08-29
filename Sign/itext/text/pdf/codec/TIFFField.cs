namespace Sign.itext.text.pdf.codec
{
    public class TIFFField : IComparable<TIFFField>
    {
        public const int TIFF_BYTE = 1;

        public const int TIFF_ASCII = 2;

        public const int TIFF_SHORT = 3;

        public const int TIFF_LONG = 4;

        public const int TIFF_RATIONAL = 5;

        public const int TIFF_SBYTE = 6;

        public const int TIFF_UNDEFINED = 7;

        public const int TIFF_SSHORT = 8;

        public const int TIFF_SLONG = 9;

        public const int TIFF_SRATIONAL = 10;

        public const int TIFF_FLOAT = 11;

        public const int TIFF_DOUBLE = 12;

        private int tag;

        private int type;

        private int count;

        private object data;

        internal TIFFField()
        {
        }

        public TIFFField(int tag, int type, int count, object data)
        {
            this.tag = tag;
            this.type = type;
            this.count = count;
            this.data = data;
        }

        public virtual int GetTag()
        {
            return tag;
        }

        public new int GetType()
        {
            return type;
        }

        public virtual int GetCount()
        {
            return count;
        }

        public virtual byte[] GetAsBytes()
        {
            return (byte[])data;
        }

        public virtual char[] GetAsChars()
        {
            return (char[])data;
        }

        public virtual short[] GetAsShorts()
        {
            return (short[])data;
        }

        public virtual int[] GetAsInts()
        {
            return (int[])data;
        }

        public virtual long[] GetAsLongs()
        {
            return (long[])data;
        }

        public virtual float[] GetAsFloats()
        {
            return (float[])data;
        }

        public virtual double[] GetAsDoubles()
        {
            return (double[])data;
        }

        public virtual int[][] GetAsSRationals()
        {
            return (int[][])data;
        }

        public virtual long[][] GetAsRationals()
        {
            return (long[][])data;
        }

        public virtual int GetAsInt(int index)
        {
            switch (type)
            {
                case 1:
                case 7:
                    return ((byte[])data)[index] & 0xFF;
                case 6:
                    return ((byte[])data)[index];
                case 3:
                    return ((char[])data)[index] & 0xFFFF;
                case 8:
                    return ((short[])data)[index];
                case 9:
                    return ((int[])data)[index];
                default:
                    throw new InvalidCastException();
            }
        }

        public virtual long GetAsLong(int index)
        {
            switch (type)
            {
                case 1:
                case 7:
                    return ((byte[])data)[index] & 0xFF;
                case 6:
                    return ((byte[])data)[index];
                case 3:
                    return ((char[])data)[index] & 0xFFFF;
                case 8:
                    return ((short[])data)[index];
                case 9:
                    return ((int[])data)[index];
                case 4:
                    return ((long[])data)[index];
                default:
                    throw new InvalidCastException();
            }
        }

        public virtual float GetAsFloat(int index)
        {
            switch (type)
            {
                case 1:
                    return ((byte[])data)[index] & 0xFF;
                case 6:
                    return (int)((byte[])data)[index];
                case 3:
                    return ((char[])data)[index] & 0xFFFF;
                case 8:
                    return ((short[])data)[index];
                case 9:
                    return ((int[])data)[index];
                case 4:
                    return ((long[])data)[index];
                case 11:
                    return ((float[])data)[index];
                case 12:
                    return (float)((double[])data)[index];
                case 10:
                    {
                        int[] asSRational = GetAsSRational(index);
                        return (float)((double)asSRational[0] / (double)asSRational[1]);
                    }
                case 5:
                    {
                        long[] asRational = GetAsRational(index);
                        return (float)((double)asRational[0] / (double)asRational[1]);
                    }
                default:
                    throw new InvalidCastException();
            }
        }

        public virtual double GetAsDouble(int index)
        {
            switch (type)
            {
                case 1:
                    return ((byte[])data)[index] & 0xFF;
                case 6:
                    return (int)((byte[])data)[index];
                case 3:
                    return ((char[])data)[index] & 0xFFFF;
                case 8:
                    return ((short[])data)[index];
                case 9:
                    return ((int[])data)[index];
                case 4:
                    return ((long[])data)[index];
                case 11:
                    return ((float[])data)[index];
                case 12:
                    return ((double[])data)[index];
                case 10:
                    {
                        int[] asSRational = GetAsSRational(index);
                        return (double)asSRational[0] / (double)asSRational[1];
                    }
                case 5:
                    {
                        long[] asRational = GetAsRational(index);
                        return (double)asRational[0] / (double)asRational[1];
                    }
                default:
                    throw new InvalidCastException();
            }
        }

        public virtual string GetAsString(int index)
        {
            return ((string[])data)[index];
        }

        public virtual int[] GetAsSRational(int index)
        {
            return ((int[][])data)[index];
        }

        public virtual long[] GetAsRational(int index)
        {
            if (type == 4)
            {
                return GetAsLongs();
            }

            return ((long[][])data)[index];
        }

        public virtual int CompareTo(TIFFField o)
        {
            if (o == null)
            {
                throw new ArgumentException();
            }

            int num = o.GetTag();
            if (tag < num)
            {
                return -1;
            }

            if (tag > num)
            {
                return 1;
            }

            return 0;
        }
    }
}
