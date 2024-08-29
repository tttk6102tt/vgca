using Sign.itext.error_messages;
using Sign.itext.text.pdf;
using Sign.itext.xml.simpleparser;
using Sign.SystemItext.util;
using System.Globalization;

namespace Sign.itext.pdf
{
    public abstract class BaseFont
    {
        public class StreamFont : PdfStream
        {
            public StreamFont(byte[] contents, int[] lengths, int compressionLevel)
            {
                bytes = contents;
                Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                for (int i = 0; i < lengths.Length; i++)
                {
                    Put(new PdfName("Length" + (i + 1)), new PdfNumber(lengths[i]));
                }

                FlateCompress(compressionLevel);
            }

            public StreamFont(byte[] contents, string subType, int compressionLevel)
            {
                bytes = contents;
                Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                if (subType != null)
                {
                    Put(PdfName.SUBTYPE, new PdfName(subType));
                }

                FlateCompress(compressionLevel);
            }
        }

        public const string COURIER = "Courier";

        public const string COURIER_BOLD = "Courier-Bold";

        public const string COURIER_OBLIQUE = "Courier-Oblique";

        public const string COURIER_BOLDOBLIQUE = "Courier-BoldOblique";

        public const string HELVETICA = "Helvetica";

        public const string HELVETICA_BOLD = "Helvetica-Bold";

        public const string HELVETICA_OBLIQUE = "Helvetica-Oblique";

        public const string HELVETICA_BOLDOBLIQUE = "Helvetica-BoldOblique";

        public const string SYMBOL = "Symbol";

        public const string TIMES_ROMAN = "Times-Roman";

        public const string TIMES_BOLD = "Times-Bold";

        public const string TIMES_ITALIC = "Times-Italic";

        public const string TIMES_BOLDITALIC = "Times-BoldItalic";

        public const string ZAPFDINGBATS = "ZapfDingbats";

        public const int ASCENT = 1;

        public const int CAPHEIGHT = 2;

        public const int DESCENT = 3;

        public const int ITALICANGLE = 4;

        public const int BBOXLLX = 5;

        public const int BBOXLLY = 6;

        public const int BBOXURX = 7;

        public const int BBOXURY = 8;

        public const int AWT_ASCENT = 9;

        public const int AWT_DESCENT = 10;

        public const int AWT_LEADING = 11;

        public const int AWT_MAXADVANCE = 12;

        public const int UNDERLINE_POSITION = 13;

        public const int UNDERLINE_THICKNESS = 14;

        public const int STRIKETHROUGH_POSITION = 15;

        public const int STRIKETHROUGH_THICKNESS = 16;

        public const int SUBSCRIPT_SIZE = 17;

        public const int SUBSCRIPT_OFFSET = 18;

        public const int SUPERSCRIPT_SIZE = 19;

        public const int SUPERSCRIPT_OFFSET = 20;

        public const int WEIGHT_CLASS = 21;

        public const int WIDTH_CLASS = 22;

        public const int FONT_WEIGHT = 23;

        public const int FONT_TYPE_T1 = 0;

        public const int FONT_TYPE_TT = 1;

        public const int FONT_TYPE_CJK = 2;

        public const int FONT_TYPE_TTUNI = 3;

        public const int FONT_TYPE_DOCUMENT = 4;

        public const int FONT_TYPE_T3 = 5;

        public const string IDENTITY_H = "Identity-H";

        public const string IDENTITY_V = "Identity-V";

        public const string CP1250 = "Cp1250";

        public const string CP1252 = "Cp1252";

        public const string CP1257 = "Cp1257";

        public const string WINANSI = "Cp1252";

        public const string MACROMAN = "MacRoman";

        public static readonly int[] CHAR_RANGE_LATIN;

        public static readonly int[] CHAR_RANGE_ARABIC;

        public static readonly int[] CHAR_RANGE_HEBREW;

        public static readonly int[] CHAR_RANGE_CYRILLIC;

        public const bool EMBEDDED = true;

