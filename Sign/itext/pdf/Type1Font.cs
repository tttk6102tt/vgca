using Sign.itext.error_messages;
using Sign.itext.io;
using Sign.itext.text.pdf;
using Sign.SystemItext.util;
using System.Globalization;

namespace Sign.itext.pdf
{
    internal class Type1Font : BaseFont
    {
        private object lockObject = new object();

        protected byte[] pfb;

        private string FontName;

        private string FullName;

        private string FamilyName;

        private string Weight = "";

        private float ItalicAngle;

        private bool IsFixedPitch;

        private string CharacterSet;

        private int llx = -50;

        private int lly = -200;

        private int urx = 1000;

        private int ury = 900;

        private int UnderlinePosition = -100;

        private int UnderlineThickness = 50;

        private string EncodingScheme = "FontSpecific";

        private int CapHeight = 700;

        private int XHeight = 480;

        private int Ascender = 800;

        private int Descender = -200;

        private int StdHW;

        private int StdVW = 80;

        private Dictionary<object, object[]> CharMetrics = new Dictionary<object, object[]>();

        private Dictionary<string, object[]> KernPairs = new Dictionary<string, object[]>();

        private string fileName;

        private bool builtinFont;

        private static readonly int[] PFB_TYPES = new int[3] { 1, 2, 1 };

        public override string PostscriptFontName
        {
            get
            {
                return FontName;
            }
            set
            {
                FontName = value;
            }
        }

        public override string[][] FullFontName => new string[1][] { new string[4] { "", "", "", FullName } };

        public override string[][] AllNameEntries => new string[1][] { new string[5] { "4", "", "", "", FullName } };

        public override string[][] FamilyFontName => new string[1][] { new string[4] { "", "", "", FamilyName } };

        internal Type1Font(string afmFile, string enc, bool emb, byte[] ttfAfm, byte[] pfb, bool forceRead)
        {
            if (emb && ttfAfm != null && pfb == null)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("two.byte.arrays.are.needed.if.the.type1.font.is.embedded"));
            }

            if (emb && ttfAfm != null)
            {
                this.pfb = pfb;
            }

