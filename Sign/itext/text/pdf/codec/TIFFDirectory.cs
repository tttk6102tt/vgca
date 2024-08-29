using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf.codec
{
    public class TIFFDirectory
    {
        private bool isBigEndian;

        private int numEntries;

        private TIFFField[] fields;

        private Dictionary<int, int> fieldIndex = new Dictionary<int, int>();

        private long IFDOffset = 8L;

        private long nextIFDOffset;

        private static int[] sizeOfType = new int[13]
        {
            0, 1, 1, 2, 4, 8, 1, 1, 2, 4,
            8, 4, 8
        };

        private TIFFDirectory()
        {
        }

        private static bool IsValidEndianTag(int endian)
        {
            if (endian != 18761)
            {
                return endian == 19789;
            }

            return true;
        }

        public TIFFDirectory(RandomAccessFileOrArray stream, int directory)
        {
            long filePointer = stream.FilePointer;
            stream.Seek(0L);
            int num = stream.ReadUnsignedShort();
            if (!IsValidEndianTag(num))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("bad.endianness.tag.not.0x4949.or.0x4d4d"));
            }

            isBigEndian = num == 19789;
            if (ReadUnsignedShort(stream) != 42)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("bad.magic.number.should.be.42"));
            }

            long num2 = ReadUnsignedInt(stream);
            for (int i = 0; i < directory; i++)
            {
                if (num2 == 0L)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("directory.number.too.large"));
                }

                stream.Seek(num2);
                int num3 = ReadUnsignedShort(stream);
                stream.Skip(12 * num3);
                num2 = ReadUnsignedInt(stream);
            }

            stream.Seek(num2);
            Initialize(stream);
            stream.Seek(filePointer);
        }

        public TIFFDirectory(RandomAccessFileOrArray stream, long ifd_offset, int directory)
        {
            long filePointer = stream.FilePointer;
            stream.Seek(0L);
            int num = stream.ReadUnsignedShort();
            if (!IsValidEndianTag(num))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("bad.endianness.tag.not.0x4949.or.0x4d4d"));
            }

            isBigEndian = num == 19789;
            stream.Seek(ifd_offset);
            for (int i = 0; i < directory; i++)
            {
                int num2 = ReadUnsignedShort(stream);
                stream.Seek(ifd_offset + 12 * num2);
                ifd_offset = ReadUnsignedInt(stream);
                stream.Seek(ifd_offset);
            }

            Initialize(stream);
            stream.Seek(filePointer);
        }

        private void Initialize(RandomAccessFileOrArray stream)
        {
            long num = 0L;
            long length = stream.Length;
            IFDOffset = stream.FilePointer;
            numEntries = ReadUnsignedShort(stream);
            fields = new TIFFField[numEntries];
            for (int i = 0; i < numEntries; i++)
            {
                if (num >= length)
                {
                    break;
                }

                int num2 = ReadUnsignedShort(stream);
                int num3 = ReadUnsignedShort(stream);
                int num4 = (int)ReadUnsignedInt(stream);
                bool flag = true;
                num = stream.FilePointer + 4;
                try
                {
                    if (num4 * sizeOfType[num3] > 4)
                    {
                        long num5 = ReadUnsignedInt(stream);
                        if (num5 < length)
                        {
                            stream.Seek(num5);
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    flag = false;
                }

                if (flag)
                {
                    fieldIndex[num2] = i;
                    object data = null;
                    switch (num3)
                    {
                        case 1:
                        case 2:
                        case 6:
                        case 7:
                            {
                                byte[] array8 = new byte[num4];
                                stream.ReadFully(array8, 0, num4);
                                if (num3 == 2)
                                {
                                    int num8 = 0;
                                    int num9 = 0;
                                    List<string> list = new List<string>();
                                    while (num8 < num4)
                                    {
                                        while (num8 < num4 && array8[num8++] != 0)
                                        {
                                        }

                                        char[] array9 = new char[num8 - num9];
                                        Array.Copy(array8, num9, array9, 0, num8 - num9);
                                        list.Add(new string(array9));
                                        num9 = num8;
                                    }

                                    num4 = list.Count;
                                    string[] array10 = new string[num4];
                                    for (int k = 0; k < num4; k++)
                                    {
                                        array10[k] = list[k];
                                    }

                                    data = array10;
                                }
                                else
                                {
                                    data = array8;
                                }

                                break;
                            }
                        case 3:
                            {
                                char[] array5 = new char[num4];
                                for (int j = 0; j < num4; j++)
                                {
                                    array5[j] = (char)ReadUnsignedShort(stream);
                                }

                                data = array5;
                                break;
                            }
                        case 4:
                            {
                                long[] array11 = new long[num4];
                                for (int j = 0; j < num4; j++)
                                {
                                    array11[j] = ReadUnsignedInt(stream);
                                }

                                data = array11;
                                break;
                            }
                        case 5:
                            {
                                long[][] array3 = new long[num4][];
                                for (int j = 0; j < num4; j++)
                                {
                                    long num6 = ReadUnsignedInt(stream);
                                    long num7 = ReadUnsignedInt(stream);
                                    array3[j] = new long[2] { num6, num7 };
                                }

                                data = array3;
                                break;
                            }
                        case 8:
                            {
                                short[] array6 = new short[num4];
                                for (int j = 0; j < num4; j++)
                                {
                                    array6[j] = ReadShort(stream);
                                }

                                data = array6;
                                break;
                            }
                        case 9:
                            {
                                int[] array2 = new int[num4];
                                for (int j = 0; j < num4; j++)
                                {
                                    array2[j] = ReadInt(stream);
                                }

                                data = array2;
                                break;
                            }
                        case 10:
                            {
                                int[,] array7 = new int[num4, 2];
                                for (int j = 0; j < num4; j++)
                                {
                                    array7[j, 0] = ReadInt(stream);
                                    array7[j, 1] = ReadInt(stream);
                                }

                                data = array7;
                                break;
                            }
                        case 11:
                            {
                                float[] array4 = new float[num4];
                                for (int j = 0; j < num4; j++)
                                {
                                    array4[j] = ReadFloat(stream);
                                }

                                data = array4;
                                break;
                            }
                        case 12:
                            {
                                double[] array = new double[num4];
                                for (int j = 0; j < num4; j++)
                                {
                                    array[j] = ReadDouble(stream);
                                }

                                data = array;
                                break;
                            }
                    }

                    fields[i] = new TIFFField(num2, num3, num4, data);
                }

                stream.Seek(num);
            }

            try
            {
                nextIFDOffset = ReadUnsignedInt(stream);
            }
            catch
            {
                nextIFDOffset = 0L;
            }
        }

        public virtual int GetNumEntries()
        {
            return numEntries;
        }

        public virtual TIFFField GetField(int tag)
        {
            if (fieldIndex.TryGetValue(tag, out var value))
            {
                return fields[value];
            }

            return null;
        }

        public virtual bool IsTagPresent(int tag)
        {
            return fieldIndex.ContainsKey(tag);
        }

        public virtual int[] GetTags()
        {
            int[] array = new int[fieldIndex.Count];
            fieldIndex.Keys.CopyTo(array, 0);
            return array;
        }

        public virtual TIFFField[] GetFields()
        {
            return fields;
        }

        public virtual byte GetFieldAsByte(int tag, int index)
        {
            int num = fieldIndex[tag];
            return fields[num].GetAsBytes()[index];
        }

        public virtual byte GetFieldAsByte(int tag)
        {
            return GetFieldAsByte(tag, 0);
        }

        public virtual long GetFieldAsLong(int tag, int index)
        {
            int num = fieldIndex[tag];
            return fields[num].GetAsLong(index);
        }

        public virtual long GetFieldAsLong(int tag)
        {
            return GetFieldAsLong(tag, 0);
        }

        public virtual float GetFieldAsFloat(int tag, int index)
        {
            int num = fieldIndex[tag];
            return fields[num].GetAsFloat(index);
        }

        public virtual float GetFieldAsFloat(int tag)
        {
            return GetFieldAsFloat(tag, 0);
        }

        public virtual double GetFieldAsDouble(int tag, int index)
        {
            int num = fieldIndex[tag];
            return fields[num].GetAsDouble(index);
        }

        public virtual double GetFieldAsDouble(int tag)
        {
            return GetFieldAsDouble(tag, 0);
        }

        private short ReadShort(RandomAccessFileOrArray stream)
        {
            if (isBigEndian)
            {
                return stream.ReadShort();
            }

            return stream.ReadShortLE();
        }

        private int ReadUnsignedShort(RandomAccessFileOrArray stream)
        {
            if (isBigEndian)
            {
                return stream.ReadUnsignedShort();
            }

            return stream.ReadUnsignedShortLE();
        }

        private int ReadInt(RandomAccessFileOrArray stream)
        {
            if (isBigEndian)
            {
                return stream.ReadInt();
            }

            return stream.ReadIntLE();
        }

        private long ReadUnsignedInt(RandomAccessFileOrArray stream)
        {
            if (isBigEndian)
            {
                return stream.ReadUnsignedInt();
            }

            return stream.ReadUnsignedIntLE();
        }

        private long ReadLong(RandomAccessFileOrArray stream)
        {
            if (isBigEndian)
            {
                return stream.ReadLong();
            }

            return stream.ReadLongLE();
        }

        private float ReadFloat(RandomAccessFileOrArray stream)
        {
            if (isBigEndian)
            {
                return stream.ReadFloat();
            }

            return stream.ReadFloatLE();
        }

        private double ReadDouble(RandomAccessFileOrArray stream)
        {
            if (isBigEndian)
            {
                return stream.ReadDouble();
            }

            return stream.ReadDoubleLE();
        }

        private static int ReadUnsignedShort(RandomAccessFileOrArray stream, bool isBigEndian)
        {
            if (isBigEndian)
            {
                return stream.ReadUnsignedShort();
            }

            return stream.ReadUnsignedShortLE();
        }

        private static long ReadUnsignedInt(RandomAccessFileOrArray stream, bool isBigEndian)
        {
            if (isBigEndian)
            {
                return stream.ReadUnsignedInt();
            }

            return stream.ReadUnsignedIntLE();
        }

        public static int GetNumDirectories(RandomAccessFileOrArray stream)
        {
            long filePointer = stream.FilePointer;
            stream.Seek(0L);
            int num = stream.ReadUnsignedShort();
            if (!IsValidEndianTag(num))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("bad.endianness.tag.not.0x4949.or.0x4d4d"));
            }

            bool flag = num == 19789;
            if (ReadUnsignedShort(stream, flag) != 42)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("bad.magic.number.should.be.42"));
            }

            stream.Seek(4L);
            long num2 = ReadUnsignedInt(stream, flag);
            int num3 = 0;
            while (num2 != 0L)
            {
                num3++;
                try
                {
                    stream.Seek(num2);
                    int num4 = ReadUnsignedShort(stream, flag);
                    stream.Skip(12 * num4);
                    num2 = ReadUnsignedInt(stream, flag);
                }
                catch (EndOfStreamException)
                {
                    num3--;
                    break;
                }
            }

            stream.Seek(filePointer);
            return num3;
        }

        public virtual bool IsBigEndian()
        {
            return isBigEndian;
        }

        public virtual long GetIFDOffset()
        {
            return IFDOffset;
        }

        public virtual long GetNextIFDOffset()
        {
            return nextIFDOffset;
        }
    }
}