        public const bool NOT_EMBEDDED = false;

        public const bool CACHED = true;

        public const bool NOT_CACHED = false;

        public const string RESOURCE_PATH = "text.pdf.fonts.";

        public const char CID_NEWLINE = '翿';

        public const char PARAGRAPH_SEPARATOR = '\u2029';

        protected List<int[]> subsetRanges;

        internal int fontType;

        public const string notdef = ".notdef";

        protected int[] widths = new int[256];

        protected string[] differences = new string[256];

        protected char[] unicodeDifferences = new char[256];

        protected int[][] charBBoxes = new int[256][];

        protected string encoding;

        protected bool embedded;

        protected int compressionLevel = -1;

        protected bool fontSpecific = true;

        protected static Dictionary<string, BaseFont> fontCache;

        protected static Dictionary<string, PdfName> BuiltinFonts14;

        protected bool forceWidthsOutput;

        protected bool directTextToByte;

        protected bool subset = true;

        protected bool fastWinansi;

        protected IntHashtable specialMap;

        private static Random random;

        protected bool vertical;

        public virtual List<int[]> SubsetRanges => subsetRanges;

        public virtual string Encoding => encoding;

        public virtual int FontType
        {
            get
            {
                return fontType;
            }
            set
            {
                fontType = value;
            }
        }

        public abstract string PostscriptFontName { get; set; }

        public abstract string[][] FullFontName { get; }

        public abstract string[][] AllNameEntries { get; }

        public abstract string[][] FamilyFontName { get; }

        public virtual string[] CodePagesSupported => new string[0];

        public virtual int[] Widths => widths;

        public virtual string[] Differences => differences;

        public virtual char[] UnicodeDifferences => unicodeDifferences;

        public virtual bool ForceWidthsOutput
        {
            get
            {
                return forceWidthsOutput;
            }
            set
            {
                forceWidthsOutput = value;
            }
        }

        public virtual bool DirectTextToByte
        {
            get
            {
                return directTextToByte;
            }
            set
            {
                directTextToByte = value;
            }
        }

        public virtual bool Subset
        {
            get
            {
                return subset;
            }
            set
            {
                subset = value;
            }
        }

        public virtual int CompressionLevel
        {
            get
            {
                return compressionLevel;
            }
            set
            {
                if (value < 0 || value > 9)
                {
                    compressionLevel = -1;
                }
                else
                {
                    compressionLevel = value;
                }
            }
        }

        static BaseFont()
        {
            CHAR_RANGE_LATIN = new int[8] { 0, 383, 8192, 8303, 8352, 8399, 64256, 64262 };
            CHAR_RANGE_ARABIC = new int[10] { 0, 127, 1536, 1663, 8352, 8399, 64336, 64511, 65136, 65279 };
            CHAR_RANGE_HEBREW = new int[8] { 0, 127, 1424, 1535, 8352, 8399, 64285, 64335 };
            CHAR_RANGE_CYRILLIC = new int[8] { 0, 127, 1024, 1327, 8192, 8303, 8352, 8399 };
            fontCache = new Dictionary<string, BaseFont>();
            BuiltinFonts14 = new Dictionary<string, PdfName>();
            random = new Random();
            BuiltinFonts14.Add("Courier", PdfName.COURIER);
            BuiltinFonts14.Add("Courier-Bold", PdfName.COURIER_BOLD);
            BuiltinFonts14.Add("Courier-BoldOblique", PdfName.COURIER_BOLDOBLIQUE);
            BuiltinFonts14.Add("Courier-Oblique", PdfName.COURIER_OBLIQUE);
            BuiltinFonts14.Add("Helvetica", PdfName.HELVETICA);
            BuiltinFonts14.Add("Helvetica-Bold", PdfName.HELVETICA_BOLD);
            BuiltinFonts14.Add("Helvetica-BoldOblique", PdfName.HELVETICA_BOLDOBLIQUE);
            BuiltinFonts14.Add("Helvetica-Oblique", PdfName.HELVETICA_OBLIQUE);
            BuiltinFonts14.Add("Symbol", PdfName.SYMBOL);
            BuiltinFonts14.Add("Times-Roman", PdfName.TIMES_ROMAN);
            BuiltinFonts14.Add("Times-Bold", PdfName.TIMES_BOLD);
            BuiltinFonts14.Add("Times-BoldItalic", PdfName.TIMES_BOLDITALIC);
            BuiltinFonts14.Add("Times-Italic", PdfName.TIMES_ITALIC);
            BuiltinFonts14.Add("ZapfDingbats", PdfName.ZAPFDINGBATS);
        }

