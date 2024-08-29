using Sign.itext.io;
using Sign.itext.pdf.fonts.cmaps;
using Sign.itext.text;
using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class DocumentFont : BaseFont
    {
        private Dictionary<int, int[]> metrics = new Dictionary<int, int[]>();

        private string fontName;

        private PRIndirectReference refFont;

        private PdfDictionary font;

        private IntHashtable uni2byte = new IntHashtable();

        private IntHashtable byte2uni = new IntHashtable();

        private IntHashtable diffmap;

        private float Ascender = 800f;

        private float CapHeight = 700f;

        private float Descender = -200f;

        private float ItalicAngle;

        private float fontWeight;

        private float llx = -50f;

        private float lly = -200f;

        private float urx = 100f;

        private float ury = 900f;

        protected bool isType0;

        protected int defaultWidth = 1000;

        private IntHashtable hMetrics;

        protected internal string cjkEncoding;

        protected internal string uniMap;

        private BaseFont cjkMirror;

        private static int[] stdEnc = new int[256]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 32, 33, 34, 35, 36, 37, 38, 8217,
            40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
            50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
            60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
            70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
            80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
            90, 91, 92, 93, 94, 95, 8216, 97, 98, 99,
            100, 101, 102, 103, 104, 105, 106, 107, 108, 109,
            110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
            120, 121, 122, 123, 124, 125, 126, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 161, 162, 163, 8260, 165, 402, 167, 164, 39,
            8220, 171, 8249, 8250, 64257, 64258, 0, 8211, 8224, 8225,
            183, 0, 182, 8226, 8218, 8222, 8221, 187, 8230, 8240,
            0, 191, 0, 96, 180, 710, 732, 175, 728, 729,
            168, 0, 730, 184, 0, 733, 731, 711, 8212, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 198, 0, 170, 0, 0,
            0, 0, 321, 216, 338, 186, 0, 0, 0, 0,
            0, 230, 0, 0, 0, 305, 0, 0, 322, 248,
            339, 223, 0, 0, 0, 0
        };

        public virtual PdfDictionary FontDictionary => font;

        public override string[][] FamilyFontName => FullFontName;

        public override string[][] FullFontName => new string[1][] { new string[4] { "", "", "", fontName } };

        public override string[][] AllNameEntries => new string[1][] { new string[5] { "4", "", "", "", fontName } };

        public override string PostscriptFontName
        {
            get
            {
                return fontName;
            }
            set
            {
            }
        }

        internal PdfIndirectReference IndirectReference
        {
            get
            {
                if (refFont == null)
                {
                    throw new ArgumentException("Font reuse not allowed with direct font objects.");
                }

                return refFont;
            }
        }

        internal IntHashtable Uni2Byte => uni2byte;

        internal IntHashtable Byte2Uni => byte2uni;

        internal IntHashtable Diffmap => diffmap;

        internal DocumentFont(PdfDictionary font)
        {
            refFont = null;
            this.font = font;
            Init();
        }

        internal DocumentFont(PRIndirectReference refFont)
        {
            this.refFont = refFont;
            font = (PdfDictionary)PdfReader.GetPdfObject(refFont);
            Init();
        }

        internal DocumentFont(PRIndirectReference refFont, PdfDictionary drEncoding)
        {
            this.refFont = refFont;
            font = (PdfDictionary)PdfReader.GetPdfObject(refFont);
            if (font.GetAsName(PdfName.ENCODING) == null && drEncoding != null)
            {
                foreach (PdfName key in drEncoding.Keys)
                {
                    font.Put(PdfName.ENCODING, drEncoding.Get(key));
                }
            }

            Init();
        }

        private void Init()
        {
            encoding = "";
            fontSpecific = false;
            fontType = 4;
            PdfName asName = font.GetAsName(PdfName.BASEFONT);
            fontName = ((asName != null) ? PdfName.DecodeName(asName.ToString()) : "Unspecified Font Name");
            PdfName asName2 = font.GetAsName(PdfName.SUBTYPE);
            if (PdfName.TYPE1.Equals(asName2) || PdfName.TRUETYPE.Equals(asName2))
            {
                DoType1TT();
                return;
            }

            if (PdfName.TYPE3.Equals(asName2))
            {
                FillEncoding(null);
                FillDiffMap(font.GetAsDict(PdfName.ENCODING), null);
                return;
            }

            PdfName asName3 = font.GetAsName(PdfName.ENCODING);
            if (asName3 == null)
            {
                return;
            }

            string text = PdfName.DecodeName(asName3.ToString());
            string compatibleFont = CJKFont.GetCompatibleFont(text);
            if (compatibleFont != null)
            {
                cjkMirror = BaseFont.CreateFont(compatibleFont, text, embedded: false);
                cjkEncoding = text;
                uniMap = ((CJKFont)cjkMirror).UniMap;
            }

            if (!PdfName.TYPE0.Equals(asName2))
            {
                return;
            }

            isType0 = true;
            if (!text.Equals("Identity-H") && cjkMirror != null)
            {
                PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(((PdfArray)PdfReader.GetPdfObjectRelease(font.Get(PdfName.DESCENDANTFONTS)))[0]);
                PdfNumber pdfNumber = (PdfNumber)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.DW));
                if (pdfNumber != null)
                {
                    defaultWidth = pdfNumber.IntValue;
                }

                hMetrics = ReadWidths((PdfArray)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.W)));
                PdfDictionary fontDesc = (PdfDictionary)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.FONTDESCRIPTOR));
                FillFontDesc(fontDesc);
            }
            else
            {
                ProcessType0(font);
            }
        }

        private void ProcessType0(PdfDictionary font)
        {
            PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(font.Get(PdfName.TOUNICODE));
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(((PdfArray)PdfReader.GetPdfObjectRelease(font.Get(PdfName.DESCENDANTFONTS)))[0]);
            PdfNumber pdfNumber = (PdfNumber)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.DW));
            int dw = 1000;
            if (pdfNumber != null)
            {
                dw = pdfNumber.IntValue;
            }

            IntHashtable intHashtable = ReadWidths((PdfArray)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.W)));
            PdfDictionary fontDesc = (PdfDictionary)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.FONTDESCRIPTOR));
            FillFontDesc(fontDesc);
            if (pdfObjectRelease is PRStream)
            {
                FillMetrics(PdfReader.GetStreamBytes((PRStream)pdfObjectRelease), intHashtable, dw);
            }
            else if (new PdfName("Identity-H").Equals(pdfObjectRelease))
            {
                FillMetricsIdentity(intHashtable, dw);
            }
        }

        private IntHashtable ReadWidths(PdfArray ws)
        {
            IntHashtable intHashtable = new IntHashtable();
            if (ws == null)
            {
                return intHashtable;
            }

            int num;
            for (num = 0; num < ws.Size; num++)
            {
                int i = ((PdfNumber)PdfReader.GetPdfObjectRelease(ws[num])).IntValue;
                PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(ws[++num]);
                if (pdfObjectRelease.IsArray())
                {
                    PdfArray pdfArray = (PdfArray)pdfObjectRelease;
                    for (int j = 0; j < pdfArray.Size; j++)
                    {
                        int intValue = ((PdfNumber)PdfReader.GetPdfObjectRelease(pdfArray[j])).IntValue;
                        intHashtable[i++] = intValue;
                    }
                }
                else
                {
                    int intValue2 = ((PdfNumber)pdfObjectRelease).IntValue;
                    int intValue3 = ((PdfNumber)PdfReader.GetPdfObjectRelease(ws[++num])).IntValue;
                    for (; i <= intValue2; i++)
                    {
                        intHashtable[i] = intValue3;
                    }
                }
            }

            return intHashtable;
        }

        private string DecodeString(PdfString ps)
        {
            if (ps.IsHexWriting())
            {
                return PdfEncodings.ConvertToString(ps.GetBytes(), "UnicodeBigUnmarked");
            }

            return ps.ToUnicodeString();
        }

        private void FillMetricsIdentity(IntHashtable widths, int dw)
        {
            for (int i = 0; i < 65536; i++)
            {
                int num = dw;
                if (widths.ContainsKey(i))
                {
                    num = widths[i];
                }

                metrics.Add(i, new int[2] { i, num });
            }
        }

        private void FillMetrics(byte[] touni, IntHashtable widths, int dw)
        {
            PdfContentParser pdfContentParser = new PdfContentParser(new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(touni))));
            PdfObject pdfObject = null;
            bool flag = true;
            int num = 0;
            int num2 = 50;
            while (flag || num > 0)
            {
                try
                {
                    pdfObject = pdfContentParser.ReadPRObject();
                }
                catch
                {
                    if (--num2 < 0)
                    {
                        return;
                    }

                    continue;
                }

                if (pdfObject == null)
                {
                    break;
                }

                if (pdfObject.Type != 200)
                {
                    continue;
                }

                if (pdfObject.ToString().Equals("begin"))
                {
                    flag = false;
                    num++;
                }
                else if (pdfObject.ToString().Equals("end"))
                {
                    num--;
                }
                else if (pdfObject.ToString().Equals("beginbfchar"))
                {
                    while (true)
                    {
                        PdfObject pdfObject2 = pdfContentParser.ReadPRObject();
                        if (pdfObject2.ToString().Equals("endbfchar"))
                        {
                            break;
                        }

                        string text = DecodeString((PdfString)pdfObject2);
                        string text2 = DecodeString((PdfString)pdfContentParser.ReadPRObject());
                        if (text2.Length == 1)
                        {
                            int num3 = text[0];
                            int key = text2[text2.Length - 1];
                            int num4 = dw;
                            if (widths.ContainsKey(num3))
                            {
                                num4 = widths[num3];
                            }

                            metrics[key] = new int[2] { num3, num4 };
                        }
                    }
                }
                else
                {
                    if (!pdfObject.ToString().Equals("beginbfrange"))
                    {
                        continue;
                    }

                    while (true)
                    {
                        PdfObject pdfObject3 = pdfContentParser.ReadPRObject();
                        if (pdfObject3.ToString().Equals("endbfrange"))
                        {
                            break;
                        }

                        string text3 = DecodeString((PdfString)pdfObject3);
                        string text4 = DecodeString((PdfString)pdfContentParser.ReadPRObject());
                        int num5 = text3[0];
                        int num6 = text4[0];
                        PdfObject pdfObject4 = pdfContentParser.ReadPRObject();
                        if (pdfObject4.IsString())
                        {
                            string text5 = DecodeString((PdfString)pdfObject4);
                            if (text5.Length != 1)
                            {
                                continue;
                            }

                            int num7 = text5[text5.Length - 1];
                            while (num5 <= num6)
                            {
                                int num8 = dw;
                                if (widths.ContainsKey(num5))
                                {
                                    num8 = widths[num5];
                                }

                                metrics[num7] = new int[2] { num5, num8 };
                                num5++;
                                num7++;
                            }

                            continue;
                        }

                        PdfArray pdfArray = (PdfArray)pdfObject4;
                        int num9 = 0;
                        while (num9 < pdfArray.Size)
                        {
                            string text6 = DecodeString(pdfArray.GetAsString(num9));
                            if (text6.Length == 1)
                            {
                                int key2 = text6[text6.Length - 1];
                                int num10 = dw;
                                if (widths.ContainsKey(num5))
                                {
                                    num10 = widths[num5];
                                }

                                metrics[key2] = new int[2] { num5, num10 };
                            }

                            num9++;
                            num5++;
                        }
                    }
                }
            }
        }

        private void DoType1TT()
        {
            CMapToUnicode toUnicode = null;
            PdfObject pdfObject = PdfReader.GetPdfObject(font.Get(PdfName.ENCODING));
            if (pdfObject == null)
            {
                PdfName asName = font.GetAsName(PdfName.BASEFONT);
                if (BaseFont.BuiltinFonts14.ContainsKey(fontName) && (PdfName.SYMBOL.Equals(asName) || PdfName.ZAPFDINGBATS.Equals(asName)))
                {
                    FillEncoding(asName);
                }
                else
                {
                    FillEncoding(null);
                }

                toUnicode = ProcessToUnicode();
                if (toUnicode != null)
                {
                    foreach (KeyValuePair<int, int> item in toUnicode.CreateReverseMapping())
                    {
                        uni2byte[item.Key] = item.Value;
                        byte2uni[item.Value] = item.Key;
                    }
                }
            }
            else if (pdfObject.IsName())
            {
                FillEncoding((PdfName)pdfObject);
            }
            else if (pdfObject.IsDictionary())
            {
                PdfDictionary pdfDictionary = (PdfDictionary)pdfObject;
                pdfObject = PdfReader.GetPdfObject(pdfDictionary.Get(PdfName.BASEENCODING));
                if (pdfObject == null)
                {
                    FillEncoding(null);
                }
                else
                {
                    FillEncoding((PdfName)pdfObject);
                }

                FillDiffMap(pdfDictionary, toUnicode);
            }

            PdfArray asArray = font.GetAsArray(PdfName.WIDTHS);
            PdfNumber asNumber = font.GetAsNumber(PdfName.FIRSTCHAR);
            PdfNumber asNumber2 = font.GetAsNumber(PdfName.LASTCHAR);
            if (BaseFont.BuiltinFonts14.ContainsKey(fontName))
            {
                BaseFont baseFont = BaseFont.CreateFont(fontName, "Cp1252", embedded: false);
                int[] array = uni2byte.ToOrderedKeys();
                for (int i = 0; i < array.Length; i++)
                {
                    int num = uni2byte[array[i]];
                    widths[num] = baseFont.GetRawWidth(num, GlyphList.UnicodeToName(array[i]));
                }

                if (diffmap != null)
                {
                    array = diffmap.ToOrderedKeys();
                    for (int j = 0; j < array.Length; j++)
                    {
                        int num2 = diffmap[array[j]];
                        widths[num2] = baseFont.GetRawWidth(num2, GlyphList.UnicodeToName(array[j]));
                    }

                    diffmap = null;
                }

                Ascender = baseFont.GetFontDescriptor(1, 1000f);
                CapHeight = baseFont.GetFontDescriptor(2, 1000f);
                Descender = baseFont.GetFontDescriptor(3, 1000f);
                ItalicAngle = baseFont.GetFontDescriptor(4, 1000f);
                fontWeight = baseFont.GetFontDescriptor(23, 1000f);
                llx = baseFont.GetFontDescriptor(5, 1000f);
                lly = baseFont.GetFontDescriptor(6, 1000f);
                urx = baseFont.GetFontDescriptor(7, 1000f);
                ury = baseFont.GetFontDescriptor(8, 1000f);
            }

            if (asNumber != null && asNumber2 != null && asArray != null)
            {
                int intValue = asNumber.IntValue;
                int num3 = intValue + asArray.Size;
                if (widths.Length < num3)
                {
                    int[] destinationArray = new int[num3];
                    Array.Copy(widths, 0, destinationArray, 0, intValue);
                    widths = destinationArray;
                }

                for (int k = 0; k < asArray.Size; k++)
                {
                    widths[intValue + k] = asArray.GetAsNumber(k).IntValue;
                }
            }

            FillFontDesc(font.GetAsDict(PdfName.FONTDESCRIPTOR));
        }

        private void FillDiffMap(PdfDictionary encDic, CMapToUnicode toUnicode)
        {
            PdfArray asArray = encDic.GetAsArray(PdfName.DIFFERENCES);
            if (asArray == null)
            {
                return;
            }

            diffmap = new IntHashtable();
            int num = 0;
            for (int i = 0; i < asArray.Size; i++)
            {
                PdfObject pdfObject = asArray[i];
                if (pdfObject.IsNumber())
                {
                    num = ((PdfNumber)pdfObject).IntValue;
                    continue;
                }

                int[] array = GlyphList.NameToUnicode(PdfName.DecodeName(((PdfName)pdfObject).ToString()));
                if (array != null && array.Length != 0)
                {
                    uni2byte[array[0]] = num;
                    byte2uni[num] = array[0];
                    diffmap[array[0]] = num;
                }
                else
                {
                    if (toUnicode == null)
                    {
                        toUnicode = ProcessToUnicode();
                        if (toUnicode == null)
                        {
                            toUnicode = new CMapToUnicode();
                        }
                    }

                    string text = toUnicode.Lookup(new byte[1] { (byte)num }, 0, 1);
                    if (text != null && text.Length == 1)
                    {
                        uni2byte[text[0]] = num;
                        byte2uni[num] = text[0];
                        diffmap[text[0]] = num;
                    }
                }

                num++;
            }
        }

        private CMapToUnicode ProcessToUnicode()
        {
            CMapToUnicode result = null;
            PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(font.Get(PdfName.TOUNICODE));
            if (pdfObjectRelease is PRStream)
            {
                try
                {
                    CidLocationFromByte location = new CidLocationFromByte(PdfReader.GetStreamBytes((PRStream)pdfObjectRelease));
                    result = new CMapToUnicode();
                    CMapParserEx.ParseCid("", result, location);
                    return result;
                }
                catch
                {
                    return null;
                }
            }

            return result;
        }

        private void FillFontDesc(PdfDictionary fontDesc)
        {
            if (fontDesc == null)
            {
                return;
            }

            PdfNumber asNumber = fontDesc.GetAsNumber(PdfName.ASCENT);
            if (asNumber != null)
            {
                Ascender = asNumber.FloatValue;
            }

            asNumber = fontDesc.GetAsNumber(PdfName.CAPHEIGHT);
            if (asNumber != null)
            {
                CapHeight = asNumber.FloatValue;
            }

            asNumber = fontDesc.GetAsNumber(PdfName.DESCENT);
            if (asNumber != null)
            {
                Descender = asNumber.FloatValue;
            }

            asNumber = fontDesc.GetAsNumber(PdfName.ITALICANGLE);
            if (asNumber != null)
            {
                ItalicAngle = asNumber.FloatValue;
            }

            asNumber = fontDesc.GetAsNumber(PdfName.FONTWEIGHT);
            if (asNumber != null)
            {
                fontWeight = asNumber.FloatValue;
            }

            PdfArray asArray = fontDesc.GetAsArray(PdfName.FONTBBOX);
            if (asArray != null)
            {
                llx = asArray.GetAsNumber(0).FloatValue;
                lly = asArray.GetAsNumber(1).FloatValue;
                urx = asArray.GetAsNumber(2).FloatValue;
                ury = asArray.GetAsNumber(3).FloatValue;
                if (llx > urx)
                {
                    float num = llx;
                    llx = urx;
                    urx = num;
                }

                if (lly > ury)
                {
                    float num2 = lly;
                    lly = ury;
                    ury = num2;
                }
            }

            float num3 = Math.Max(ury, Ascender);
            float num4 = Math.Min(lly, Descender);
            Ascender = num3 * 1000f / (num3 - num4);
            Descender = num4 * 1000f / (num3 - num4);
        }

        private void FillEncoding(PdfName encoding)
        {
            if (encoding == null && IsSymbolic())
            {
                for (int i = 0; i < 256; i++)
                {
                    uni2byte[i] = i;
                    byte2uni[i] = i;
                }
            }
            else if (PdfName.MAC_ROMAN_ENCODING.Equals(encoding) || PdfName.WIN_ANSI_ENCODING.Equals(encoding) || PdfName.SYMBOL.Equals(encoding) || PdfName.ZAPFDINGBATS.Equals(encoding))
            {
                byte[] array = new byte[256];
                for (int j = 0; j < 256; j++)
                {
                    array[j] = (byte)j;
                }

                string text = "Cp1252";
                if (PdfName.MAC_ROMAN_ENCODING.Equals(encoding))
                {
                    text = "MacRoman";
                }
                else if (PdfName.SYMBOL.Equals(encoding))
                {
                    text = "Symbol";
                }
                else if (PdfName.ZAPFDINGBATS.Equals(encoding))
                {
                    text = "ZapfDingbats";
                }

                char[] array2 = PdfEncodings.ConvertToString(array, text).ToCharArray();
                for (int k = 0; k < 256; k++)
                {
                    uni2byte[array2[k]] = k;
                    byte2uni[k] = array2[k];
                }

                base.encoding = text;
            }
            else
            {
                for (int l = 0; l < 256; l++)
                {
                    uni2byte[stdEnc[l]] = l;
                    byte2uni[l] = stdEnc[l];
                }
            }
        }

        public override float GetFontDescriptor(int key, float fontSize)
        {
            if (cjkMirror != null)
            {
                return cjkMirror.GetFontDescriptor(key, fontSize);
            }

            switch (key)
            {
                case 1:
                case 9:
                    return Ascender * fontSize / 1000f;
                case 2:
                    return CapHeight * fontSize / 1000f;
                case 3:
                case 10:
                    return Descender * fontSize / 1000f;
                case 4:
                    return ItalicAngle;
                case 5:
                    return llx * fontSize / 1000f;
                case 6:
                    return lly * fontSize / 1000f;
                case 7:
                    return urx * fontSize / 1000f;
                case 8:
                    return ury * fontSize / 1000f;
                case 11:
                    return 0f;
                case 12:
                    return (urx - llx) * fontSize / 1000f;
                case 23:
                    return fontWeight * fontSize / 1000f;
                default:
                    return 0f;
            }
        }

        public override int GetKerning(int char1, int char2)
        {
            return 0;
        }

        internal override int GetRawWidth(int c, string name)
        {
            return 0;
        }

        public override bool HasKernPairs()
        {
            return false;
        }

        internal override void WriteFont(PdfWriter writer, PdfIndirectReference refi, object[] param)
        {
        }

        public override PdfStream GetFullFontStream()
        {
            return null;
        }

        public override int GetWidth(int char1)
        {
            if (isType0)
            {
                if (hMetrics != null && cjkMirror != null && !cjkMirror.IsVertical())
                {
                    int cidCode = cjkMirror.GetCidCode(char1);
                    int num = hMetrics[cidCode];
                    if (num > 0)
                    {
                        return num;
                    }

                    return defaultWidth;
                }

                int[] value = null;
                metrics.TryGetValue(char1, out value);
                if (value != null)
                {
                    return value[1];
                }

                return 0;
            }

            if (cjkMirror != null)
            {
                return cjkMirror.GetWidth(char1);
            }

            return base.GetWidth(char1);
        }

        public override int GetWidth(string text)
        {
            if (isType0)
            {
                int num = 0;
                if (hMetrics != null && cjkMirror != null && !cjkMirror.IsVertical())
                {
                    if (((CJKFont)cjkMirror).IsIdentity())
                    {
                        for (int i = 0; i < text.Length; i++)
                        {
                            num += GetWidth(text[i]);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < text.Length; j++)
                        {
                            int @char;
                            if (Utilities.IsSurrogatePair(text, j))
                            {
                                @char = Utilities.ConvertToUtf32(text, j);
                                j++;
                            }
                            else
                            {
                                @char = text[j];
                            }

                            num += GetWidth(@char);
                        }
                    }
                }
                else
                {
                    char[] array = text.ToCharArray();
                    int num2 = array.Length;
                    for (int k = 0; k < num2; k++)
                    {
                        int[] value = null;
                        metrics.TryGetValue(array[k], out value);
                        if (value != null)
                        {
                            num += value[1];
                        }
                    }
                }

                return num;
            }

            if (cjkMirror != null)
            {
                return cjkMirror.GetWidth(text);
            }

            return base.GetWidth(text);
        }

        public override byte[] ConvertToBytes(string text)
        {
            if (cjkMirror != null)
            {
                return cjkMirror.ConvertToBytes(text);
            }

            if (isType0)
            {
                char[] array = text.ToCharArray();
                int num = array.Length;
                byte[] array2 = new byte[num * 2];
                int num2 = 0;
                for (int i = 0; i < num; i++)
                {
                    metrics.TryGetValue(array[i], out var value);
                    if (value != null)
                    {
                        int num3 = value[0];
                        array2[num2++] = (byte)(num3 / 256);
                        array2[num2++] = (byte)num3;
                    }
                }

                if (num2 == array2.Length)
                {
                    return array2;
                }

                byte[] array3 = new byte[num2];
                Array.Copy(array2, 0, array3, 0, num2);
                return array3;
            }

            char[] array4 = text.ToCharArray();
            byte[] array5 = new byte[array4.Length];
            int num4 = 0;
            for (int j = 0; j < array4.Length; j++)
            {
                if (uni2byte.ContainsKey(array4[j]))
                {
                    array5[num4++] = (byte)uni2byte[array4[j]];
                }
            }

            if (num4 == array5.Length)
            {
                return array5;
            }

            byte[] array6 = new byte[num4];
            Array.Copy(array5, 0, array6, 0, num4);
            return array6;
        }

        internal override byte[] ConvertToBytes(int char1)
        {
            if (cjkMirror != null)
            {
                return cjkMirror.ConvertToBytes(char1);
            }

            if (isType0)
            {
                metrics.TryGetValue(char1, out var value);
                if (value != null)
                {
                    int num = value[0];
                    return new byte[2]
                    {
                        (byte)(num / 256),
                        (byte)num
                    };
                }

                return new byte[0];
            }

            if (uni2byte.ContainsKey(char1))
            {
                return new byte[1] { (byte)uni2byte[char1] };
            }

            return new byte[0];
        }

        public override bool CharExists(int c)
        {
            if (cjkMirror != null)
            {
                return cjkMirror.CharExists(c);
            }

            if (isType0)
            {
                return metrics.ContainsKey(c);
            }

            return base.CharExists(c);
        }

        public override bool SetKerning(int char1, int char2, int kern)
        {
            return false;
        }

        public override int[] GetCharBBox(int c)
        {
            return null;
        }

        public override bool IsVertical()
        {
            if (cjkMirror != null)
            {
                return cjkMirror.IsVertical();
            }

            return base.IsVertical();
        }

        protected override int[] GetRawCharBBox(int c, string name)
        {
            return null;
        }

        private bool IsSymbolic()
        {
            PdfDictionary asDict = font.GetAsDict(PdfName.FONTDESCRIPTOR);
            if (asDict == null)
            {
                return false;
            }

            PdfNumber asNumber = asDict.GetAsNumber(PdfName.FLAGS);
            if (asNumber == null)
            {
                return false;
            }

            return (asNumber.IntValue & 4) != 0;
        }
    }
}
