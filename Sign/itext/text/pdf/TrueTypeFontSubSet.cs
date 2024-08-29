using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.SystemItext.util.collections;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class TrueTypeFontSubSet
    {
        internal static string[] tableNamesSimple = new string[9] { "cvt ", "fpgm", "glyf", "head", "hhea", "hmtx", "loca", "maxp", "prep" };

        internal static string[] tableNamesCmap = new string[10] { "cmap", "cvt ", "fpgm", "glyf", "head", "hhea", "hmtx", "loca", "maxp", "prep" };

        internal static string[] tableNamesExtra = new string[11]
        {
            "OS/2", "cmap", "cvt ", "fpgm", "glyf", "head", "hhea", "hmtx", "loca", "maxp",
            "name, prep"
        };

        internal static int[] entrySelectors = new int[21]
        {
            0, 0, 1, 1, 2, 2, 2, 2, 3, 3,
            3, 3, 3, 3, 3, 3, 4, 4, 4, 4,
            4
        };

        internal static int TABLE_CHECKSUM = 0;

        internal static int TABLE_OFFSET = 1;

        internal static int TABLE_LENGTH = 2;

        internal static int HEAD_LOCA_FORMAT_OFFSET = 51;

        internal static int ARG_1_AND_2_ARE_WORDS = 1;

        internal static int WE_HAVE_A_SCALE = 8;

        internal static int MORE_COMPONENTS = 32;

        internal static int WE_HAVE_AN_X_AND_Y_SCALE = 64;

        internal static int WE_HAVE_A_TWO_BY_TWO = 128;

        protected Dictionary<string, int[]> tableDirectory;

        protected RandomAccessFileOrArray rf;

        protected string fileName;

        protected bool includeCmap;

        protected bool includeExtras;

        protected bool locaShortTable;

        protected int[] locaTable;

        protected HashSet2<int> glyphsUsed;

        protected List<int> glyphsInList;

        protected int tableGlyphOffset;

        protected int[] newLocaTable;

        protected byte[] newLocaTableOut;

        protected byte[] newGlyfTable;

        protected int glyfTableRealSize;

        protected int locaTableRealSize;

        protected byte[] outFont;

        protected int fontPtr;

        protected int directoryOffset;

        public TrueTypeFontSubSet(string fileName, RandomAccessFileOrArray rf, HashSet2<int> glyphsUsed, int directoryOffset, bool includeCmap, bool includeExtras)
        {
            this.fileName = fileName;
            this.rf = rf;
            this.glyphsUsed = glyphsUsed;
            this.includeCmap = includeCmap;
            this.includeExtras = includeExtras;
            this.directoryOffset = directoryOffset;
            glyphsInList = new List<int>(glyphsUsed);
        }

        public virtual byte[] Process()
        {
            try
            {
                rf.ReOpen();
                CreateTableDirectory();
                ReadLoca();
                FlatGlyphs();
                CreateNewGlyphTables();
                LocaTobytes();
                AssembleFont();
                return outFont;
            }
            finally
            {
                try
                {
                    rf.Close();
                }
                catch
                {
                }
            }
        }

        protected virtual void AssembleFont()
        {
            int num = 0;
            string[] array = (includeExtras ? tableNamesExtra : ((!includeCmap) ? tableNamesSimple : tableNamesCmap));
            int num2 = 2;
            int num3 = 0;
            int[] value;
            foreach (string text in array)
            {
                if (!text.Equals("glyf") && !text.Equals("loca"))
                {
                    tableDirectory.TryGetValue(text, out value);
                    if (value != null)
                    {
                        num2++;
                        num += (value[TABLE_LENGTH] + 3) & -4;
                    }
                }
            }

            num += newLocaTableOut.Length;
            num += newGlyfTable.Length;
            int num4 = 16 * num2 + 12;
            num += num4;
            outFont = new byte[num];
            fontPtr = 0;
            WriteFontInt(65536);
            WriteFontShort(num2);
            int num5 = entrySelectors[num2];
            WriteFontShort((1 << num5) * 16);
            WriteFontShort(num5);
            WriteFontShort((num2 - (1 << num5)) * 16);
            foreach (string text2 in array)
            {
                tableDirectory.TryGetValue(text2, out value);
                if (value != null)
                {
                    WriteFontString(text2);
                    if (text2.Equals("glyf"))
                    {
                        WriteFontInt(CalculateChecksum(newGlyfTable));
                        num3 = glyfTableRealSize;
                    }
                    else if (text2.Equals("loca"))
                    {
                        WriteFontInt(CalculateChecksum(newLocaTableOut));
                        num3 = locaTableRealSize;
                    }
                    else
                    {
                        WriteFontInt(value[TABLE_CHECKSUM]);
                        num3 = value[TABLE_LENGTH];
                    }

                    WriteFontInt(num4);
                    WriteFontInt(num3);
                    num4 += (num3 + 3) & -4;
                }
            }

            foreach (string text3 in array)
            {
                tableDirectory.TryGetValue(text3, out value);
                if (value != null)
                {
                    if (text3.Equals("glyf"))
                    {
                        Array.Copy(newGlyfTable, 0, outFont, fontPtr, newGlyfTable.Length);
                        fontPtr += newGlyfTable.Length;
                        newGlyfTable = null;
                    }
                    else if (text3.Equals("loca"))
                    {
                        Array.Copy(newLocaTableOut, 0, outFont, fontPtr, newLocaTableOut.Length);
                        fontPtr += newLocaTableOut.Length;
                        newLocaTableOut = null;
                    }
                    else
                    {
                        rf.Seek(value[TABLE_OFFSET]);
                        rf.ReadFully(outFont, fontPtr, value[TABLE_LENGTH]);
                        fontPtr += (value[TABLE_LENGTH] + 3) & -4;
                    }
                }
            }
        }

        protected virtual void CreateTableDirectory()
        {
            tableDirectory = new Dictionary<string, int[]>();
            rf.Seek(directoryOffset);
            if (rf.ReadInt() != 65536)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.true.type.file", fileName));
            }

            int num = rf.ReadUnsignedShort();
            rf.SkipBytes(6L);
            for (int i = 0; i < num; i++)
            {
                string key = ReadStandardString(4);
                int[] array = new int[3];
                array[TABLE_CHECKSUM] = rf.ReadInt();
                array[TABLE_OFFSET] = rf.ReadInt();
                array[TABLE_LENGTH] = rf.ReadInt();
                tableDirectory[key] = array;
            }
        }

        protected virtual void ReadLoca()
        {
            tableDirectory.TryGetValue("head", out var value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "head", fileName));
            }

            rf.Seek(value[TABLE_OFFSET] + HEAD_LOCA_FORMAT_OFFSET);
            locaShortTable = rf.ReadUnsignedShort() == 0;
            tableDirectory.TryGetValue("loca", out value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "loca", fileName));
            }

            rf.Seek(value[TABLE_OFFSET]);
            if (locaShortTable)
            {
                int num = value[TABLE_LENGTH] / 2;
                locaTable = new int[num];
                for (int i = 0; i < num; i++)
                {
                    locaTable[i] = rf.ReadUnsignedShort() * 2;
                }
            }
            else
            {
                int num2 = value[TABLE_LENGTH] / 4;
                locaTable = new int[num2];
                for (int j = 0; j < num2; j++)
                {
                    locaTable[j] = rf.ReadInt();
                }
            }
        }

        protected virtual void CreateNewGlyphTables()
        {
            newLocaTable = new int[locaTable.Length];
            int[] array = new int[glyphsInList.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = glyphsInList[i];
            }

            Array.Sort(array);
            int num = 0;
            foreach (int num2 in array)
            {
                num += locaTable[num2 + 1] - locaTable[num2];
            }

            glyfTableRealSize = num;
            num = (num + 3) & -4;
            newGlyfTable = new byte[num];
            int num3 = 0;
            int num4 = 0;
            for (int k = 0; k < newLocaTable.Length; k++)
            {
                newLocaTable[k] = num3;
                if (num4 < array.Length && array[num4] == k)
                {
                    num4++;
                    newLocaTable[k] = num3;
                    int num5 = locaTable[k];
                    int num6 = locaTable[k + 1] - num5;
                    if (num6 > 0)
                    {
                        rf.Seek(tableGlyphOffset + num5);
                        rf.ReadFully(newGlyfTable, num3, num6);
                        num3 += num6;
                    }
                }
            }
        }

        protected virtual void LocaTobytes()
        {
            if (locaShortTable)
            {
                locaTableRealSize = newLocaTable.Length * 2;
            }
            else
            {
                locaTableRealSize = newLocaTable.Length * 4;
            }

            newLocaTableOut = new byte[(locaTableRealSize + 3) & -4];
            outFont = newLocaTableOut;
            fontPtr = 0;
            for (int i = 0; i < newLocaTable.Length; i++)
            {
                if (locaShortTable)
                {
                    WriteFontShort(newLocaTable[i] / 2);
                }
                else
                {
                    WriteFontInt(newLocaTable[i]);
                }
            }
        }

        protected virtual void FlatGlyphs()
        {
            tableDirectory.TryGetValue("glyf", out var value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "glyf", fileName));
            }

            int item = 0;
            if (!glyphsUsed.Contains(item))
            {
                glyphsUsed.Add(item);
                glyphsInList.Add(item);
            }

            tableGlyphOffset = value[TABLE_OFFSET];
            for (int i = 0; i < glyphsInList.Count; i++)
            {
                int glyph = glyphsInList[i];
                CheckGlyphComposite(glyph);
            }
        }

        protected virtual void CheckGlyphComposite(int glyph)
        {
            int num = locaTable[glyph];
            if (num == locaTable[glyph + 1])
            {
                return;
            }

            rf.Seek(tableGlyphOffset + num);
            if (rf.ReadShort() >= 0)
            {
                return;
            }

            rf.SkipBytes(8L);
            while (true)
            {
                int num2 = rf.ReadUnsignedShort();
                int item = rf.ReadUnsignedShort();
                if (!glyphsUsed.Contains(item))
                {
                    glyphsUsed.Add(item);
                    glyphsInList.Add(item);
                }

                if ((num2 & MORE_COMPONENTS) == 0)
                {
                    break;
                }

                int num3 = (((num2 & ARG_1_AND_2_ARE_WORDS) == 0) ? 2 : 4);
                if ((num2 & WE_HAVE_A_SCALE) != 0)
                {
                    num3 += 2;
                }
                else if ((num2 & WE_HAVE_AN_X_AND_Y_SCALE) != 0)
                {
                    num3 += 4;
                }

                if ((num2 & WE_HAVE_A_TWO_BY_TWO) != 0)
                {
                    num3 += 8;
                }

                rf.SkipBytes(num3);
            }
        }

        protected virtual string ReadStandardString(int length)
        {
            byte[] array = new byte[length];
            rf.ReadFully(array);
            return Encoding.GetEncoding(1252).GetString(array);
        }

        protected virtual void WriteFontShort(int n)
        {
            outFont[fontPtr++] = (byte)(n >> 8);
            outFont[fontPtr++] = (byte)n;
        }

        protected virtual void WriteFontInt(int n)
        {
            outFont[fontPtr++] = (byte)(n >> 24);
            outFont[fontPtr++] = (byte)(n >> 16);
            outFont[fontPtr++] = (byte)(n >> 8);
            outFont[fontPtr++] = (byte)n;
        }

        protected virtual void WriteFontString(string s)
        {
            byte[] array = PdfEncodings.ConvertToBytes(s, "Cp1252");
            Array.Copy(array, 0, outFont, fontPtr, array.Length);
            fontPtr += array.Length;
        }

        protected virtual int CalculateChecksum(byte[] b)
        {
            int num = b.Length / 4;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            for (int i = 0; i < num; i++)
            {
                num5 += b[num6++] & 0xFF;
                num4 += b[num6++] & 0xFF;
                num3 += b[num6++] & 0xFF;
                num2 += b[num6++] & 0xFF;
            }

            return num2 + (num3 << 8) + (num4 << 16) + (num5 << 24);
        }
    }
}