        public static BaseFont CreateFont()
        {
            return CreateFont("Helvetica", "Cp1252", embedded: false);
        }

        public static BaseFont CreateFont(string name, string encoding, bool embedded)
        {
            return CreateFont(name, encoding, embedded, cached: true, null, null, noThrow: false);
        }

        public static BaseFont CreateFont(string name, string encoding, bool embedded, bool forceRead)
        {
            return CreateFont(name, encoding, embedded, cached: true, null, null, forceRead);
        }

        public static BaseFont CreateFont(string name, string encoding, bool embedded, bool cached, byte[] ttfAfm, byte[] pfb)
        {
            return CreateFont(name, encoding, embedded, cached, ttfAfm, pfb, noThrow: false);
        }

        public static BaseFont CreateFont(string name, string encoding, bool embedded, bool cached, byte[] ttfAfm, byte[] pfb, bool noThrow)
        {
            return CreateFont(name, encoding, embedded, cached, ttfAfm, pfb, noThrow, forceRead: false);
        }

        public static BaseFont CreateFont(string name, string encoding, bool embedded, bool cached, byte[] ttfAfm, byte[] pfb, bool noThrow, bool forceRead)
        {
            string baseName = GetBaseName(name);
            encoding = NormalizeEncoding(encoding);
            bool flag = BuiltinFonts14.ContainsKey(name);
            bool flag2 = !flag && CJKFont.IsCJKFont(baseName, encoding);
            if (flag || flag2)
            {
                embedded = false;
            }
            else if (encoding.Equals("Identity-H") || encoding.Equals("Identity-V"))
            {
                embedded = true;
            }

            BaseFont value = null;
            BaseFont baseFont = null;
            string key = name + "\n" + encoding + "\n" + embedded;
            if (cached)
            {
                lock (fontCache)
                {
                    fontCache.TryGetValue(key, out value);
                }

                if (value != null)
                {
                    return value;
                }
            }

            if (flag || name.ToLower(CultureInfo.InvariantCulture).EndsWith(".afm") || name.ToLower(CultureInfo.InvariantCulture).EndsWith(".pfm"))
            {
                baseFont = new Type1Font(name, encoding, embedded, ttfAfm, pfb, forceRead);
                baseFont.fastWinansi = encoding.Equals("Cp1252");
            }
            else if (baseName.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") || baseName.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") || baseName.ToLower(CultureInfo.InvariantCulture).IndexOf(".ttc,") > 0)
            {
                if (encoding.Equals("Identity-H") || encoding.Equals("Identity-V"))
                {
                    baseFont = new TrueTypeFontUnicode(name, encoding, embedded, ttfAfm, forceRead);
                }
                else
                {
                    baseFont = new TrueTypeFont(name, encoding, embedded, ttfAfm, justNames: false, forceRead);
                    baseFont.fastWinansi = encoding.Equals("Cp1252");
                }
            }
            else
            {
                if (!flag2)
                {
                    if (noThrow)
                    {
                        return null;
                    }

                    throw new DocumentException(MessageLocalization.GetComposedMessage("font.1.with.2.is.not.recognized", name, encoding));
                }

                baseFont = new CJKFont(name, encoding, embedded);
            }

