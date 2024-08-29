using Sign.itext.pdf;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class CFFFont
    {
        protected internal abstract class Item
        {
            protected internal int myOffset = -1;

            public virtual void Increment(int[] currentOffset)
            {
                myOffset = currentOffset[0];
            }

            public virtual void Emit(byte[] buffer)
            {
            }

            public virtual void Xref()
            {
            }
        }

        protected internal abstract class OffsetItem : Item
        {
            public int value;

            public virtual void Set(int offset)
            {
                value = offset;
            }
        }

        protected internal class RangeItem : Item
        {
            public int offset;

            public int length;

            private RandomAccessFileOrArray buf;

            public RangeItem(RandomAccessFileOrArray buf, int offset, int length)
            {
                this.offset = offset;
                this.length = length;
                this.buf = buf;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0] += length;
            }

            public override void Emit(byte[] buffer)
            {
                buf.Seek(offset);
                for (int i = myOffset; i < myOffset + length; i++)
                {
                    buffer[i] = buf.ReadByte();
                }
            }
        }

        protected internal class IndexOffsetItem : OffsetItem
        {
            public int size;

            public IndexOffsetItem(int size, int value)
            {
                this.size = size;
                base.value = value;
            }

            public IndexOffsetItem(int size)
            {
                this.size = size;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0] += size;
            }

            public override void Emit(byte[] buffer)
            {
                int num = 0;
                switch (size)
                {
                    default:
                        return;
                    case 4:
                        buffer[myOffset + num] = (byte)((uint)(value >> 24) & 0xFFu);
                        num++;
                        goto case 3;
                    case 3:
                        buffer[myOffset + num] = (byte)((uint)(value >> 16) & 0xFFu);
                        num++;
                        goto case 2;
                    case 2:
                        buffer[myOffset + num] = (byte)((uint)(value >> 8) & 0xFFu);
                        num++;
                        break;
                    case 1:
                        break;
                }

                buffer[myOffset + num] = (byte)((uint)value & 0xFFu);
                num++;
            }
        }

        protected internal class IndexBaseItem : Item
        {
        }

        protected internal class IndexMarkerItem : Item
        {
            private OffsetItem offItem;

            private IndexBaseItem indexBase;

            public IndexMarkerItem(OffsetItem offItem, IndexBaseItem indexBase)
            {
                this.offItem = offItem;
                this.indexBase = indexBase;
            }

            public override void Xref()
            {
                offItem.Set(myOffset - indexBase.myOffset + 1);
            }
        }

        protected internal class SubrMarkerItem : Item
        {
            private OffsetItem offItem;

            private IndexBaseItem indexBase;

            public SubrMarkerItem(OffsetItem offItem, IndexBaseItem indexBase)
            {
                this.offItem = offItem;
                this.indexBase = indexBase;
            }

            public override void Xref()
            {
                offItem.Set(myOffset - indexBase.myOffset);
            }
        }

        protected internal class DictOffsetItem : OffsetItem
        {
            public int size;

            public DictOffsetItem()
            {
                size = 5;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0] += size;
            }

            public override void Emit(byte[] buffer)
            {
                if (size == 5)
                {
                    buffer[myOffset] = 29;
                    buffer[myOffset + 1] = (byte)((uint)(value >> 24) & 0xFFu);
                    buffer[myOffset + 2] = (byte)((uint)(value >> 16) & 0xFFu);
                    buffer[myOffset + 3] = (byte)((uint)(value >> 8) & 0xFFu);
                    buffer[myOffset + 4] = (byte)((uint)value & 0xFFu);
                }
            }
        }

        protected internal class UInt24Item : Item
        {
            public int value;

            public UInt24Item(int value)
            {
                this.value = value;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0] += 3;
            }

            public override void Emit(byte[] buffer)
            {
                buffer[myOffset] = (byte)((uint)(value >> 16) & 0xFFu);
                buffer[myOffset + 1] = (byte)((uint)(value >> 8) & 0xFFu);
                buffer[myOffset + 2] = (byte)((uint)value & 0xFFu);
            }
        }

        protected internal class UInt32Item : Item
        {
            public int value;

            public UInt32Item(int value)
            {
                this.value = value;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0] += 4;
            }

            public override void Emit(byte[] buffer)
            {
                buffer[myOffset] = (byte)((uint)(value >> 24) & 0xFFu);
                buffer[myOffset + 1] = (byte)((uint)(value >> 16) & 0xFFu);
                buffer[myOffset + 2] = (byte)((uint)(value >> 8) & 0xFFu);
                buffer[myOffset + 3] = (byte)((uint)value & 0xFFu);
            }
        }

        protected internal class UInt16Item : Item
        {
            public char value;

            public UInt16Item(char value)
            {
                this.value = value;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0] += 2;
            }

            public override void Emit(byte[] buffer)
            {
                buffer[myOffset] = (byte)((uint)((int)value >> 8) & 0xFFu);
                buffer[myOffset + 1] = (byte)(value & 0xFFu);
            }
        }

        protected internal class UInt8Item : Item
        {
            public char value;

            public UInt8Item(char value)
            {
                this.value = value;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0]++;
            }

            public override void Emit(byte[] buffer)
            {
                buffer[myOffset] = (byte)(value & 0xFFu);
            }
        }

        protected internal class StringItem : Item
        {
            public string s;

            public StringItem(string s)
            {
                this.s = s;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0] += s.Length;
            }

            public override void Emit(byte[] buffer)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    buffer[myOffset + i] = (byte)(s[i] & 0xFFu);
                }
            }
        }

        protected internal class DictNumberItem : Item
        {
            public int value;

            public int size = 5;

            public DictNumberItem(int value)
            {
                this.value = value;
            }

            public override void Increment(int[] currentOffset)
            {
                base.Increment(currentOffset);
                currentOffset[0] += size;
            }

            public override void Emit(byte[] buffer)
            {
                if (size == 5)
                {
                    buffer[myOffset] = 29;
                    buffer[myOffset + 1] = (byte)((uint)(value >> 24) & 0xFFu);
                    buffer[myOffset + 2] = (byte)((uint)(value >> 16) & 0xFFu);
                    buffer[myOffset + 3] = (byte)((uint)(value >> 8) & 0xFFu);
                    buffer[myOffset + 4] = (byte)((uint)value & 0xFFu);
                }
            }
        }

        protected internal class MarkerItem : Item
        {
            private OffsetItem p;

            public MarkerItem(OffsetItem pointerToMarker)
            {
                p = pointerToMarker;
            }

            public override void Xref()
            {
                p.Set(myOffset);
            }
        }

        protected internal class Font
        {
            public string name;

            public string fullName;

            public bool isCID;

            public int privateOffset = -1;

            public int privateLength = -1;

            public int privateSubrs = -1;

            public int charstringsOffset = -1;

            public int encodingOffset = -1;

            public int charsetOffset = -1;

            public int fdarrayOffset = -1;

            public int fdselectOffset = -1;

            public int[] fdprivateOffsets;

            public int[] fdprivateLengths;

            public int[] fdprivateSubrs;

            public int nglyphs;

            public int nstrings;

            public int CharsetLength;

            public int[] charstringsOffsets;

            public int[] charset;

            public int[] FDSelect;

            public int FDSelectLength;

            public int FDSelectFormat;

            public int CharstringType = 2;

            public int FDArrayCount;

            public int FDArrayOffsize;

            public int[] FDArrayOffsets;

            public int[] PrivateSubrsOffset;

            public int[][] PrivateSubrsOffsetsArray;

            public int[] SubrsOffsets;
        }

        internal static string[] operatorNames = new string[71]
        {
            "version", "Notice", "FullName", "FamilyName", "Weight", "FontBBox", "BlueValues", "OtherBlues", "FamilyBlues", "FamilyOtherBlues",
            "StdHW", "StdVW", "UNKNOWN_12", "UniqueID", "XUID", "charset", "Encoding", "CharStrings", "Private", "Subrs",
            "defaultWidthX", "nominalWidthX", "UNKNOWN_22", "UNKNOWN_23", "UNKNOWN_24", "UNKNOWN_25", "UNKNOWN_26", "UNKNOWN_27", "UNKNOWN_28", "UNKNOWN_29",
            "UNKNOWN_30", "UNKNOWN_31", "Copyright", "isFixedPitch", "ItalicAngle", "UnderlinePosition", "UnderlineThickness", "PaintType", "CharstringType", "FontMatrix",
            "StrokeWidth", "BlueScale", "BlueShift", "BlueFuzz", "StemSnapH", "StemSnapV", "ForceBold", "UNKNOWN_12_15", "UNKNOWN_12_16", "LanguageGroup",
            "ExpansionFactor", "initialRandomSeed", "SyntheticBase", "PostScript", "BaseFontName", "BaseFontBlend", "UNKNOWN_12_24", "UNKNOWN_12_25", "UNKNOWN_12_26", "UNKNOWN_12_27",
            "UNKNOWN_12_28", "UNKNOWN_12_29", "ROS", "CIDFontVersion", "CIDFontRevision", "CIDFontType", "CIDCount", "UIDBase", "FDArray", "FDSelect",
            "FontName"
        };

        internal static string[] standardStrings = new string[391]
        {
            ".notdef", "space", "exclam", "quotedbl", "numbersign", "dollar", "percent", "ampersand", "quoteright", "parenleft",
            "parenright", "asterisk", "plus", "comma", "hyphen", "period", "slash", "zero", "one", "two",
            "three", "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon", "less",
            "equal", "greater", "question", "at", "A", "B", "C", "D", "E", "F",
            "G", "H", "I", "J", "K", "L", "M", "N", "O", "P",
            "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "bracketleft", "backslash", "bracketright", "asciicircum", "underscore", "quoteleft", "a", "b", "c", "d",
            "e", "f", "g", "h", "i", "j", "k", "l", "m", "n",
            "o", "p", "q", "r", "s", "t", "u", "v", "w", "x",
            "y", "z", "braceleft", "bar", "braceright", "asciitilde", "exclamdown", "cent", "sterling", "fraction",
            "yen", "florin", "section", "currency", "quotesingle", "quotedblleft", "guillemotleft", "guilsinglleft", "guilsinglright", "fi",
            "fl", "endash", "dagger", "daggerdbl", "periodcentered", "paragraph", "bullet", "quotesinglbase", "quotedblbase", "quotedblright",
            "guillemotright", "ellipsis", "perthousand", "questiondown", "grave", "acute", "circumflex", "tilde", "macron", "breve",
            "dotaccent", "dieresis", "ring", "cedilla", "hungarumlaut", "ogonek", "caron", "emdash", "AE", "ordfeminine",
            "Lslash", "Oslash", "OE", "ordmasculine", "ae", "dotlessi", "lslash", "oslash", "oe", "germandbls",
            "onesuperior", "logicalnot", "mu", "trademark", "Eth", "onehalf", "plusminus", "Thorn", "onequarter", "divide",
            "brokenbar", "degree", "thorn", "threequarters", "twosuperior", "registered", "minus", "eth", "multiply", "threesuperior",
            "copyright", "Aacute", "Acircumflex", "Adieresis", "Agrave", "Aring", "Atilde", "Ccedilla", "Eacute", "Ecircumflex",
            "Edieresis", "Egrave", "Iacute", "Icircumflex", "Idieresis", "Igrave", "Ntilde", "Oacute", "Ocircumflex", "Odieresis",
            "Ograve", "Otilde", "Scaron", "Uacute", "Ucircumflex", "Udieresis", "Ugrave", "Yacute", "Ydieresis", "Zcaron",
            "aacute", "acircumflex", "adieresis", "agrave", "aring", "atilde", "ccedilla", "eacute", "ecircumflex", "edieresis",
            "egrave", "iacute", "icircumflex", "idieresis", "igrave", "ntilde", "oacute", "ocircumflex", "odieresis", "ograve",
            "otilde", "scaron", "uacute", "ucircumflex", "udieresis", "ugrave", "yacute", "ydieresis", "zcaron", "exclamsmall",
            "Hungarumlautsmall", "dollaroldstyle", "dollarsuperior", "ampersandsmall", "Acutesmall", "parenleftsuperior", "parenrightsuperior", "twodotenleader", "onedotenleader", "zerooldstyle",
            "oneoldstyle", "twooldstyle", "threeoldstyle", "fouroldstyle", "fiveoldstyle", "sixoldstyle", "sevenoldstyle", "eightoldstyle", "nineoldstyle", "commasuperior",
            "threequartersemdash", "periodsuperior", "questionsmall", "asuperior", "bsuperior", "centsuperior", "dsuperior", "esuperior", "isuperior", "lsuperior",
            "msuperior", "nsuperior", "osuperior", "rsuperior", "ssuperior", "tsuperior", "ff", "ffi", "ffl", "parenleftinferior",
            "parenrightinferior", "Circumflexsmall", "hyphensuperior", "Gravesmall", "Asmall", "Bsmall", "Csmall", "Dsmall", "Esmall", "Fsmall",
            "Gsmall", "Hsmall", "Ismall", "Jsmall", "Ksmall", "Lsmall", "Msmall", "Nsmall", "Osmall", "Psmall",
            "Qsmall", "Rsmall", "Ssmall", "Tsmall", "Usmall", "Vsmall", "Wsmall", "Xsmall", "Ysmall", "Zsmall",
            "colonmonetary", "onefitted", "rupiah", "Tildesmall", "exclamdownsmall", "centoldstyle", "Lslashsmall", "Scaronsmall", "Zcaronsmall", "Dieresissmall",
            "Brevesmall", "Caronsmall", "Dotaccentsmall", "Macronsmall", "figuredash", "hypheninferior", "Ogoneksmall", "Ringsmall", "Cedillasmall", "questiondownsmall",
            "oneeighth", "threeeighths", "fiveeighths", "seveneighths", "onethird", "twothirds", "zerosuperior", "foursuperior", "fivesuperior", "sixsuperior",
            "sevensuperior", "eightsuperior", "ninesuperior", "zeroinferior", "oneinferior", "twoinferior", "threeinferior", "fourinferior", "fiveinferior", "sixinferior",
            "seveninferior", "eightinferior", "nineinferior", "centinferior", "dollarinferior", "periodinferior", "commainferior", "Agravesmall", "Aacutesmall", "Acircumflexsmall",
            "Atildesmall", "Adieresissmall", "Aringsmall", "AEsmall", "Ccedillasmall", "Egravesmall", "Eacutesmall", "Ecircumflexsmall", "Edieresissmall", "Igravesmall",
            "Iacutesmall", "Icircumflexsmall", "Idieresissmall", "Ethsmall", "Ntildesmall", "Ogravesmall", "Oacutesmall", "Ocircumflexsmall", "Otildesmall", "Odieresissmall",
            "OEsmall", "Oslashsmall", "Ugravesmall", "Uacutesmall", "Ucircumflexsmall", "Udieresissmall", "Yacutesmall", "Thornsmall", "Ydieresissmall", "001.000",
            "001.001", "001.002", "001.003", "Black", "Bold", "Book", "Light", "Medium", "Regular", "Roman",
            "Semibold"
        };

        internal int nextIndexOffset;

        protected string key;

        protected object[] args = new object[48];

        protected int arg_count;

        protected RandomAccessFileOrArray buf;

        private int offSize;

        protected int nameIndexOffset;

        protected int topdictIndexOffset;

        protected int stringIndexOffset;

        protected int gsubrIndexOffset;

        protected int[] nameOffsets;

        protected int[] topdictOffsets;

        protected int[] stringOffsets;

        protected int[] gsubrOffsets;

        protected Font[] fonts;

        public virtual string GetString(char sid)
        {
            if (sid < standardStrings.Length)
            {
                return standardStrings[(uint)sid];
            }

            if (sid >= standardStrings.Length + (stringOffsets.Length - 1))
            {
                return null;
            }

            int num = sid - standardStrings.Length;
            int position = GetPosition();
            Seek(stringOffsets[num]);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = stringOffsets[num]; i < stringOffsets[num + 1]; i++)
            {
                stringBuilder.Append(GetCard8());
            }

            Seek(position);
            return stringBuilder.ToString();
        }

        internal char GetCard8()
        {
            return (char)(buf.ReadByte() & 0xFFu);
        }

        internal char GetCard16()
        {
            return buf.ReadChar();
        }

        internal int GetOffset(int offSize)
        {
            int num = 0;
            for (int i = 0; i < offSize; i++)
            {
                num *= 256;
                num += GetCard8();
            }

            return num;
        }

        internal void Seek(int offset)
        {
            buf.Seek(offset);
        }

        internal short GetShort()
        {
            return buf.ReadShort();
        }

        internal int GetInt()
        {
            return buf.ReadInt();
        }

        internal int GetPosition()
        {
            return (int)buf.FilePointer;
        }

        internal int[] GetIndex(int nextIndexOffset)
        {
            Seek(nextIndexOffset);
            int card = GetCard16();
            int[] array = new int[card + 1];
            if (card == 0)
            {
                array[0] = -1;
                nextIndexOffset += 2;
                return array;
            }

            int card2 = GetCard8();
            for (int i = 0; i <= card; i++)
            {
                array[i] = nextIndexOffset + 2 + 1 + (card + 1) * card2 - 1 + GetOffset(card2);
            }

            return array;
        }

        protected virtual void GetDictItem()
        {
            for (int i = 0; i < arg_count; i++)
            {
                args[i] = null;
            }

            arg_count = 0;
            key = null;
            bool flag = false;
            while (!flag)
            {
                char card = GetCard8();
                if (card == '\u001d')
                {
                    int @int = GetInt();
                    args[arg_count] = @int;
                    arg_count++;
                }
                else if (card == '\u001c')
                {
                    short @short = GetShort();
                    args[arg_count] = (int)@short;
                    arg_count++;
                }
                else if (card >= ' ' && card <= 'ö')
                {
                    sbyte b = (sbyte)(card - 139);
                    args[arg_count] = (int)b;
                    arg_count++;
                }
                else if (card >= '÷' && card <= 'ú')
                {
                    char card2 = GetCard8();
                    short num = (short)((card - 247) * 256 + card2 + 108);
                    args[arg_count] = (int)num;
                    arg_count++;
                }
                else if (card >= 'û' && card <= 'þ')
                {
                    char card3 = GetCard8();
                    short num2 = (short)(-(card - 251) * 256 - card3 - 108);
                    args[arg_count] = (int)num2;
                    arg_count++;
                }
                else if (card == '\u001e')
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    bool flag2 = false;
                    char c = '\0';
                    byte b2 = 0;
                    int num3 = 0;
                    while (!flag2)
                    {
                        if (b2 == 0)
                        {
                            c = GetCard8();
                            b2 = 2;
                        }

                        if (b2 == 1)
                        {
                            num3 = (int)c / 16;
                            b2 = (byte)(b2 - 1);
                        }

                        if (b2 == 2)
                        {
                            num3 = (int)c % 16;
                            b2 = (byte)(b2 - 1);
                        }

                        switch (num3)
                        {
                            case 10:
                                stringBuilder.Append(".");
                                break;
                            case 11:
                                stringBuilder.Append("E");
                                break;
                            case 12:
                                stringBuilder.Append("E-");
                                break;
                            case 14:
                                stringBuilder.Append("-");
                                break;
                            case 15:
                                flag2 = true;
                                break;
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                stringBuilder.Append(num3.ToString());
                                break;
                            default:
                                stringBuilder.Append("<NIBBLE ERROR: " + num3 + ">");
                                flag2 = true;
                                break;
                        }
                    }

                    args[arg_count] = stringBuilder.ToString();
                    arg_count++;
                }
                else if (card <= '\u0015')
                {
                    flag = true;
                    if (card != '\f')
                    {
                        key = operatorNames[(uint)card];
                    }
                    else
                    {
                        key = operatorNames[32 + GetCard8()];
                    }
                }
            }
        }

        protected virtual RangeItem GetEntireIndexRange(int indexOffset)
        {
            Seek(indexOffset);
            int card = GetCard16();
            if (card == 0)
            {
                return new RangeItem(buf, indexOffset, 2);
            }

            int card2 = GetCard8();
            Seek(indexOffset + 2 + 1 + card * card2);
            int num = GetOffset(card2) - 1;
            return new RangeItem(buf, indexOffset, 3 + (card + 1) * card2 + num);
        }

        public virtual byte[] GetCID(string fontName)
        {
            int i;
            for (i = 0; i < fonts.Length && !fontName.Equals(fonts[i].name); i++)
            {
            }

            if (i == fonts.Length)
            {
                return null;
            }

            List<Item> list = new List<Item>();
            Seek(0);
            GetCard8();
            GetCard8();
            int card = GetCard8();
            GetCard8();
            nextIndexOffset = card;
            list.Add(new RangeItem(buf, 0, card));
            int num = -1;
            int num2 = -1;
            if (!fonts[i].isCID)
            {
                Seek(fonts[i].charstringsOffset);
                num = GetCard16();
                Seek(stringIndexOffset);
                num2 = GetCard16() + standardStrings.Length;
            }

            list.Add(new UInt16Item('\u0001'));
            list.Add(new UInt8Item('\u0001'));
            list.Add(new UInt8Item('\u0001'));
            list.Add(new UInt8Item((char)(1 + fonts[i].name.Length)));
            list.Add(new StringItem(fonts[i].name));
            list.Add(new UInt16Item('\u0001'));
            list.Add(new UInt8Item('\u0002'));
            list.Add(new UInt16Item('\u0001'));
            OffsetItem offsetItem = new IndexOffsetItem(2);
            list.Add(offsetItem);
            IndexBaseItem indexBaseItem = new IndexBaseItem();
            list.Add(indexBaseItem);
            OffsetItem offsetItem2 = new DictOffsetItem();
            OffsetItem offsetItem3 = new DictOffsetItem();
            OffsetItem offsetItem4 = new DictOffsetItem();
            OffsetItem offsetItem5 = new DictOffsetItem();
            if (!fonts[i].isCID)
            {
                list.Add(new DictNumberItem(num2));
                list.Add(new DictNumberItem(num2 + 1));
                list.Add(new DictNumberItem(0));
                list.Add(new UInt8Item('\f'));
                list.Add(new UInt8Item('\u001e'));
                list.Add(new DictNumberItem(num));
                list.Add(new UInt8Item('\f'));
                list.Add(new UInt8Item('"'));
            }

            list.Add(offsetItem4);
            list.Add(new UInt8Item('\f'));
            list.Add(new UInt8Item('$'));
            list.Add(offsetItem5);
            list.Add(new UInt8Item('\f'));
            list.Add(new UInt8Item('%'));
            list.Add(offsetItem2);
            list.Add(new UInt8Item('\u000f'));
            list.Add(offsetItem3);
            list.Add(new UInt8Item('\u0011'));
            Seek(topdictOffsets[i]);
            while (GetPosition() < topdictOffsets[i + 1])
            {
                int position = GetPosition();
                GetDictItem();
                int position2 = GetPosition();
                if (!(key == "Encoding") && !(key == "Private") && !(key == "FDSelect") && !(key == "FDArray") && !(key == "charset") && !(key == "CharStrings"))
                {
                    list.Add(new RangeItem(buf, position, position2 - position));
                }
            }

            list.Add(new IndexMarkerItem(offsetItem, indexBaseItem));
            if (fonts[i].isCID)
            {
                list.Add(GetEntireIndexRange(stringIndexOffset));
            }
            else
            {
                string text = fonts[i].name + "-OneRange";
                if (text.Length > 127)
                {
                    text = text.Substring(0, 127);
                }

                string text2 = "AdobeIdentity" + text;
                int num3 = stringOffsets[stringOffsets.Length - 1] - stringOffsets[0];
                int num4 = stringOffsets[0] - 1;
                byte b = (byte)((num3 + text2.Length <= 255) ? 1 : ((num3 + text2.Length <= 65535) ? 2 : ((num3 + text2.Length > 16777215) ? 4 : 3)));
                list.Add(new UInt16Item((char)(stringOffsets.Length - 1 + 3)));
                list.Add(new UInt8Item((char)b));
                int[] array = stringOffsets;
                foreach (int num5 in array)
                {
                    list.Add(new IndexOffsetItem(b, num5 - num4));
                }

                int num6 = stringOffsets[stringOffsets.Length - 1] - num4;
                num6 += "Adobe".Length;
                list.Add(new IndexOffsetItem(b, num6));
                num6 += "Identity".Length;
                list.Add(new IndexOffsetItem(b, num6));
                num6 += text.Length;
                list.Add(new IndexOffsetItem(b, num6));
                list.Add(new RangeItem(buf, stringOffsets[0], num3));
                list.Add(new StringItem(text2));
            }

            list.Add(GetEntireIndexRange(gsubrIndexOffset));
            if (!fonts[i].isCID)
            {
                list.Add(new MarkerItem(offsetItem5));
                list.Add(new UInt8Item('\u0003'));
                list.Add(new UInt16Item('\u0001'));
                list.Add(new UInt16Item('\0'));
                list.Add(new UInt8Item('\0'));
                list.Add(new UInt16Item((char)num));
                list.Add(new MarkerItem(offsetItem2));
                list.Add(new UInt8Item('\u0002'));
                list.Add(new UInt16Item('\u0001'));
                list.Add(new UInt16Item((char)(num - 1)));
                list.Add(new MarkerItem(offsetItem4));
                list.Add(new UInt16Item('\u0001'));
                list.Add(new UInt8Item('\u0001'));
                list.Add(new UInt8Item('\u0001'));
                OffsetItem offsetItem6 = new IndexOffsetItem(1);
                list.Add(offsetItem6);
                IndexBaseItem indexBaseItem2 = new IndexBaseItem();
                list.Add(indexBaseItem2);
                list.Add(new DictNumberItem(fonts[i].privateLength));
                OffsetItem offsetItem7 = new DictOffsetItem();
                list.Add(offsetItem7);
                list.Add(new UInt8Item('\u0012'));
                list.Add(new IndexMarkerItem(offsetItem6, indexBaseItem2));
                list.Add(new MarkerItem(offsetItem7));
                list.Add(new RangeItem(buf, fonts[i].privateOffset, fonts[i].privateLength));
                if (fonts[i].privateSubrs >= 0)
                {
                    list.Add(GetEntireIndexRange(fonts[i].privateSubrs));
                }
            }

            list.Add(new MarkerItem(offsetItem3));
            list.Add(GetEntireIndexRange(fonts[i].charstringsOffset));
            int[] array2 = new int[1] { 0 };
            foreach (Item item in list)
            {
                item.Increment(array2);
            }

            foreach (Item item2 in list)
            {
                item2.Xref();
            }

            byte[] array3 = new byte[array2[0]];
            foreach (Item item3 in list)
            {
                item3.Emit(array3);
            }

            return array3;
        }

        public virtual bool IsCID(string fontName)
        {
            for (int i = 0; i < fonts.Length; i++)
            {
                if (fontName.Equals(fonts[i].name))
                {
                    return fonts[i].isCID;
                }
            }

            return false;
        }

        public virtual bool Exists(string fontName)
        {
            for (int i = 0; i < fonts.Length; i++)
            {
                if (fontName.Equals(fonts[i].name))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual string[] GetNames()
        {
            string[] array = new string[fonts.Length];
            for (int i = 0; i < fonts.Length; i++)
            {
                array[i] = fonts[i].name;
            }

            return array;
        }

        public CFFFont(RandomAccessFileOrArray inputbuffer)
        {
            buf = inputbuffer;
            Seek(0);
            GetCard8();
            GetCard8();
            int card = GetCard8();
            offSize = GetCard8();
            nameIndexOffset = card;
            nameOffsets = GetIndex(nameIndexOffset);
            topdictIndexOffset = nameOffsets[nameOffsets.Length - 1];
            topdictOffsets = GetIndex(topdictIndexOffset);
            stringIndexOffset = topdictOffsets[topdictOffsets.Length - 1];
            stringOffsets = GetIndex(stringIndexOffset);
            gsubrIndexOffset = stringOffsets[stringOffsets.Length - 1];
            gsubrOffsets = GetIndex(gsubrIndexOffset);
            fonts = new Font[nameOffsets.Length - 1];
            for (int i = 0; i < nameOffsets.Length - 1; i++)
            {
                fonts[i] = new Font();
                Seek(nameOffsets[i]);
                fonts[i].name = "";
                for (int j = nameOffsets[i]; j < nameOffsets[i + 1]; j++)
                {
                    fonts[i].name += GetCard8();
                }
            }

            for (int k = 0; k < topdictOffsets.Length - 1; k++)
            {
                Seek(topdictOffsets[k]);
                while (GetPosition() < topdictOffsets[k + 1])
                {
                    GetDictItem();
                    if (key == "FullName")
                    {
                        fonts[k].fullName = GetString((char)(int)args[0]);
                    }
                    else if (key == "ROS")
                    {
                        fonts[k].isCID = true;
                    }
                    else if (key == "Private")
                    {
                        fonts[k].privateLength = (int)args[0];
                        fonts[k].privateOffset = (int)args[1];
                    }
                    else if (key == "charset")
                    {
                        fonts[k].charsetOffset = (int)args[0];
                    }
                    else if (key == "CharStrings")
                    {
                        fonts[k].charstringsOffset = (int)args[0];
                        int position = GetPosition();
                        fonts[k].charstringsOffsets = GetIndex(fonts[k].charstringsOffset);
                        Seek(position);
                    }
                    else if (key == "FDArray")
                    {
                        fonts[k].fdarrayOffset = (int)args[0];
                    }
                    else if (key == "FDSelect")
                    {
                        fonts[k].fdselectOffset = (int)args[0];
                    }
                    else if (key == "CharstringType")
                    {
                        fonts[k].CharstringType = (int)args[0];
                    }
                }

                if (fonts[k].privateOffset >= 0)
                {
                    Seek(fonts[k].privateOffset);
                    while (GetPosition() < fonts[k].privateOffset + fonts[k].privateLength)
                    {
                        GetDictItem();
                        if (key == "Subrs")
                        {
                            fonts[k].privateSubrs = (int)args[0] + fonts[k].privateOffset;
                        }
                    }
                }

                if (fonts[k].fdarrayOffset < 0)
                {
                    continue;
                }

                int[] index = GetIndex(fonts[k].fdarrayOffset);
                fonts[k].fdprivateOffsets = new int[index.Length - 1];
                fonts[k].fdprivateLengths = new int[index.Length - 1];
                for (int l = 0; l < index.Length - 1; l++)
                {
                    Seek(index[l]);
                    while (GetPosition() < index[l + 1])
                    {
                        GetDictItem();
                        if (key == "Private")
                        {
                            fonts[k].fdprivateLengths[l] = (int)args[0];
                            fonts[k].fdprivateOffsets[l] = (int)args[1];
                        }
                    }
                }
            }
        }

        internal void ReadEncoding(int nextIndexOffset)
        {
            Seek(nextIndexOffset);
            GetCard8();
        }
    }
}