            encoding = enc;
            embedded = emb;
            fileName = afmFile;
            FontType = 0;
            RandomAccessFileOrArray randomAccessFileOrArray = null;
            Stream stream = null;
            if (BaseFont.BuiltinFonts14.ContainsKey(afmFile))
            {
                embedded = false;
                builtinFont = true;
                byte[] array = new byte[1024];
                try
                {
                    stream = StreamUtil.GetResourceStream("text.pdf.fonts." + afmFile);// + ".afm");
                    if (stream == null)
                    {
                        string composedMessage = MessageLocalization.GetComposedMessage("1.not.found.as.resource", afmFile);
                        Console.Error.WriteLine(composedMessage);
                        throw new DocumentException(composedMessage);
                    }

                    MemoryStream memoryStream = new MemoryStream();
                    while (true)
                    {
                        int num = stream.Read(array, 0, array.Length);
                        if (num == 0)
                        {
                            break;
                        }

                        memoryStream.Write(array, 0, num);
                    }

                    array = memoryStream.ToArray();
                }
                finally
                {
                    if (stream != null)
                    {
                        try
                        {
                            stream.Close();
                        }
                        catch
                        {
                        }
                    }
                }

                try
                {
                    randomAccessFileOrArray = new RandomAccessFileOrArray(array);
                    Process(randomAccessFileOrArray);
                }
                finally
                {
                    if (randomAccessFileOrArray != null)
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
            }
            else if (afmFile.ToLower(CultureInfo.InvariantCulture).EndsWith(".afm"))
            {
                try
                {
                    randomAccessFileOrArray = ((ttfAfm != null) ? new RandomAccessFileOrArray(ttfAfm) : new RandomAccessFileOrArray(afmFile, forceRead));
                    Process(randomAccessFileOrArray);
                }
                finally
                {
                    if (randomAccessFileOrArray != null)
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
            }
            else
            {
                if (!afmFile.ToLower(CultureInfo.InvariantCulture).EndsWith(".pfm"))
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.an.afm.or.pfm.font.file", afmFile));
                }

                try
                {
                    MemoryStream memoryStream2 = new MemoryStream();
                    randomAccessFileOrArray = ((ttfAfm != null) ? new RandomAccessFileOrArray(ttfAfm) : new RandomAccessFileOrArray(afmFile, forceRead));
                    Pfm2afm.Convert(randomAccessFileOrArray, memoryStream2);
                    randomAccessFileOrArray.Close();
                    randomAccessFileOrArray = new RandomAccessFileOrArray(memoryStream2.ToArray());
                    Process(randomAccessFileOrArray);
                }
                finally
                {
                    if (randomAccessFileOrArray != null)
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
            }

            EncodingScheme = EncodingScheme.Trim();
            if (EncodingScheme.Equals("AdobeStandardEncoding") || EncodingScheme.Equals("StandardEncoding"))
            {
                fontSpecific = false;
            }

            if (!encoding.StartsWith("#"))
            {
                PdfEncodings.ConvertToBytes(" ", enc);
            }

            CreateEncoding();
        }

        internal override int GetRawWidth(int c, string name)
        {
            object[] value;
            if (name == null)
            {
                CharMetrics.TryGetValue(c, out value);
            }
            else
            {
                if (name.Equals(".notdef"))
                {
                    return 0;
                }

                CharMetrics.TryGetValue(name, out value);
            }

            if (value != null)
            {
                return (int)value[1];
            }

            return 0;
        }

        public override int GetKerning(int char1, int char2)
        {
            string text = GlyphList.UnicodeToName(char1);
            if (text == null)
            {
                return 0;
            }

            string text2 = GlyphList.UnicodeToName(char2);
            if (text2 == null)
            {
                return 0;
            }

            KernPairs.TryGetValue(text, out var value);
            if (value == null)
            {
                return 0;
            }

            for (int i = 0; i < value.Length; i += 2)
            {
                if (text2.Equals(value[i]))
                {
                    return (int)value[i + 1];
                }
            }

            return 0;
        }

        public virtual void Process(RandomAccessFileOrArray rf)
        {
            bool flag = false;
            string str;
            while ((str = rf.ReadLine()) != null)
            {
                StringTokenizer stringTokenizer = new StringTokenizer(str, " ,\n\r\t\f");
                if (stringTokenizer.HasMoreTokens())
                {
                    string text = stringTokenizer.NextToken();
                    if (text.Equals("FontName"))
                    {
                        FontName = stringTokenizer.NextToken("ÿ").Substring(1);
                    }
                    else if (text.Equals("FullName"))
                    {
                        FullName = stringTokenizer.NextToken("ÿ").Substring(1);
                    }
                    else if (text.Equals("FamilyName"))
                    {
                        FamilyName = stringTokenizer.NextToken("ÿ").Substring(1);
                    }
                    else if (text.Equals("Weight"))
                    {
                        Weight = stringTokenizer.NextToken("ÿ").Substring(1);
                    }
                    else if (text.Equals("ItalicAngle"))
                    {
                        ItalicAngle = float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("IsFixedPitch"))
                    {
                        IsFixedPitch = stringTokenizer.NextToken().Equals("true");
                    }
                    else if (text.Equals("CharacterSet"))
                    {
                        CharacterSet = stringTokenizer.NextToken("ÿ").Substring(1);
                    }
                    else if (text.Equals("FontBBox"))
                    {
                        llx = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                        lly = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                        urx = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                        ury = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("UnderlinePosition"))
                    {
                        UnderlinePosition = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("UnderlineThickness"))
                    {
                        UnderlineThickness = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("EncodingScheme"))
                    {
                        EncodingScheme = stringTokenizer.NextToken("ÿ").Substring(1);
                    }
                    else if (text.Equals("CapHeight"))
                    {
                        CapHeight = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("XHeight"))
                    {
                        XHeight = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("Ascender"))
                    {
                        Ascender = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("Descender"))
                    {
                        Descender = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("StdHW"))
                    {
                        StdHW = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("StdVW"))
                    {
                        StdVW = (int)float.Parse(stringTokenizer.NextToken(), NumberFormatInfo.InvariantInfo);
                    }
                    else if (text.Equals("StartCharMetrics"))
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (!flag)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("missing.startcharmetrics.in.1", fileName));
            }

            while ((str = rf.ReadLine()) != null)
            {
                StringTokenizer stringTokenizer2 = new StringTokenizer(str);
                if (!stringTokenizer2.HasMoreTokens())
                {
                    continue;
                }

                string text2 = stringTokenizer2.NextToken();
                if (text2.Equals("EndCharMetrics"))
                {
                    flag = false;
                    break;
                }

                int num = -1;
                int num2 = 250;
                string text3 = "";
                int[] array = null;
                stringTokenizer2 = new StringTokenizer(str, ";");
                while (stringTokenizer2.HasMoreTokens())
                {
                    StringTokenizer stringTokenizer3 = new StringTokenizer(stringTokenizer2.NextToken());
                    if (stringTokenizer3.HasMoreTokens())
                    {
                        text2 = stringTokenizer3.NextToken();
                        if (text2.Equals("C"))
                        {
                            num = int.Parse(stringTokenizer3.NextToken());
                        }
                        else if (text2.Equals("WX"))
                        {
                            num2 = (int)float.Parse(stringTokenizer3.NextToken(), NumberFormatInfo.InvariantInfo);
                        }
                        else if (text2.Equals("N"))
                        {
                            text3 = stringTokenizer3.NextToken();
                        }
                        else if (text2.Equals("B"))
                        {
                            array = new int[4]
                            {
                                int.Parse(stringTokenizer3.NextToken()),
                                int.Parse(stringTokenizer3.NextToken()),
                                int.Parse(stringTokenizer3.NextToken()),
                                int.Parse(stringTokenizer3.NextToken())
                            };
                        }
                    }
                }

                object[] value = new object[4] { num, num2, text3, array };
                if (num >= 0)
                {
                    CharMetrics[num] = value;
                }

                CharMetrics[text3] = value;
            }

            if (flag)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("missing.endcharmetrics.in.1", fileName));
            }