            if (cached)
            {
                lock (fontCache)
                {
                    fontCache.TryGetValue(key, out value);
                    if (value != null)
                    {
                        return value;
                    }

                    fontCache[key] = baseFont;
                    return baseFont;
                }
            }

            return baseFont;
        }

        public static BaseFont CreateFont(PRIndirectReference fontRef)
        {
            return new DocumentFont(fontRef);
        }

        public virtual bool IsVertical()
        {
            return vertical;
        }

        protected static string GetBaseName(string name)
        {
            if (name.EndsWith(",Bold"))
            {
                return name.Substring(0, name.Length - 5);
            }

            if (name.EndsWith(",Italic"))
            {
                return name.Substring(0, name.Length - 7);
            }

            if (name.EndsWith(",BoldItalic"))
            {
                return name.Substring(0, name.Length - 11);
            }

            return name;
        }

        protected static string NormalizeEncoding(string enc)
        {
            if (enc.Equals("winansi") || enc.Equals(""))
            {
                return "Cp1252";
            }

            if (enc.Equals("macroman"))
            {
                return "MacRoman";
            }

            return IanaEncodings.GetEncodingNumber(enc) switch
            {
                1252 => "Cp1252",
                10000 => "MacRoman",
                _ => enc,
            };
        }

        protected virtual void CreateEncoding()
        {
            if (encoding.StartsWith("#"))
            {
                specialMap = new IntHashtable();
                StringTokenizer stringTokenizer = new StringTokenizer(encoding.Substring(1), " ,\t\n\r\f");
                if (stringTokenizer.NextToken().Equals("full"))
                {
                    while (stringTokenizer.HasMoreTokens())
                    {
                        string text = stringTokenizer.NextToken();
                        string text2 = stringTokenizer.NextToken();
                        char c = (char)int.Parse(stringTokenizer.NextToken(), NumberStyles.HexNumber);
                        int num = ((!text.StartsWith("'")) ? int.Parse(text) : text[1]);
                        num %= 256;
                        specialMap[c] = num;
                        differences[num] = text2;
                        unicodeDifferences[num] = c;
                        widths[num] = GetRawWidth(c, text2);
                        charBBoxes[num] = GetRawCharBBox(c, text2);
                    }
                }
                else
                {
                    int num2 = 0;
                    if (stringTokenizer.HasMoreTokens())
                    {
                        num2 = int.Parse(stringTokenizer.NextToken());
                    }

                    while (stringTokenizer.HasMoreTokens() && num2 < 256)
                    {
                        int num3 = int.Parse(stringTokenizer.NextToken(), NumberStyles.HexNumber) % 65536;
                        string text3 = GlyphList.UnicodeToName(num3);
                        if (text3 != null)
                        {
                            specialMap[num3] = num2;
                            differences[num2] = text3;
                            unicodeDifferences[num2] = (char)num3;
                            widths[num2] = GetRawWidth(num3, text3);
                            charBBoxes[num2] = GetRawCharBBox(num3, text3);
                            num2++;
                        }
                    }
                }

                for (int i = 0; i < 256; i++)
                {
                    if (differences[i] == null)
                    {
                        differences[i] = ".notdef";
                    }
                }

                return;
            }

            if (fontSpecific)
            {
                for (int j = 0; j < 256; j++)
                {
                    widths[j] = GetRawWidth(j, null);
                    charBBoxes[j] = GetRawCharBBox(j, null);
                }

                return;
            }

            byte[] array = new byte[1];
            for (int k = 0; k < 256; k++)
            {
                array[0] = (byte)k;
                string text4 = PdfEncodings.ConvertToString(array, encoding);
                char c2 = ((text4.Length <= 0) ? '?' : text4[0]);
                string text5 = GlyphList.UnicodeToName(c2);
                if (text5 == null)
                {
                    text5 = ".notdef";
                }

                differences[k] = text5;
                UnicodeDifferences[k] = c2;
                widths[k] = GetRawWidth(c2, text5);
                charBBoxes[k] = GetRawCharBBox(c2, text5);
            }
        }

