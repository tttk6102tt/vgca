using Sign.itext.io;
using System.Text;

namespace Sign.itext.pdf
{
    public class RandomAccessFileOrArray
    {
        private readonly IRandomAccessSource byteSource;

        private long byteSourcePosition;

        private byte back;

        private bool isBack;

        public virtual long Length => byteSource.Length;

        public virtual long FilePointer => byteSourcePosition - (isBack ? 1 : 0);

        public RandomAccessFileOrArray(string filename)
            : this(new RandomAccessSourceFactory().SetForceRead(forceRead: false).CreateBestSource(filename))
        {
        }

        public RandomAccessFileOrArray(RandomAccessFileOrArray source)
            : this(new IndependentRandomAccessSource(source.byteSource))
        {
        }

        public virtual RandomAccessFileOrArray CreateView()
        {
            return new RandomAccessFileOrArray(new IndependentRandomAccessSource(byteSource));
        }

        public virtual IRandomAccessSource CreateSourceView()
        {
            return new IndependentRandomAccessSource(byteSource);
        }

        public RandomAccessFileOrArray(IRandomAccessSource byteSource)
        {
            this.byteSource = byteSource;
        }

        public RandomAccessFileOrArray(string filename, bool forceRead)
            : this(new RandomAccessSourceFactory().SetForceRead(forceRead).CreateBestSource(filename))
        {
        }

        public RandomAccessFileOrArray(Uri url)
            : this(new RandomAccessSourceFactory().CreateSource(url))
        {
        }

        public RandomAccessFileOrArray(Stream inp)
            : this(new RandomAccessSourceFactory().CreateSource(inp))
        {
        }

        public RandomAccessFileOrArray(byte[] arrayIn)
            : this(new RandomAccessSourceFactory().CreateSource(arrayIn))
        {
        }

        protected internal virtual IRandomAccessSource GetByteSource()
        {
            return byteSource;
        }

        public virtual void PushBack(byte b)
        {
            back = b;
            isBack = true;
        }

        public virtual int Read()
        {
            if (isBack)
            {
                isBack = false;
                return back & 0xFF;
            }

            return byteSource.Get(byteSourcePosition++);
        }

        public virtual int Read(byte[] b, int off, int len)
        {
            if (len == 0)
            {
                return 0;
            }

            int num = 0;
            if (isBack && len > 0)
            {
                isBack = false;
                b[off++] = back;
                len--;
                num++;
            }

            if (len > 0)
            {
                int num2 = byteSource.Get(byteSourcePosition, b, off, len);
                if (num2 > 0)
                {
                    num += num2;
                    byteSourcePosition += num2;
                }
            }

            if (num == 0)
            {
                return -1;
            }

            return num;
        }

        public virtual int Read(byte[] b)
        {
            return Read(b, 0, b.Length);
        }

        public virtual void ReadFully(byte[] b)
        {
            ReadFully(b, 0, b.Length);
        }

        public virtual void ReadFully(byte[] b, int off, int len)
        {
            if (len == 0)
            {
                return;
            }

            int num = 0;
            do
            {
                int num2 = Read(b, off + num, len - num);
                if (num2 <= 0)
                {
                    throw new EndOfStreamException();
                }

                num += num2;
            }
            while (num < len);
        }

        public virtual long Skip(long n)
        {
            return SkipBytes(n);
        }

        public virtual long SkipBytes(long n)
        {
            if (n <= 0)
            {
                return 0L;
            }

            int num = 0;
            if (isBack)
            {
                isBack = false;
                if (n == 1)
                {
                    return 1L;
                }

                n--;
                num = 1;
            }

            long filePointer = FilePointer;
            long length = Length;
            long num2 = filePointer + n;
            if (num2 > length)
            {
                num2 = length;
            }

            Seek(num2);
            return num2 - filePointer + num;
        }

        public virtual void ReOpen()
        {
            Seek(0);
        }

        public virtual void Close()
        {
            isBack = false;
            byteSource.Close();
        }

        public virtual void Seek(long pos)
        {
            byteSourcePosition = pos;
            isBack = false;
        }

        public virtual void Seek(int pos)
        {
            Seek((long)pos);
        }

        public virtual bool ReadBoolean()
        {
            int num = Read();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }

            return num != 0;
        }

        public virtual byte ReadByte()
        {
            int num = Read();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }

