using Sign.itext.error_messages;
using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    internal class EnumerateTTC : TrueTypeFont
    {
        protected string[] names;

        internal string[] Names => names;

        internal EnumerateTTC(string ttcFile)
        {
            fileName = ttcFile;
            rf = new RandomAccessFileOrArray(ttcFile);
            FindNames();
        }

        internal EnumerateTTC(byte[] ttcArray)
        {
            fileName = "Byte array TTC";
            rf = new RandomAccessFileOrArray(ttcArray);
            FindNames();
        }

        internal void FindNames()
        {
            tables = new Dictionary<string, int[]>();
            try
            {
                if (!ReadStandardString(4).Equals("ttcf"))
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.ttc.file", fileName));
                }

                rf.SkipBytes(4L);
                int num = rf.ReadInt();
                names = new string[num];
                int pos = (int)rf.FilePointer;
                for (int i = 0; i < num; i++)
                {
                    tables.Clear();
                    rf.Seek(pos);
                    rf.SkipBytes(i * 4);
                    directoryOffset = rf.ReadInt();
                    rf.Seek(directoryOffset);
                    if (rf.ReadInt() != 65536)
                    {
                        throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.ttf.file", fileName));
                    }

                    int num2 = rf.ReadUnsignedShort();
                    rf.SkipBytes(6L);
                    for (int j = 0; j < num2; j++)
                    {
                        string key = ReadStandardString(4);
                        rf.SkipBytes(4L);
                        int[] value = new int[2]
                        {
                            rf.ReadInt(),
                            rf.ReadInt()
                        };
                        tables[key] = value;
                    }

                    names[i] = base.BaseFont;
                }
            }
            finally
            {
                if (rf != null)
                {
                    rf.Close();
                }
            }
        }
    }
}