            if (!CharMetrics.ContainsKey("nonbreakingspace"))
            {
                CharMetrics.TryGetValue("space", out var value2);
                if (value2 != null)
                {
                    CharMetrics["nonbreakingspace"] = value2;
                }
            }

            while ((str = rf.ReadLine()) != null)
            {
                StringTokenizer stringTokenizer4 = new StringTokenizer(str);
                if (stringTokenizer4.HasMoreTokens())
                {
                    string text4 = stringTokenizer4.NextToken();
                    if (text4.Equals("EndFontMetrics"))
                    {
                        return;
                    }

                    if (text4.Equals("StartKernPairs"))
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (!flag)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("missing.endfontmetrics.in.1", fileName));
            }

            while ((str = rf.ReadLine()) != null)
            {
                StringTokenizer stringTokenizer5 = new StringTokenizer(str);
                if (!stringTokenizer5.HasMoreTokens())
                {
                    continue;
                }

                string text5 = stringTokenizer5.NextToken();
                if (text5.Equals("KPX"))
                {
                    string key = stringTokenizer5.NextToken();
                    string text6 = stringTokenizer5.NextToken();
                    int num3 = (int)float.Parse(stringTokenizer5.NextToken(), NumberFormatInfo.InvariantInfo);
                    KernPairs.TryGetValue(key, out var value3);
                    if (value3 == null)
                    {
                        KernPairs[key] = new object[2] { text6, num3 };
                        continue;
                    }

                    int num4 = value3.Length;
                    object[] array2 = new object[num4 + 2];
                    Array.Copy(value3, 0, array2, 0, num4);
                    array2[num4] = text6;
                    array2[num4 + 1] = num3;
                    KernPairs[key] = array2;
                }
                else if (text5.Equals("EndKernPairs"))
                {
                    flag = false;
                    break;
                }
            }