            return (byte)num;
        }

        public virtual int ReadUnsignedByte()
        {
            int num = Read();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }

            return num;
        }

        public virtual short ReadShort()
        {
            int num = Read();
            int num2 = Read();
            if ((num | num2) < 0)
            {
                throw new EndOfStreamException();
            }

            return (short)((num << 8) + num2);
        }

        public short ReadShortLE()
        {
            int num = Read();
            int num2 = Read();
            if ((num | num2) < 0)
            {
                throw new EndOfStreamException();
            }

            return (short)((num2 << 8) + num);
        }

        public virtual int ReadUnsignedShort()
        {
            int num = Read();
            int num2 = Read();
            if ((num | num2) < 0)
            {
                throw new EndOfStreamException();
            }

            return (num << 8) + num2;
        }

        public int ReadUnsignedShortLE()
        {
            int num = Read();
            int num2 = Read();
            if ((num | num2) < 0)
            {
                throw new EndOfStreamException();
            }

            return (num2 << 8) + num;
        }

        public virtual char ReadChar()
        {
            int num = Read();
            int num2 = Read();
            if ((num | num2) < 0)
            {
                throw new EndOfStreamException();
            }

            return (char)((num << 8) + num2);
        }

        public char ReadCharLE()
        {
            int num = Read();
            int num2 = Read();
            if ((num | num2) < 0)
            {
                throw new EndOfStreamException();
            }

            return (char)((num2 << 8) + num);
        }

        public virtual int ReadInt()
        {
            int num = Read();
            int num2 = Read();
            int num3 = Read();
            int num4 = Read();
            if ((num | num2 | num3 | num4) < 0)
            {
                throw new EndOfStreamException();
            }

            return (num << 24) + (num2 << 16) + (num3 << 8) + num4;
        }

        public int ReadIntLE()
        {
            int num = Read();
            int num2 = Read();
            int num3 = Read();
            int num4 = Read();
            if ((num | num2 | num3 | num4) < 0)
            {
                throw new EndOfStreamException();
            }

            return (num4 << 24) + (num3 << 16) + (num2 << 8) + num;
        }

        public long ReadUnsignedInt()
        {
            long num = Read();
            long num2 = Read();
            long num3 = Read();
            long num4 = Read();
            if ((num | num2 | num3 | num4) < 0)
            {
                throw new EndOfStreamException();
            }

            return (num << 24) + (num2 << 16) + (num3 << 8) + num4;
        }

        public long ReadUnsignedIntLE()
        {
            long num = Read();
            long num2 = Read();
            long num3 = Read();
            long num4 = Read();
            if ((num | num2 | num3 | num4) < 0)
            {
                throw new EndOfStreamException();
            }

            return (num4 << 24) + (num3 << 16) + (num2 << 8) + num;
        }

        public virtual long ReadLong()
        {
            return ((long)ReadInt() << 32) + (ReadInt() & 0xFFFFFFFFu);
        }

        public long ReadLongLE()
        {
            int num = ReadIntLE();
            return ((long)ReadIntLE() << 32) + (num & 0xFFFFFFFFu);
        }

        public virtual float ReadFloat()
        {
            int[] src = new int[1] { ReadInt() };
            float[] array = new float[1];
            Buffer.BlockCopy(src, 0, array, 0, 4);
            return array[0];
        }

        public float ReadFloatLE()
        {
            int[] src = new int[1] { ReadIntLE() };
            float[] array = new float[1];
            Buffer.BlockCopy(src, 0, array, 0, 4);
            return array[0];
        }

        public virtual double ReadDouble()
        {
            long[] src = new long[1] { ReadLong() };
            double[] array = new double[1];
            Buffer.BlockCopy(src, 0, array, 0, 8);
            return array[0];
        }

        public double ReadDoubleLE()
        {
            long[] src = new long[1] { ReadLongLE() };
            double[] array = new double[1];
            Buffer.BlockCopy(src, 0, array, 0, 8);
            return array[0];
        }

        public virtual string ReadLine()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = -1;
            bool flag = false;
            while (!flag)
            {
                switch (num = Read())
                {
                    case -1:
                    case 10:
                        flag = true;
                        break;
                    case 13:
                        {
                            flag = true;
                            long filePointer = FilePointer;
                            if (Read() != 10)
                            {
                                Seek(filePointer);
                            }

                            break;
                        }
                    default:
                        stringBuilder.Append((char)num);
                        break;
                }
            }

            if (num == -1 && stringBuilder.Length == 0)
            {
                return null;
            }

            return stringBuilder.ToString();
        }

        public virtual string ReadString(int length, string encoding)
        {
            byte[] array = new byte[length];
            ReadFully(array);
            return Encoding.GetEncoding(encoding).GetString(array);
        }
    }
}