        internal abstract int GetRawWidth(int c, string name);

        public abstract int GetKerning(int char1, int char2);

        public abstract bool SetKerning(int char1, int char2, int kern);

        public virtual int GetWidth(int char1)
        {
            if (fastWinansi)
            {
                if (char1 < 128 || (char1 >= 160 && char1 <= 255))
                {
                    return widths[char1];
                }

                return widths[PdfEncodings.winansi[char1]];
            }

            int num = 0;
            byte[] array = ConvertToBytes(char1);
            for (int i = 0; i < array.Length; i++)
            {
                num += widths[0xFF & array[i]];
            }

            return num;
        }

        public virtual int GetWidth(string text)
        {
            int num = 0;
            if (fastWinansi)
            {
                int length = text.Length;
                for (int i = 0; i < length; i++)
                {
                    char c = text[i];
                    num = ((c >= '\u0080' && (c < '\u00a0' || c > 'ÿ')) ? (num + widths[PdfEncodings.winansi[c]]) : (num + widths[(uint)c]));
                }

                return num;
            }

            byte[] array = ConvertToBytes(text);
            for (int j = 0; j < array.Length; j++)
            {
                num += widths[0xFF & array[j]];
            }

            return num;
        }

        public virtual int GetDescent(string text)
        {
            int num = 0;
            char[] array = text.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                int[] charBBox = GetCharBBox(array[i]);
                if (charBBox != null && charBBox[1] < num)
                {
                    num = charBBox[1];
                }
            }