            if (flag)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("missing.endkernpairs.in.1", fileName));
            }

            rf.Close();
        }

        public override PdfStream GetFullFontStream()
        {
            if (builtinFont || !embedded)
            {
                return null;
            }

            lock (lockObject)
            {
                RandomAccessFileOrArray randomAccessFileOrArray = null;
                try
                {
                    string text = fileName.Substring(0, fileName.Length - 3) + "pfb";
                    randomAccessFileOrArray = ((pfb != null) ? new RandomAccessFileOrArray(pfb) : new RandomAccessFileOrArray(text, forceRead: true));
                    byte[] array = new byte[(int)randomAccessFileOrArray.Length - 18];
                    int[] array2 = new int[3];
                    int num = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (randomAccessFileOrArray.Read() != 128)
                        {
                            throw new DocumentException(MessageLocalization.GetComposedMessage("start.marker.missing.in.1", text));
                        }

                        if (randomAccessFileOrArray.Read() != PFB_TYPES[i])
                        {
                            throw new DocumentException(MessageLocalization.GetComposedMessage("incorrect.segment.type.in.1", text));
                        }

                        int num2 = randomAccessFileOrArray.Read();
                        num2 += randomAccessFileOrArray.Read() << 8;
                        num2 += randomAccessFileOrArray.Read() << 16;
                        num2 = (array2[i] = num2 + (randomAccessFileOrArray.Read() << 24));
                        while (num2 != 0)
                        {
                            int num3 = randomAccessFileOrArray.Read(array, num, num2);
                            if (num3 < 0)
                            {
                                throw new DocumentException(MessageLocalization.GetComposedMessage("premature.end.in.1", text));
                            }

                            num += num3;
                            num2 -= num3;
                        }
                    }

                    return new StreamFont(array, array2, compressionLevel);
                }
                finally
                {
                    if (randomAccessFileOrArray != null)
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
            }
        }

        public virtual PdfDictionary GetFontDescriptor(PdfIndirectReference fontStream)
        {
            if (builtinFont)
            {
                return null;
            }

            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONTDESCRIPTOR);
            pdfDictionary.Put(PdfName.ASCENT, new PdfNumber(Ascender));
            pdfDictionary.Put(PdfName.CAPHEIGHT, new PdfNumber(CapHeight));
            pdfDictionary.Put(PdfName.DESCENT, new PdfNumber(Descender));
            pdfDictionary.Put(PdfName.FONTBBOX, new PdfRectangle(llx, lly, urx, ury));
            pdfDictionary.Put(PdfName.FONTNAME, new PdfName(FontName));
            pdfDictionary.Put(PdfName.ITALICANGLE, new PdfNumber(ItalicAngle));
            pdfDictionary.Put(PdfName.STEMV, new PdfNumber(StdVW));
            if (fontStream != null)
            {
                pdfDictionary.Put(PdfName.FONTFILE, fontStream);
            }

            int num = 0;
            if (IsFixedPitch)
            {
                num |= 1;
            }

            num |= (fontSpecific ? 4 : 32);
            if (ItalicAngle < 0f)
            {
                num |= 0x40;
            }

            if (FontName.IndexOf("Caps") >= 0 || FontName.EndsWith("SC"))
            {
                num |= 0x20000;
            }

            if (Weight.Equals("Bold"))
            {
                num |= 0x40000;
            }

            pdfDictionary.Put(PdfName.FLAGS, new PdfNumber(num));
            return pdfDictionary;
        }

        private PdfDictionary GetFontBaseType(PdfIndirectReference fontDescriptor, int firstChar, int lastChar, byte[] shortTag)
        {
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONT);
            pdfDictionary.Put(PdfName.SUBTYPE, PdfName.TYPE1);
            pdfDictionary.Put(PdfName.BASEFONT, new PdfName(FontName));
            bool flag = encoding.Equals("Cp1252") || encoding.Equals("MacRoman");
            if (!fontSpecific || specialMap != null)
            {
                for (int i = firstChar; i <= lastChar; i++)
                {
                    if (!differences[i].Equals(".notdef"))
                    {
                        firstChar = i;
                        break;
                    }
                }

                if (flag)
                {
                    pdfDictionary.Put(PdfName.ENCODING, encoding.Equals("Cp1252") ? PdfName.WIN_ANSI_ENCODING : PdfName.MAC_ROMAN_ENCODING);
                }
                else
                {
                    PdfDictionary pdfDictionary2 = new PdfDictionary(PdfName.ENCODING);
                    PdfArray pdfArray = new PdfArray();
                    bool flag2 = true;
                    for (int j = firstChar; j <= lastChar; j++)
                    {
                        if (shortTag[j] != 0)
                        {
                            if (flag2)
                            {
                                pdfArray.Add(new PdfNumber(j));
                                flag2 = false;
                            }

                            pdfArray.Add(new PdfName(differences[j]));
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }

                    pdfDictionary2.Put(PdfName.DIFFERENCES, pdfArray);
                    pdfDictionary.Put(PdfName.ENCODING, pdfDictionary2);
                }
            }

            if (specialMap != null || forceWidthsOutput || !builtinFont || !(fontSpecific || flag))
            {
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
            }

            if (!builtinFont && fontDescriptor != null)
            {
                pdfDictionary.Put(PdfName.FONTDESCRIPTOR, fontDescriptor);
            }

            return pdfDictionary;
        }

        internal override void WriteFont(PdfWriter writer, PdfIndirectReference piref, object[] parms)
        {
            int firstChar = (int)parms[0];
            int lastChar = (int)parms[1];
            byte[] array = (byte[])parms[2];
            if (!(bool)parms[3] || !subset)
            {
                firstChar = 0;
                lastChar = array.Length - 1;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = 1;
                }
            }

            PdfIndirectReference pdfIndirectReference = null;
            PdfObject pdfObject = null;
            pdfObject = GetFullFontStream();
            if (pdfObject != null)
            {
                pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
            }

            pdfObject = GetFontDescriptor(pdfIndirectReference);
            if (pdfObject != null)
            {
                pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
            }

            pdfObject = GetFontBaseType(pdfIndirectReference, firstChar, lastChar, array);
            writer.AddToBody(pdfObject, piref);
        }

        public override float GetFontDescriptor(int key, float fontSize)
        {
            switch (key)
            {
                case 1:
                case 9:
                    return (float)Ascender * fontSize / 1000f;
                case 2:
                    return (float)CapHeight * fontSize / 1000f;
                case 3:
                case 10:
                    return (float)Descender * fontSize / 1000f;
                case 4:
                    return ItalicAngle;
                case 5:
                    return (float)llx * fontSize / 1000f;
                case 6:
                    return (float)lly * fontSize / 1000f;
                case 7:
                    return (float)urx * fontSize / 1000f;
                case 8:
                    return (float)ury * fontSize / 1000f;
                case 11:
                    return 0f;
                case 12:
                    return (float)(urx - llx) * fontSize / 1000f;
                case 13:
                    return (float)UnderlinePosition * fontSize / 1000f;
                case 14:
                    return (float)UnderlineThickness * fontSize / 1000f;
                default:
                    return 0f;
            }
        }

        public override void SetFontDescriptor(int key, float value)
        {
            switch (key)
            {
                case 1:
                case 9:
                    Ascender = (int)value;
                    break;
                case 3:
                case 10:
                    Descender = (int)value;
                    break;
            }
        }

        public override bool HasKernPairs()
        {
            return KernPairs.Count > 0;
        }

        public override bool SetKerning(int char1, int char2, int kern)
        {
            string text = GlyphList.UnicodeToName(char1);
            if (text == null)
            {
                return false;
            }

            string text2 = GlyphList.UnicodeToName(char2);
            if (text2 == null)
            {
                return false;
            }

            KernPairs.TryGetValue(text, out var value);
            if (value == null)
            {
                value = new object[2] { text2, kern };
                KernPairs[text] = value;
                return true;
            }

            for (int i = 0; i < value.Length; i += 2)
            {
                if (text2.Equals(value[i]))
                {
                    value[i + 1] = kern;
                    return true;
                }
            }

            int num = value.Length;
            object[] array = new object[num + 2];
            Array.Copy(value, 0, array, 0, num);
            array[num] = text2;
            array[num + 1] = kern;
            KernPairs[text] = array;
            return true;
        }

        protected override int[] GetRawCharBBox(int c, string name)
        {
            object[] value;
            if (name == null)
            {
                CharMetrics.TryGetValue(c, out value);
            }
            else
            {
                if (name.Equals(".notdef"))
                {
                    return null;
                }

                CharMetrics.TryGetValue(name, out value);
            }

            if (value != null)
            {
                return (int[])value[3];
            }

            return null;
        }
    }
}
