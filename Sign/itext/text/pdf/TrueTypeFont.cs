using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.SystemItext.util.collections;
using System.Globalization;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class TrueTypeFont : BaseFont
    {
        protected class FontHeader
        {
            internal int flags;

            internal int unitsPerEm;

            internal short xMin;

            internal short yMin;

            internal short xMax;

            internal short yMax;

            internal int macStyle;
        }

        protected class HorizontalHeader
        {
            internal short Ascender;

            internal short Descender;

            internal short LineGap;

            internal int advanceWidthMax;

            internal short minLeftSideBearing;

            internal short minRightSideBearing;

            internal short xMaxExtent;

            internal short caretSlopeRise;

            internal short caretSlopeRun;

            internal int numberOfHMetrics;
        }

        protected class WindowsMetrics
        {
            internal short xAvgCharWidth;

            internal int usWeightClass;

            internal int usWidthClass;

            internal short fsType;

            internal short ySubscriptXSize;

            internal short ySubscriptYSize;

            internal short ySubscriptXOffset;

            internal short ySubscriptYOffset;

            internal short ySuperscriptXSize;

            internal short ySuperscriptYSize;

            internal short ySuperscriptXOffset;

            internal short ySuperscriptYOffset;

            internal short yStrikeoutSize;

            internal short yStrikeoutPosition;

            internal short sFamilyClass;

            internal byte[] panose = new byte[10];

            internal byte[] achVendID = new byte[4];

            internal int fsSelection;

            internal int usFirstCharIndex;

            internal int usLastCharIndex;

            internal short sTypoAscender;

            internal short sTypoDescender;

            internal short sTypoLineGap;

            internal int usWinAscent;

            internal int usWinDescent;

            internal int ulCodePageRange1;

            internal int ulCodePageRange2;

            internal int sCapHeight;
        }

        internal static string[] codePages = new string[64]
        {
            "1252 Latin 1", "1250 Latin 2: Eastern Europe", "1251 Cyrillic", "1253 Greek", "1254 Turkish", "1255 Hebrew", "1256 Arabic", "1257 Windows Baltic", "1258 Vietnamese", null,
            null, null, null, null, null, null, "874 Thai", "932 JIS/Japan", "936 Chinese: Simplified chars--PRC and Singapore", "949 Korean Wansung",
            "950 Chinese: Traditional chars--Taiwan and Hong Kong", "1361 Korean Johab", null, null, null, null, null, null, null, "Macintosh Character Set (US Roman)",
            "OEM Character Set", "Symbol Character Set", null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, "869 IBM Greek", "866 MS-DOS Russian",
            "865 MS-DOS Nordic", "864 Arabic", "863 MS-DOS Canadian French", "862 Hebrew", "861 MS-DOS Icelandic", "860 MS-DOS Portuguese", "857 IBM Turkish", "855 IBM Cyrillic; primarily Russian", "852 Latin 2", "775 MS-DOS Baltic",
            "737 Greek; former 437 G", "708 Arabic; ASMO 708", "850 WE/Latin 1", "437 US"
        };

        protected bool justNames;

        protected Dictionary<string, int[]> tables;

        protected RandomAccessFileOrArray rf;

        protected string fileName;

        protected bool cff;

        protected int cffOffset;

        protected int cffLength;

        protected int directoryOffset;

        protected string ttcIndex;

        protected string style = "";

        protected FontHeader head = new FontHeader();

        protected HorizontalHeader hhea = new HorizontalHeader();

        protected WindowsMetrics os_2 = new WindowsMetrics();

        protected int[] glyphWidthsByIndex;

        protected int[][] bboxes;

        protected Dictionary<int, int[]> cmap10;

        protected Dictionary<int, int[]> cmap31;

        protected Dictionary<int, int[]> cmapExt;

        protected int[] glyphIdToChar;

        protected int maxGlyphId;

        protected IntHashtable kerning = new IntHashtable();

        protected string fontName;

        protected string[][] fullName;

        protected string[][] allNameEntries;

        protected string[][] familyName;

        protected double italicAngle;

        protected bool isFixedPitch;

        protected int underlinePosition;

        protected int underlineThickness;

        public virtual RandomAccessFileOrArray Rf => rf;

        public virtual string FileName => fileName;

        public virtual bool Cff => cff;

        public virtual int DirectoryOffset => directoryOffset;

        internal string BaseFont
        {
            get
            {
                tables.TryGetValue("name", out var value);
                if (value == null)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "name", fileName + style));
                }

                rf.Seek(value[0] + 2);
                int num = rf.ReadUnsignedShort();
                int num2 = rf.ReadUnsignedShort();
                for (int i = 0; i < num; i++)
                {
                    int num3 = rf.ReadUnsignedShort();
                    rf.ReadUnsignedShort();
                    rf.ReadUnsignedShort();
                    int num4 = rf.ReadUnsignedShort();
                    int length = rf.ReadUnsignedShort();
                    int num5 = rf.ReadUnsignedShort();
                    if (num4 == 6)
                    {
                        rf.Seek(value[0] + num2 + num5);
                        if (num3 == 0 || num3 == 3)
                        {
                            return ReadUnicodeString(length);
                        }

                        return ReadStandardString(length);
                    }
                }

                return new FileInfo(fileName).Name.Replace(' ', '-');
            }
        }

        public override string PostscriptFontName
        {
            get
            {
                return fontName;
            }
            set
            {
                fontName = value;
            }
        }

        public override string[] CodePagesSupported
        {
            get
            {
                long num = ((long)os_2.ulCodePageRange2 << 32) + (os_2.ulCodePageRange1 & 0xFFFFFFFFu);
                int num2 = 0;
                long num3 = 1L;
                for (int i = 0; i < 64; i++)
                {
                    if ((num & num3) != 0L && codePages[i] != null)
                    {
                        num2++;
                    }

                    num3 <<= 1;
                }

                string[] array = new string[num2];
                num2 = 0;
                num3 = 1L;
                for (int j = 0; j < 64; j++)
                {
                    if ((num & num3) != 0L && codePages[j] != null)
                    {
                        array[num2++] = codePages[j];
                    }

                    num3 <<= 1;
                }

                return array;
            }
        }

        public override string[][] FullFontName => fullName;

        public override string[][] AllNameEntries => allNameEntries;

        public override string[][] FamilyFontName => familyName;

        protected TrueTypeFont()
        {
        }

        internal TrueTypeFont(string ttFile, string enc, bool emb, byte[] ttfAfm, bool justNames, bool forceRead)
        {
            this.justNames = justNames;
            string baseName = Sign.itext.pdf.BaseFont.GetBaseName(ttFile);
            string tTCName = GetTTCName(baseName);
            if (baseName.Length < ttFile.Length)
            {
                style = ttFile.Substring(baseName.Length);
            }

            encoding = enc;
            embedded = emb;
            fileName = tTCName;
            FontType = 1;
            ttcIndex = "";
            if (tTCName.Length < baseName.Length)
            {
                ttcIndex = baseName.Substring(tTCName.Length + 1);
            }

            if (fileName.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") || fileName.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") || fileName.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttc"))
            {
                Process(ttfAfm, forceRead);
                if (!justNames && embedded && os_2.fsType == 2)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("1.cannot.be.embedded.due.to.licensing.restrictions", fileName + style));
                }

                if (!encoding.StartsWith("#"))
                {
                    PdfEncodings.ConvertToBytes(" ", enc);
                }

                CreateEncoding();
                return;
            }

            throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.ttf.otf.or.ttc.font.file", fileName + style));
        }

        protected static string GetTTCName(string name)
        {
            int num = name.ToLower(CultureInfo.InvariantCulture).IndexOf(".ttc,");
            if (num < 0)
            {
                return name;
            }

            return name.Substring(0, num + 4);
        }

        internal void FillTables()
        {
            tables.TryGetValue("head", out var value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "head", fileName + style));
            }

            rf.Seek(value[0] + 16);
            head.flags = rf.ReadUnsignedShort();
            head.unitsPerEm = rf.ReadUnsignedShort();
            rf.SkipBytes(16L);
            head.xMin = rf.ReadShort();
            head.yMin = rf.ReadShort();
            head.xMax = rf.ReadShort();
            head.yMax = rf.ReadShort();
            head.macStyle = rf.ReadUnsignedShort();
            tables.TryGetValue("hhea", out value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "hhea", fileName + style));
            }

            rf.Seek(value[0] + 4);
            hhea.Ascender = rf.ReadShort();
            hhea.Descender = rf.ReadShort();
            hhea.LineGap = rf.ReadShort();
            hhea.advanceWidthMax = rf.ReadUnsignedShort();
            hhea.minLeftSideBearing = rf.ReadShort();
            hhea.minRightSideBearing = rf.ReadShort();
            hhea.xMaxExtent = rf.ReadShort();
            hhea.caretSlopeRise = rf.ReadShort();
            hhea.caretSlopeRun = rf.ReadShort();
            rf.SkipBytes(12L);
            hhea.numberOfHMetrics = rf.ReadUnsignedShort();
            tables.TryGetValue("OS/2", out value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "OS/2", fileName + style));
            }

            rf.Seek(value[0]);
            int num = rf.ReadUnsignedShort();
            os_2.xAvgCharWidth = rf.ReadShort();
            os_2.usWeightClass = rf.ReadUnsignedShort();
            os_2.usWidthClass = rf.ReadUnsignedShort();
            os_2.fsType = rf.ReadShort();
            os_2.ySubscriptXSize = rf.ReadShort();
            os_2.ySubscriptYSize = rf.ReadShort();
            os_2.ySubscriptXOffset = rf.ReadShort();
            os_2.ySubscriptYOffset = rf.ReadShort();
            os_2.ySuperscriptXSize = rf.ReadShort();
            os_2.ySuperscriptYSize = rf.ReadShort();
            os_2.ySuperscriptXOffset = rf.ReadShort();
            os_2.ySuperscriptYOffset = rf.ReadShort();
            os_2.yStrikeoutSize = rf.ReadShort();
            os_2.yStrikeoutPosition = rf.ReadShort();
            os_2.sFamilyClass = rf.ReadShort();
            rf.ReadFully(os_2.panose);
            rf.SkipBytes(16L);
            rf.ReadFully(os_2.achVendID);
            os_2.fsSelection = rf.ReadUnsignedShort();
            os_2.usFirstCharIndex = rf.ReadUnsignedShort();
            os_2.usLastCharIndex = rf.ReadUnsignedShort();
            os_2.sTypoAscender = rf.ReadShort();
            os_2.sTypoDescender = rf.ReadShort();
            if (os_2.sTypoDescender > 0)
            {
                os_2.sTypoDescender = (short)(-os_2.sTypoDescender);
            }

            os_2.sTypoLineGap = rf.ReadShort();
            os_2.usWinAscent = rf.ReadUnsignedShort();
            os_2.usWinDescent = rf.ReadUnsignedShort();
            os_2.ulCodePageRange1 = 0;
            os_2.ulCodePageRange2 = 0;
            if (num > 0)
            {
                os_2.ulCodePageRange1 = rf.ReadInt();
                os_2.ulCodePageRange2 = rf.ReadInt();
            }

            if (num > 1)
            {
                rf.SkipBytes(2L);
                os_2.sCapHeight = rf.ReadShort();
            }
            else
            {
                os_2.sCapHeight = (int)(0.7 * (double)head.unitsPerEm);
            }

            tables.TryGetValue("post", out value);
            if (value == null)
            {
                italicAngle = (0.0 - Math.Atan2(hhea.caretSlopeRun, hhea.caretSlopeRise)) * 180.0 / Math.PI;
            }
            else
            {
                rf.Seek(value[0] + 4);
                short num2 = rf.ReadShort();
                int num3 = rf.ReadUnsignedShort();
                italicAngle = (double)num2 + (double)num3 / 16384.0;
                underlinePosition = rf.ReadShort();
                underlineThickness = rf.ReadShort();
                isFixedPitch = rf.ReadInt() != 0;
            }

            tables.TryGetValue("maxp", out value);
            if (value == null)
            {
                maxGlyphId = 65536;
                return;
            }

            rf.Seek(value[0] + 4);
            maxGlyphId = rf.ReadUnsignedShort();
        }

        internal string[][] GetNames(int id)
        {
            tables.TryGetValue("name", out var value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "name", fileName + style));
            }

            rf.Seek(value[0] + 2);
            int num = rf.ReadUnsignedShort();
            int num2 = rf.ReadUnsignedShort();
            List<string[]> list = new List<string[]>();
            for (int i = 0; i < num; i++)
            {
                int num3 = rf.ReadUnsignedShort();
                int num4 = rf.ReadUnsignedShort();
                int num5 = rf.ReadUnsignedShort();
                int num6 = rf.ReadUnsignedShort();
                int length = rf.ReadUnsignedShort();
                int num7 = rf.ReadUnsignedShort();
                if (num6 == id)
                {
                    int pos = (int)rf.FilePointer;
                    rf.Seek(value[0] + num2 + num7);
                    string text = ((num3 != 0 && num3 != 3 && (num3 != 2 || num4 != 1)) ? ReadStandardString(length) : ReadUnicodeString(length));
                    list.Add(new string[4]
                    {
                        num3.ToString(),
                        num4.ToString(),
                        num5.ToString(),
                        text
                    });
                    rf.Seek(pos);
                }
            }

            string[][] array = new string[list.Count][];
            for (int j = 0; j < list.Count; j++)
            {
                array[j] = list[j];
            }

            return array;
        }

        internal string[][] GetAllNames()
        {
            tables.TryGetValue("name", out var value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "name", fileName + style));
            }

            rf.Seek(value[0] + 2);
            int num = rf.ReadUnsignedShort();
            int num2 = rf.ReadUnsignedShort();
            List<string[]> list = new List<string[]>();
            for (int i = 0; i < num; i++)
            {
                int num3 = rf.ReadUnsignedShort();
                int num4 = rf.ReadUnsignedShort();
                int num5 = rf.ReadUnsignedShort();
                int num6 = rf.ReadUnsignedShort();
                int length = rf.ReadUnsignedShort();
                int num7 = rf.ReadUnsignedShort();
                int pos = (int)rf.FilePointer;
                rf.Seek(value[0] + num2 + num7);
                string text = ((num3 != 0 && num3 != 3 && (num3 != 2 || num4 != 1)) ? ReadStandardString(length) : ReadUnicodeString(length));
                list.Add(new string[5]
                {
                    num6.ToString(),
                    num3.ToString(),
                    num4.ToString(),
                    num5.ToString(),
                    text
                });
                rf.Seek(pos);
            }

            string[][] array = new string[list.Count][];
            for (int j = 0; j < list.Count; j++)
            {
                array[j] = list[j];
            }

            return array;
        }

        internal void CheckCff()
        {
            tables.TryGetValue("CFF ", out var value);
            if (value != null)
            {
                cff = true;
                cffOffset = value[0];
                cffLength = value[1];
            }
        }

        internal void Process(byte[] ttfAfm, bool preload)
        {
            tables = new Dictionary<string, int[]>();
            try
            {
                if (ttfAfm == null)
                {
                    rf = new RandomAccessFileOrArray(fileName, preload);
                }
                else
                {
                    rf = new RandomAccessFileOrArray(ttfAfm);
                }

                if (ttcIndex.Length > 0)
                {
                    int num = int.Parse(ttcIndex);
                    if (num < 0)
                    {
                        throw new DocumentException(MessageLocalization.GetComposedMessage("the.font.index.for.1.must.be.positive", fileName));
                    }

                    if (!ReadStandardString(4).Equals("ttcf"))
                    {
                        throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.ttc.file", fileName));
                    }

                    rf.SkipBytes(4L);
                    int num2 = rf.ReadInt();
                    if (num >= num2)
                    {
                        throw new DocumentException(MessageLocalization.GetComposedMessage("the.font.index.for.1.must.be.between.0.and.2.it.was.3", fileName, num2 - 1, num));
                    }

                    rf.SkipBytes(num * 4);
                    directoryOffset = rf.ReadInt();
                }

                rf.Seek(directoryOffset);
                int num3 = rf.ReadInt();
                if (num3 != 65536 && num3 != 1330926671)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.ttf.or.otf.file", fileName));
                }

                int num4 = rf.ReadUnsignedShort();
                rf.SkipBytes(6L);
                for (int i = 0; i < num4; i++)
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

                CheckCff();
                fontName = BaseFont;
                fullName = GetNames(4);
                familyName = GetNames(1);
                allNameEntries = GetAllNames();
                if (!justNames)
                {
                    FillTables();
                    ReadGlyphWidths();
                    ReadCMaps();
                    ReadKerning();
                    ReadBbox();
                }
            }
            finally
            {
                if (!embedded)
                {
                    rf.Close();
                    rf = null;
                }
            }
        }

        protected virtual string ReadStandardString(int length)
        {
            return rf.ReadString(length, "windows-1252");
        }

        protected virtual string ReadUnicodeString(int length)
        {
            StringBuilder stringBuilder = new StringBuilder();
            length /= 2;
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(rf.ReadChar());
            }

            return stringBuilder.ToString();
        }

        protected virtual void ReadGlyphWidths()
        {
            tables.TryGetValue("hmtx", out var value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "hmtx", fileName + style));
            }

            rf.Seek(value[0]);
            glyphWidthsByIndex = new int[hhea.numberOfHMetrics];
            for (int i = 0; i < hhea.numberOfHMetrics; i++)
            {
                glyphWidthsByIndex[i] = rf.ReadUnsignedShort() * 1000 / head.unitsPerEm;
                _ = rf.ReadShort() * 1000 / head.unitsPerEm;
            }
        }

        protected internal virtual int GetGlyphWidth(int glyph)
        {
            if (glyph >= glyphWidthsByIndex.Length)
            {
                glyph = glyphWidthsByIndex.Length - 1;
            }

            return glyphWidthsByIndex[glyph];
        }

        private void ReadBbox()
        {
            tables.TryGetValue("head", out var value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "head", fileName + style));
            }

            rf.Seek(value[0] + TrueTypeFontSubSet.HEAD_LOCA_FORMAT_OFFSET);
            bool flag = rf.ReadUnsignedShort() == 0;
            tables.TryGetValue("loca", out value);
            if (value == null)
            {
                return;
            }

            rf.Seek(value[0]);
            int[] array;
            if (flag)
            {
                int num = value[1] / 2;
                array = new int[num];
                for (int i = 0; i < num; i++)
                {
                    array[i] = rf.ReadUnsignedShort() * 2;
                }
            }
            else
            {
                int num2 = value[1] / 4;
                array = new int[num2];
                for (int j = 0; j < num2; j++)
                {
                    array[j] = rf.ReadInt();
                }
            }

            tables.TryGetValue("glyf", out value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "glyf", fileName + style));
            }

            int num3 = value[0];
            bboxes = new int[array.Length - 1][];
            for (int k = 0; k < array.Length - 1; k++)
            {
                int num4 = array[k];
                if (num4 != array[k + 1])
                {
                    rf.Seek(num3 + num4 + 2);
                    bboxes[k] = new int[4]
                    {
                        rf.ReadShort() * 1000 / head.unitsPerEm,
                        rf.ReadShort() * 1000 / head.unitsPerEm,
                        rf.ReadShort() * 1000 / head.unitsPerEm,
                        rf.ReadShort() * 1000 / head.unitsPerEm
                    };
                }
            }
        }

        internal void ReadCMaps()
        {
            tables.TryGetValue("cmap", out var value);
            if (value == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("table.1.does.not.exist.in.2", "cmap", fileName + style));
            }

            rf.Seek(value[0]);
            rf.SkipBytes(2L);
            int num = rf.ReadUnsignedShort();
            fontSpecific = false;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            for (int i = 0; i < num; i++)
            {
                int num6 = rf.ReadUnsignedShort();
                int num7 = rf.ReadUnsignedShort();
                int num8 = rf.ReadInt();
                if (num6 == 3 && num7 == 0)
                {
                    fontSpecific = true;
                    num4 = num8;
                }
                else if (num6 == 3 && num7 == 1)
                {
                    num3 = num8;
                }
                else if (num6 == 3 && num7 == 10)
                {
                    num5 = num8;
                }

                if (num6 == 1 && num7 == 0)
                {
                    num2 = num8;
                }
            }

            if (num2 > 0)
            {
                rf.Seek(value[0] + num2);
                switch (rf.ReadUnsignedShort())
                {
                    case 0:
                        cmap10 = ReadFormat0();
                        break;
                    case 4:
                        cmap10 = ReadFormat4();
                        break;
                    case 6:
                        cmap10 = ReadFormat6();
                        break;
                }
            }

            if (num3 > 0)
            {
                rf.Seek(value[0] + num3);
                if (rf.ReadUnsignedShort() == 4)
                {
                    cmap31 = ReadFormat4();
                }
            }

            if (num4 > 0)
            {
                rf.Seek(value[0] + num4);
                if (rf.ReadUnsignedShort() == 4)
                {
                    cmap10 = ReadFormat4();
                }
            }

            if (num5 > 0)
            {
                rf.Seek(value[0] + num5);
                switch (rf.ReadUnsignedShort())
                {
                    case 0:
                        cmapExt = ReadFormat0();
                        break;
                    case 4:
                        cmapExt = ReadFormat4();
                        break;
                    case 6:
                        cmapExt = ReadFormat6();
                        break;
                    case 12:
                        cmapExt = ReadFormat12();
                        break;
                }
            }
        }

        internal Dictionary<int, int[]> ReadFormat12()
        {
            Dictionary<int, int[]> dictionary = new Dictionary<int, int[]>();
            rf.SkipBytes(2L);
            rf.ReadInt();
            rf.SkipBytes(4L);
            int num = rf.ReadInt();
            for (int i = 0; i < num; i++)
            {
                int num2 = rf.ReadInt();
                int num3 = rf.ReadInt();
                int num4 = rf.ReadInt();
                for (int j = num2; j <= num3; j++)
                {
                    int[] array = new int[2];
                    array[0] = num4;
                    array[1] = GetGlyphWidth(array[0]);
                    dictionary[j] = array;
                    num4++;
                }
            }

            return dictionary;
        }

        internal Dictionary<int, int[]> ReadFormat0()
        {
            Dictionary<int, int[]> dictionary = new Dictionary<int, int[]>();
            rf.SkipBytes(4L);
            for (int i = 0; i < 256; i++)
            {
                int[] array = new int[2];
                array[0] = rf.ReadUnsignedByte();
                array[1] = GetGlyphWidth(array[0]);
                dictionary[i] = array;
            }

            return dictionary;
        }

        internal Dictionary<int, int[]> ReadFormat4()
        {
            Dictionary<int, int[]> dictionary = new Dictionary<int, int[]>();
            int num = rf.ReadUnsignedShort();
            rf.SkipBytes(2L);
            int num2 = rf.ReadUnsignedShort() / 2;
            rf.SkipBytes(6L);
            int[] array = new int[num2];
            for (int i = 0; i < num2; i++)
            {
                array[i] = rf.ReadUnsignedShort();
            }

            rf.SkipBytes(2L);
            int[] array2 = new int[num2];
            for (int j = 0; j < num2; j++)
            {
                array2[j] = rf.ReadUnsignedShort();
            }

            int[] array3 = new int[num2];
            for (int k = 0; k < num2; k++)
            {
                array3[k] = rf.ReadUnsignedShort();
            }

            int[] array4 = new int[num2];
            for (int l = 0; l < num2; l++)
            {
                array4[l] = rf.ReadUnsignedShort();
            }

            int[] array5 = new int[num / 2 - 8 - num2 * 4];
            for (int m = 0; m < array5.Length; m++)
            {
                array5[m] = rf.ReadUnsignedShort();
            }

            for (int n = 0; n < num2; n++)
            {
                for (int num3 = array2[n]; num3 <= array[n] && num3 != 65535; num3++)
                {
                    int num4;
                    if (array4[n] == 0)
                    {
                        num4 = (num3 + array3[n]) & 0xFFFF;
                    }
                    else
                    {
                        int num5 = n + array4[n] / 2 - num2 + num3 - array2[n];
                        if (num5 >= array5.Length)
                        {
                            continue;
                        }

                        num4 = (array5[num5] + array3[n]) & 0xFFFF;
                    }

                    int[] array6 = new int[2];
                    array6[0] = num4;
                    array6[1] = GetGlyphWidth(array6[0]);
                    dictionary[(!fontSpecific) ? num3 : (((num3 & 0xFF00) == 61440) ? (num3 & 0xFF) : num3)] = array6;
                }
            }

            return dictionary;
        }

        internal Dictionary<int, int[]> ReadFormat6()
        {
            Dictionary<int, int[]> dictionary = new Dictionary<int, int[]>();
            rf.SkipBytes(4L);
            int num = rf.ReadUnsignedShort();
            int num2 = rf.ReadUnsignedShort();
            for (int i = 0; i < num2; i++)
            {
                int[] array = new int[2];
                array[0] = rf.ReadUnsignedShort();
                array[1] = GetGlyphWidth(array[0]);
                dictionary[i + num] = array;
            }

            return dictionary;
        }

        internal void ReadKerning()
        {
            tables.TryGetValue("kern", out var value);
            if (value == null)
            {
                return;
            }

            rf.Seek(value[0] + 2);
            int num = rf.ReadUnsignedShort();
            int num2 = value[0] + 4;
            int num3 = 0;
            for (int i = 0; i < num; i++)
            {
                num2 += num3;
                rf.Seek(num2);
                rf.SkipBytes(2L);
                num3 = rf.ReadUnsignedShort();
                if ((rf.ReadUnsignedShort() & 0xFFF7) == 1)
                {
                    int num4 = rf.ReadUnsignedShort();
                    rf.SkipBytes(6L);
                    for (int j = 0; j < num4; j++)
                    {
                        int key = rf.ReadInt();
                        int value2 = rf.ReadShort() * 1000 / head.unitsPerEm;
                        kerning[key] = value2;
                    }
                }
            }
        }

        public override int GetKerning(int char1, int char2)
        {
            int[] metricsTT = GetMetricsTT(char1);
            if (metricsTT == null)
            {
                return 0;
            }

            int num = metricsTT[0];
            metricsTT = GetMetricsTT(char2);
            if (metricsTT == null)
            {
                return 0;
            }

            int num2 = metricsTT[0];
            return kerning[(num << 16) + num2];
        }

        internal override int GetRawWidth(int c, string name)
        {
            int[] metricsTT = GetMetricsTT(c);
            if (metricsTT == null)
            {
                return 0;
            }

            return metricsTT[1];
        }

        public virtual PdfDictionary GetFontDescriptor(PdfIndirectReference fontStream, string subsetPrefix, PdfIndirectReference cidset)
        {
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONTDESCRIPTOR);
            pdfDictionary.Put(PdfName.ASCENT, new PdfNumber(os_2.sTypoAscender * 1000 / head.unitsPerEm));
            pdfDictionary.Put(PdfName.CAPHEIGHT, new PdfNumber(os_2.sCapHeight * 1000 / head.unitsPerEm));
            pdfDictionary.Put(PdfName.DESCENT, new PdfNumber(os_2.sTypoDescender * 1000 / head.unitsPerEm));
            pdfDictionary.Put(PdfName.FONTBBOX, new PdfRectangle(head.xMin * 1000 / head.unitsPerEm, head.yMin * 1000 / head.unitsPerEm, head.xMax * 1000 / head.unitsPerEm, head.yMax * 1000 / head.unitsPerEm));
            if (cidset != null)
            {
                pdfDictionary.Put(PdfName.CIDSET, cidset);
            }

            if (cff)
            {
                if (encoding.StartsWith("Identity-"))
                {
                    pdfDictionary.Put(PdfName.FONTNAME, new PdfName(subsetPrefix + fontName + "-" + encoding));
                }
                else
                {
                    pdfDictionary.Put(PdfName.FONTNAME, new PdfName(subsetPrefix + fontName + style));
                }
            }
            else
            {
                pdfDictionary.Put(PdfName.FONTNAME, new PdfName(subsetPrefix + fontName + style));
            }

            pdfDictionary.Put(PdfName.ITALICANGLE, new PdfNumber(italicAngle));
            pdfDictionary.Put(PdfName.STEMV, new PdfNumber(80));
            if (fontStream != null)
            {
                if (cff)
                {
                    pdfDictionary.Put(PdfName.FONTFILE3, fontStream);
                }
                else
                {
                    pdfDictionary.Put(PdfName.FONTFILE2, fontStream);
                }
            }

            int num = 0;
            if (isFixedPitch)
            {
                num |= 1;
            }

            num |= (fontSpecific ? 4 : 32);
            if (((uint)head.macStyle & 2u) != 0)
            {
                num |= 0x40;
            }

            if (((uint)head.macStyle & (true ? 1u : 0u)) != 0)
            {
                num |= 0x40000;
            }

            pdfDictionary.Put(PdfName.FLAGS, new PdfNumber(num));
            return pdfDictionary;
        }

        protected virtual PdfDictionary GetFontBaseType(PdfIndirectReference fontDescriptor, string subsetPrefix, int firstChar, int lastChar, byte[] shortTag)
        {
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONT);
            if (cff)
            {
                pdfDictionary.Put(PdfName.SUBTYPE, PdfName.TYPE1);
                pdfDictionary.Put(PdfName.BASEFONT, new PdfName(fontName + style));
            }
            else
            {
                pdfDictionary.Put(PdfName.SUBTYPE, PdfName.TRUETYPE);
                pdfDictionary.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName + style));
            }

            pdfDictionary.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName + style));
            if (!fontSpecific)
            {
                for (int i = firstChar; i <= lastChar; i++)
                {
                    if (!differences[i].Equals(".notdef"))
                    {
                        firstChar = i;
                        break;
                    }
                }

                if (encoding.Equals("Cp1252") || encoding.Equals("MacRoman"))
                {
                    pdfDictionary.Put(PdfName.ENCODING, encoding.Equals("Cp1252") ? PdfName.WIN_ANSI_ENCODING : PdfName.MAC_ROMAN_ENCODING);
                }
                else
                {
                    PdfDictionary pdfDictionary2 = new PdfDictionary(PdfName.ENCODING);
                    PdfArray pdfArray = new PdfArray();
                    bool flag = true;
                    for (int j = firstChar; j <= lastChar; j++)
                    {
                        if (shortTag[j] != 0)
                        {
                            if (flag)
                            {
                                pdfArray.Add(new PdfNumber(j));
                                flag = false;
                            }

                            pdfArray.Add(new PdfName(differences[j]));
                        }
                        else
                        {
                            flag = true;
                        }
                    }

                    pdfDictionary2.Put(PdfName.DIFFERENCES, pdfArray);
                    pdfDictionary.Put(PdfName.ENCODING, pdfDictionary2);
                }
            }

            pdfDictionary.Put(PdfName.FIRSTCHAR, new PdfNumber(firstChar));
            pdfDictionary.Put(PdfName.LASTCHAR, new PdfNumber(lastChar));
            PdfArray pdfArray2 = new PdfArray();
            for (int k = firstChar; k <= lastChar; k++)
            {
                if (shortTag[k] == 0)
                {
                    pdfArray2.Add(new PdfNumber(0));
                }
                else
                {
                    pdfArray2.Add(new PdfNumber(widths[k]));
                }
            }

            pdfDictionary.Put(PdfName.WIDTHS, pdfArray2);
            if (fontDescriptor != null)
            {
                pdfDictionary.Put(PdfName.FONTDESCRIPTOR, fontDescriptor);
            }

            return pdfDictionary;
        }

        public virtual byte[] GetFullFont()
        {
            lock (head)
            {
                RandomAccessFileOrArray randomAccessFileOrArray = null;
                try
                {
                    randomAccessFileOrArray = new RandomAccessFileOrArray(rf);
                    randomAccessFileOrArray.ReOpen();
                    byte[] array = new byte[randomAccessFileOrArray.Length];
                    randomAccessFileOrArray.ReadFully(array);
                    return array;
                }
                finally
                {
                    try
                    {
                        randomAccessFileOrArray?.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        protected internal virtual byte[] GetSubSet(HashSet2<int> glyphs, bool subsetp)
        {
            lock (head)
            {
                return new TrueTypeFontSubSet(fileName, new RandomAccessFileOrArray(rf), glyphs, directoryOffset, includeCmap: true, !subsetp).Process();
            }
        }

        protected static int[] CompactRanges(List<int[]> ranges)
        {
            List<int[]> list = new List<int[]>();
            for (int i = 0; i < ranges.Count; i++)
            {
                int[] array = ranges[i];
                for (int j = 0; j < array.Length; j += 2)
                {
                    list.Add(new int[2]
                    {
                        Math.Max(0, Math.Min(array[j], array[j + 1])),
                        Math.Min(65535, Math.Max(array[j], array[j + 1]))
                    });
                }
            }

            for (int k = 0; k < list.Count - 1; k++)
            {
                for (int l = k + 1; l < list.Count; l++)
                {
                    int[] array2 = list[k];
                    int[] array3 = list[l];
                    if ((array2[0] >= array3[0] && array2[0] <= array3[1]) || (array2[1] >= array3[0] && array2[0] <= array3[1]))
                    {
                        array2[0] = Math.Min(array2[0], array3[0]);
                        array2[1] = Math.Max(array2[1], array3[1]);
                        list.RemoveAt(l);
                        l--;
                    }
                }
            }

            int[] array4 = new int[list.Count * 2];
            for (int m = 0; m < list.Count; m++)
            {
                int[] array5 = list[m];
                array4[m * 2] = array5[0];
                array4[m * 2 + 1] = array5[1];
            }

            return array4;
        }

        public virtual void AddRangeUni(Dictionary<int, int[]> longTag, bool includeMetrics, bool subsetp)
        {
            if (subsetp || (subsetRanges == null && directoryOffset <= 0))
            {
                return;
            }

            int[] array = ((subsetRanges != null || directoryOffset <= 0) ? CompactRanges(subsetRanges) : new int[2] { 0, 65535 });
            Dictionary<int, int[]> dictionary = ((!fontSpecific && cmap31 != null) ? cmap31 : ((fontSpecific && cmap10 != null) ? cmap10 : ((cmap31 == null) ? cmap10 : cmap31)));
            foreach (KeyValuePair<int, int[]> item in dictionary)
            {
                int[] value = item.Value;
                int key = value[0];
                if (longTag.ContainsKey(key))
                {
                    continue;
                }

                int key2 = item.Key;
                bool flag = true;
                for (int i = 0; i < array.Length; i += 2)
                {
                    if (key2 >= array[i] && key2 <= array[i + 1])
                    {
                        flag = false;
                        break;
                    }
                }

                if (!flag)
                {
                    longTag[key] = ((!includeMetrics) ? null : new int[3]
                    {
                        value[0],
                        value[1],
                        key2
                    });
                }
            }
        }

        internal override void WriteFont(PdfWriter writer, PdfIndirectReference piref, object[] parms)
        {
            int num = (int)parms[0];
            int num2 = (int)parms[1];
            byte[] array = (byte[])parms[2];
            bool flag = (bool)parms[3] && subset;
            if (!flag)
            {
                num = 0;
                num2 = array.Length - 1;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = 1;
                }
            }

            PdfIndirectReference pdfIndirectReference = null;
            PdfObject pdfObject = null;
            string subsetPrefix = "";
            if (embedded)
            {
                if (cff)
                {
                    pdfObject = new StreamFont(ReadCffFont(), "Type1C", compressionLevel);
                    pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
                }
                else
                {
                    if (flag)
                    {
                        subsetPrefix = Sign.itext.pdf.BaseFont.CreateSubsetPrefix();
                    }

                    Dictionary<int, int[]> dictionary = new Dictionary<int, int[]>();
                    for (int j = num; j <= num2; j++)
                    {
                        if (array[j] == 0)
                        {
                            continue;
                        }

                        int[] array2 = null;
                        if (specialMap == null)
                        {
                            array2 = ((!fontSpecific) ? GetMetricsTT(unicodeDifferences[j]) : GetMetricsTT(j));
                        }
                        else
                        {
                            int[] array3 = GlyphList.NameToUnicode(differences[j]);
                            if (array3 != null)
                            {
                                array2 = GetMetricsTT(array3[0]);
                            }
                        }

                        if (array2 != null)
                        {
                            dictionary[array2[0]] = null;
                        }
                    }

                    AddRangeUni(new Dictionary<int, int[]>(dictionary), includeMetrics: false, flag);
                    byte[] array4 = null;
                    array4 = ((!flag && directoryOffset == 0 && subsetRanges == null) ? GetFullFont() : GetSubSet(new HashSet2<int>(dictionary.Keys), flag));
                    int[] lengths = new int[1] { array4.Length };
                    pdfObject = new StreamFont(array4, lengths, compressionLevel);
                    pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
                }
            }

            pdfObject = GetFontDescriptor(pdfIndirectReference, subsetPrefix, null);
            if (pdfObject != null)
            {
                pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
            }

            pdfObject = GetFontBaseType(pdfIndirectReference, subsetPrefix, num, num2, array);
            writer.AddToBody(pdfObject, piref);
        }

        public virtual byte[] ReadCffFont()
        {
            RandomAccessFileOrArray randomAccessFileOrArray = new RandomAccessFileOrArray(rf);
            byte[] array = new byte[cffLength];
            try
            {
                randomAccessFileOrArray.ReOpen();
                randomAccessFileOrArray.Seek(cffOffset);
                randomAccessFileOrArray.ReadFully(array);
                return array;
            }
            finally
            {
                try
                {
                    randomAccessFileOrArray.Close();
                }
                catch
                {
                }
            }
        }

        public override PdfStream GetFullFontStream()
        {
            if (cff)
            {
                return new StreamFont(ReadCffFont(), "Type1C", compressionLevel);
            }

            byte[] fullFont = GetFullFont();
            int[] lengths = new int[1] { fullFont.Length };
            return new StreamFont(fullFont, lengths, compressionLevel);
        }

        public override float GetFontDescriptor(int key, float fontSize)
        {
            return key switch
            {
                1 => (float)os_2.sTypoAscender * fontSize / (float)head.unitsPerEm,
                2 => (float)os_2.sCapHeight * fontSize / (float)head.unitsPerEm,
                3 => (float)os_2.sTypoDescender * fontSize / (float)head.unitsPerEm,
                4 => (float)italicAngle,
                5 => fontSize * (float)head.xMin / (float)head.unitsPerEm,
                6 => fontSize * (float)head.yMin / (float)head.unitsPerEm,
                7 => fontSize * (float)head.xMax / (float)head.unitsPerEm,
                8 => fontSize * (float)head.yMax / (float)head.unitsPerEm,
                9 => fontSize * (float)hhea.Ascender / (float)head.unitsPerEm,
                10 => fontSize * (float)hhea.Descender / (float)head.unitsPerEm,
                11 => fontSize * (float)hhea.LineGap / (float)head.unitsPerEm,
                12 => fontSize * (float)hhea.advanceWidthMax / (float)head.unitsPerEm,
                13 => (float)(underlinePosition - underlineThickness / 2) * fontSize / (float)head.unitsPerEm,
                14 => (float)underlineThickness * fontSize / (float)head.unitsPerEm,
                15 => (float)os_2.yStrikeoutPosition * fontSize / (float)head.unitsPerEm,
                16 => (float)os_2.yStrikeoutSize * fontSize / (float)head.unitsPerEm,
                17 => (float)os_2.ySubscriptYSize * fontSize / (float)head.unitsPerEm,
                18 => (float)(-os_2.ySubscriptYOffset) * fontSize / (float)head.unitsPerEm,
                19 => (float)os_2.ySuperscriptYSize * fontSize / (float)head.unitsPerEm,
                20 => (float)os_2.ySuperscriptYOffset * fontSize / (float)head.unitsPerEm,
                21 => os_2.usWeightClass,
                22 => os_2.usWidthClass,
                _ => 0f,
            };
        }

        public virtual int[] GetMetricsTT(int c)
        {
            int[] value = null;
            if (cmapExt != null)
            {
                cmapExt.TryGetValue(c, out value);
            }
            else if (!fontSpecific && cmap31 != null)
            {
                cmap31.TryGetValue(c, out value);
            }
            else if (fontSpecific && cmap10 != null)
            {
                cmap10.TryGetValue(c, out value);
            }
            else if (cmap31 != null)
            {
                cmap31.TryGetValue(c, out value);
            }
            else if (cmap10 != null)
            {
                cmap10.TryGetValue(c, out value);
            }

            return value;
        }

        public override bool HasKernPairs()
        {
            return kerning.Size > 0;
        }

        public override bool SetKerning(int char1, int char2, int kern)
        {
            int[] metricsTT = GetMetricsTT(char1);
            if (metricsTT == null)
            {
                return false;
            }

            int num = metricsTT[0];
            metricsTT = GetMetricsTT(char2);
            if (metricsTT == null)
            {
                return false;
            }

            int num2 = metricsTT[0];
            kerning[(num << 16) + num2] = kern;
            return true;
        }

        protected override int[] GetRawCharBBox(int c, string name)
        {
            Dictionary<int, int[]> dictionary = null;
            dictionary = ((name != null && cmap31 != null) ? cmap31 : cmap10);
            if (dictionary == null)
            {
                return null;
            }

            dictionary.TryGetValue(c, out var value);
            if (value == null || bboxes == null)
            {
                return null;
            }

            return bboxes[value[0]];
        }
    }
}