            return num;
        }

        public virtual int GetAscent(string text)
        {
            int num = 0;
            char[] array = text.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                int[] charBBox = GetCharBBox(array[i]);
                if (charBBox != null && charBBox[3] > num)
                {
                    num = charBBox[3];
                }
            }

            return num;
        }

        public virtual float GetDescentPoint(string text, float fontSize)
        {
            return (float)GetDescent(text) * 0.001f * fontSize;
        }

        public virtual float GetAscentPoint(string text, float fontSize)
        {
            return (float)GetAscent(text) * 0.001f * fontSize;
        }

        public virtual float GetWidthPointKerned(string text, float fontSize)
        {
            float num = (float)GetWidth(text) * 0.001f * fontSize;
            if (!HasKernPairs())
            {
                return num;
            }

            int num2 = text.Length - 1;
            int num3 = 0;
            char[] array = text.ToCharArray();
            for (int i = 0; i < num2; i++)
            {
                num3 += GetKerning(array[i], array[i + 1]);
            }

            return num + (float)num3 * 0.001f * fontSize;
        }

        public virtual float GetWidthPoint(string text, float fontSize)
        {
            return (float)GetWidth(text) * 0.001f * fontSize;
        }

        public virtual float GetWidthPoint(int char1, float fontSize)
        {
            return (float)GetWidth(char1) * 0.001f * fontSize;
        }

        public virtual byte[] ConvertToBytes(string text)
        {
            if (directTextToByte)
            {
                return PdfEncodings.ConvertToBytes(text, null);
            }

            if (specialMap != null)
            {
                byte[] array = new byte[text.Length];
                int num = 0;
                int length = text.Length;
                for (int i = 0; i < length; i++)
                {
                    char key = text[i];
                    if (specialMap.ContainsKey(key))
                    {
                        array[num++] = (byte)specialMap[key];
                    }
                }

                if (num < length)
                {
                    byte[] array2 = new byte[num];
                    Array.Copy(array, 0, array2, 0, num);
                    return array2;
                }

                return array;
            }

            return PdfEncodings.ConvertToBytes(text, encoding);
        }

        internal virtual byte[] ConvertToBytes(int char1)
        {
            if (directTextToByte)
            {
                return PdfEncodings.ConvertToBytes((char)char1, null);
            }

            if (specialMap != null)
            {
                if (specialMap.ContainsKey(char1))
                {
                    return new byte[1] { (byte)specialMap[char1] };
                }

                return new byte[0];
            }

            return PdfEncodings.ConvertToBytes((char)char1, encoding);
        }

        internal abstract void WriteFont(PdfWriter writer, PdfIndirectReference piRef, object[] oParams);

        public abstract PdfStream GetFullFontStream();

        public abstract float GetFontDescriptor(int key, float fontSize);

        public virtual void SetFontDescriptor(int key, float value)
        {
        }

        public virtual bool IsEmbedded()
        {
            return embedded;
        }

        public virtual bool IsFontSpecific()
        {
            return fontSpecific;
        }

        public static string CreateSubsetPrefix()
        {
            char[] array = new char[7];
            lock (random)
            {
                for (int i = 0; i < 6; i++)
                {
                    array[i] = (char)random.Next(65, 91);
                }
            }

            array[6] = '+';
            return new string(array);
        }

        internal char GetUnicodeDifferences(int index)
        {
            return unicodeDifferences[index];
        }

        public static string[][] GetFullFontName(string name, string encoding, byte[] ttfAfm)
        {
            string baseName = GetBaseName(name);
            BaseFont baseFont = null;
            baseFont = ((!baseName.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") && !baseName.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") && baseName.ToLower(CultureInfo.InvariantCulture).IndexOf(".ttc,") <= 0) ? CreateFont(name, encoding, embedded: false, cached: false, ttfAfm, null) : new TrueTypeFont(name, "Cp1252", emb: false, ttfAfm, justNames: true, forceRead: false));
            return baseFont.FullFontName;
        }

        public static object[] GetAllFontNames(string name, string encoding, byte[] ttfAfm)
        {
            string baseName = GetBaseName(name);
            BaseFont baseFont = null;
            baseFont = ((!baseName.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") && !baseName.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") && baseName.ToLower(CultureInfo.InvariantCulture).IndexOf(".ttc,") <= 0) ? CreateFont(name, encoding, embedded: false, cached: false, ttfAfm, null) : new TrueTypeFont(name, "Cp1252", emb: false, ttfAfm, justNames: true, forceRead: false));
            return new object[3] { baseFont.PostscriptFontName, baseFont.FamilyFontName, baseFont.FullFontName };
        }

        public static string[][] GetAllNameEntries(string name, string encoding, byte[] ttfAfm)
        {
            string baseName = GetBaseName(name);
            BaseFont baseFont = null;
            baseFont = ((!baseName.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") && !baseName.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") && baseName.ToLower(CultureInfo.InvariantCulture).IndexOf(".ttc,") <= 0) ? CreateFont(name, encoding, embedded: false, cached: false, ttfAfm, null) : new TrueTypeFont(name, "Cp1252", emb: false, ttfAfm, justNames: true, forceRead: false));
            return baseFont.AllNameEntries;
        }

        public static string[] EnumerateTTCNames(string ttcFile)
        {
            return new EnumerateTTC(ttcFile).Names;
        }

        public static string[] EnumerateTTCNames(byte[] ttcArray)
        {
            return new EnumerateTTC(ttcArray).Names;
        }

        public virtual int GetUnicodeEquivalent(int c)
        {
            return c;
        }

        public virtual int GetCidCode(int c)
        {
            return c;
        }

        public abstract bool HasKernPairs();

        public virtual bool CharExists(int c)
        {
            return ConvertToBytes(c).Length != 0;
        }

        public virtual bool SetCharAdvance(int c, int advance)
        {
            byte[] array = ConvertToBytes(c);
            if (array.Length == 0)
            {
                return false;
            }

            widths[0xFF & array[0]] = advance;
            return true;
        }

        private static void AddFont(PRIndirectReference fontRef, IntHashtable hits, List<object[]> fonts)
        {
            PdfObject pdfObject = PdfReader.GetPdfObject(fontRef);
            if (pdfObject != null && pdfObject.IsDictionary())
            {
                PdfDictionary pdfDictionary = (PdfDictionary)pdfObject;
                PdfName asName = pdfDictionary.GetAsName(PdfName.SUBTYPE);
                if (PdfName.TYPE1.Equals(asName) || PdfName.TRUETYPE.Equals(asName) || PdfName.TYPE0.Equals(asName))
                {
                    PdfName asName2 = pdfDictionary.GetAsName(PdfName.BASEFONT);
                    fonts.Add(new object[2]
                    {
                        PdfName.DecodeName(asName2.ToString()),
                        fontRef
                    });
                    hits[fontRef.Number] = 1;
                }
            }
        }

        private static void RecourseFonts(PdfDictionary page, IntHashtable hits, List<object[]> fonts, int level)
        {
            level++;
            if (level > 50 || page == null)
            {
                return;
            }

            PdfDictionary asDict = page.GetAsDict(PdfName.RESOURCES);
            if (asDict == null)
            {
                return;
            }

            PdfDictionary asDict2 = asDict.GetAsDict(PdfName.FONT);
            if (asDict2 != null)
            {
                foreach (PdfName key in asDict2.Keys)
                {
                    PdfObject pdfObject = asDict2.Get(key);
                    if (pdfObject != null && pdfObject.IsIndirect())
                    {
                        int number = ((PRIndirectReference)pdfObject).Number;
                        if (!hits.ContainsKey(number))
                        {
                            AddFont((PRIndirectReference)pdfObject, hits, fonts);
                        }
                    }
                }
            }

            PdfDictionary asDict3 = asDict.GetAsDict(PdfName.XOBJECT);
            if (asDict3 == null)
            {
                return;
            }

            foreach (PdfName key2 in asDict3.Keys)
            {
                PdfObject directObject = asDict3.GetDirectObject(key2);
                if (directObject is PdfDictionary)
                {
                    RecourseFonts((PdfDictionary)directObject, hits, fonts, level);
                }
            }
        }

        public static List<object[]> GetDocumentFonts(PdfReader reader)
        {
            IntHashtable hits = new IntHashtable();
            List<object[]> list = new List<object[]>();
            int numberOfPages = reader.NumberOfPages;
            for (int i = 1; i <= numberOfPages; i++)
            {
                RecourseFonts(reader.GetPageN(i), hits, list, 1);
            }

            return list;
        }

        public static List<object[]> GetDocumentFonts(PdfReader reader, int page)
        {
            IntHashtable hits = new IntHashtable();
            List<object[]> list = new List<object[]>();
            RecourseFonts(reader.GetPageN(page), hits, list, 1);
            return list;
        }

        public virtual int[] GetCharBBox(int c)
        {
            byte[] array = ConvertToBytes(c);
            if (array.Length == 0)
            {
                return null;
            }

            return charBBoxes[array[0] & 0xFF];
        }

        protected abstract int[] GetRawCharBBox(int c, string name);

        public virtual void CorrectArabicAdvance()
        {
            for (char c = '\u064b'; c <= '\u0658'; c = (char)(c + 1))
            {
                SetCharAdvance(c, 0);
            }

            SetCharAdvance(1648, 0);
            for (char c2 = '\u06d6'; c2 <= '\u06dc'; c2 = (char)(c2 + 1))
            {
                SetCharAdvance(c2, 0);
            }

            for (char c3 = '\u06df'; c3 <= '\u06e4'; c3 = (char)(c3 + 1))
            {
                SetCharAdvance(c3, 0);
            }

            for (char c4 = '\u06e7'; c4 <= '\u06e8'; c4 = (char)(c4 + 1))
            {
                SetCharAdvance(c4, 0);
            }

            for (char c5 = '\u06ea'; c5 <= '\u06ed'; c5 = (char)(c5 + 1))
            {
                SetCharAdvance(c5, 0);
            }
        }

        public virtual void AddSubsetRange(int[] range)
        {
            if (subsetRanges == null)
            {
                subsetRanges = new List<int[]>();
            }

            subsetRanges.Add(range);
        }
    }
}
