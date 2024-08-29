using Sign.itext.error_messages;
using Sign.itext.io;
using Sign.itext.pdf.fonts.cmaps;
using Sign.itext.text;
using Sign.itext.text.pdf;
using Sign.SystemItext.util;
using System.Text;
namespace Sign.itext.pdf
{
    internal class CJKFont : BaseFont
    {
        internal const string CJK_ENCODING = "UNICODEBIGUNMARKED";

        private const int FIRST = 0;

        private const int BRACKET = 1;

        private const int SERIAL = 2;

        private const int V1Y = 880;

        internal static Sign.SystemItext.util.Properties cjkFonts = new Sign.SystemItext.util.Properties();

        internal static Sign.SystemItext.util.Properties cjkEncodings = new Sign.SystemItext.util.Properties();

        private static Dictionary<string, Dictionary<string, object>> allFonts = new Dictionary<string, Dictionary<string, object>>();

        private static bool propertiesLoaded = false;

        public const string RESOURCE_PATH_CMAP = "text.pdf.fonts.cmaps.";

        private static Dictionary<string, Dictionary<string, object>> registryNames = new Dictionary<string, Dictionary<string, object>>();

        private CMapCidByte cidByte;

        private CMapUniCid uniCid;

        private CMapCidUni cidUni;

        private string uniMap;

        private string fontName;

        private string style = "";

        private string CMap;

        private bool cidDirect;

        private IntHashtable vMetrics;

        private IntHashtable hMetrics;

        private Dictionary<string, object> fontDesc;

        private static readonly char[] cspace = new char[1] { ' ' };

        internal string UniMap => uniMap;

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

        public override string[][] FullFontName => new string[1][] { new string[4] { "", "", "", fontName } };

        public override string[][] AllNameEntries => new string[1][] { new string[5] { "4", "", "", "", fontName } };

        public override string[][] FamilyFontName => FullFontName;

        private static void LoadProperties()
        {
            if (propertiesLoaded)
            {
                return;
            }

            lock (allFonts)
            {
                if (propertiesLoaded)
                {
                    return;
                }

                try
                {
                    LoadRegistry();
                    foreach (string key in registryNames["fonts"].Keys)
                    {
                        allFonts[key] = ReadFontProperties(key);
                    }
                }
                catch
                {
                }

                propertiesLoaded = true;
            }
        }

        private static void LoadRegistry()
        {
            Stream resourceStream = StreamUtil.GetResourceStream("text.pdf.fonts.cmaps.cjk_registry.properties");
            Sign.SystemItext.util.Properties properties = new Sign.SystemItext.util.Properties();
            properties.Load(resourceStream);
            resourceStream.Close();
            foreach (string key2 in properties.Keys)
            {
                string[] array = properties[key2].Split(cspace, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                string[] array2 = array;
                foreach (string key in array2)
                {
                    dictionary[key] = null;
                }

                registryNames[key2] = dictionary;
            }
        }

        internal CJKFont(string fontName, string enc, bool emb)
        {
            LoadProperties();
            FontType = 2;
            string baseName = BaseFont.GetBaseName(fontName);
            if (!IsCJKFont(baseName, enc))
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("font.1.with.2.encoding.is.not.a.cjk.font", fontName, enc));
            }

            if (baseName.Length < fontName.Length)
            {
                style = fontName.Substring(baseName.Length);
                fontName = baseName;
            }

            this.fontName = fontName;
            encoding = "UNICODEBIGUNMARKED";
            vertical = enc.EndsWith("V");
            CMap = enc;
            if (enc.Equals("Identity-H") || enc.Equals("Identity-V"))
            {
                cidDirect = true;
            }

            LoadCMaps();
        }

        private void LoadCMaps()
        {
            try
            {
                fontDesc = allFonts[fontName];
                hMetrics = (IntHashtable)fontDesc["W"];
                vMetrics = (IntHashtable)fontDesc["W2"];
                string text = (string)fontDesc["Registry"];
                uniMap = "";
                foreach (string key in registryNames[text + "_Uni"].Keys)
                {
                    string text2 = (uniMap = key);
                    if ((text2.EndsWith("V") && vertical) || (!text2.EndsWith("V") && !vertical))
                    {
                        break;
                    }
                }

                if (cidDirect)
                {
                    cidUni = CMapCache.GetCachedCMapCidUni(uniMap);
                    return;
                }

                uniCid = CMapCache.GetCachedCMapUniCid(uniMap);
                cidByte = CMapCache.GetCachedCMapCidByte(CMap);
            }
            catch (Exception ex)
            {
                throw new DocumentException(ex.Message);
            }
        }

        public static string GetCompatibleFont(string enc)
        {
            LoadProperties();
            string text = null;
            foreach (KeyValuePair<string, Dictionary<string, object>> registryName in registryNames)
            {
                if (!registryName.Value.ContainsKey(enc))
                {
                    continue;
                }

                text = registryName.Key;
                foreach (KeyValuePair<string, Dictionary<string, object>> allFont in allFonts)
                {
                    if (text.Equals(allFont.Value["Registry"]))
                    {
                        return allFont.Key;
                    }
                }
            }

            return null;
        }

        public static bool IsCJKFont(string fontName, string enc)
        {
            LoadProperties();
            if (!registryNames.ContainsKey("fonts"))
            {
                return false;
            }

            if (!registryNames["fonts"].ContainsKey(fontName))
            {
                return false;
            }

            if (enc.Equals("Identity-H") || enc.Equals("Identity-V"))
            {
                return true;
            }

            string key = allFonts[fontName]["Registry"] as string;
            registryNames.TryGetValue(key, out var value);
            return value?.ContainsKey(enc) ?? false;
        }

        public override int GetWidth(int char1)
        {
            int key = char1;
            if (!cidDirect)
            {
                key = uniCid.Lookup(char1);
            }

            int num = ((!vertical) ? hMetrics[key] : vMetrics[key]);
            if (num > 0)
            {
                return num;
            }

            return 1000;
        }

        public override int GetWidth(string text)
        {
            int num = 0;
            if (cidDirect)
            {
                foreach (char @char in text)
                {
                    num += GetWidth(@char);
                }
            }
            else
            {
                for (int j = 0; j < text.Length; j++)
                {
                    int char2;
                    if (Utilities.IsSurrogatePair(text, j))
                    {
                        char2 = Utilities.ConvertToUtf32(text, j);
                        j++;
                    }
                    else
                    {
                        char2 = text[j];
                    }

                    num += GetWidth(char2);
                }
            }

            return num;
        }

        internal override int GetRawWidth(int c, string name)
        {
            return 0;
        }

        public override int GetKerning(int char1, int char2)
        {
            return 0;
        }

        private PdfDictionary GetFontDescriptor()
        {
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONTDESCRIPTOR);
            pdfDictionary.Put(PdfName.ASCENT, new PdfLiteral((string)fontDesc["Ascent"]));
            pdfDictionary.Put(PdfName.CAPHEIGHT, new PdfLiteral((string)fontDesc["CapHeight"]));
            pdfDictionary.Put(PdfName.DESCENT, new PdfLiteral((string)fontDesc["Descent"]));
            pdfDictionary.Put(PdfName.FLAGS, new PdfLiteral((string)fontDesc["Flags"]));
            pdfDictionary.Put(PdfName.FONTBBOX, new PdfLiteral((string)fontDesc["FontBBox"]));
            pdfDictionary.Put(PdfName.FONTNAME, new PdfName(fontName + style));
            pdfDictionary.Put(PdfName.ITALICANGLE, new PdfLiteral((string)fontDesc["ItalicAngle"]));
            pdfDictionary.Put(PdfName.STEMV, new PdfLiteral((string)fontDesc["StemV"]));
            PdfDictionary pdfDictionary2 = new PdfDictionary();
            pdfDictionary2.Put(PdfName.PANOSE, new PdfString((string)fontDesc["Panose"], null));
            pdfDictionary.Put(PdfName.STYLE, pdfDictionary2);
            return pdfDictionary;
        }

        private PdfDictionary GetCIDFont(PdfIndirectReference fontDescriptor, IntHashtable cjkTag)
        {
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONT);
            pdfDictionary.Put(PdfName.SUBTYPE, PdfName.CIDFONTTYPE0);
            pdfDictionary.Put(PdfName.BASEFONT, new PdfName(fontName + style));
            pdfDictionary.Put(PdfName.FONTDESCRIPTOR, fontDescriptor);
            int[] keys = cjkTag.ToOrderedKeys();
            string text = ConvertToHCIDMetrics(keys, hMetrics);
            if (text != null)
            {
                pdfDictionary.Put(PdfName.W, new PdfLiteral(text));
            }

            if (vertical)
            {
                text = ConvertToVCIDMetrics(keys, vMetrics, hMetrics);
                if (text != null)
                {
                    pdfDictionary.Put(PdfName.W2, new PdfLiteral(text));
                }
            }
            else
            {
                pdfDictionary.Put(PdfName.DW, new PdfNumber(1000));
            }

            PdfDictionary pdfDictionary2 = new PdfDictionary();
            if (cidDirect)
            {
                pdfDictionary2.Put(PdfName.REGISTRY, new PdfString(cidUni.Registry, null));
                pdfDictionary2.Put(PdfName.ORDERING, new PdfString(cidUni.Ordering, null));
                pdfDictionary2.Put(PdfName.SUPPLEMENT, new PdfNumber(cidUni.Supplement));
            }
            else
            {
                pdfDictionary2.Put(PdfName.REGISTRY, new PdfString(cidByte.Registry, null));
                pdfDictionary2.Put(PdfName.ORDERING, new PdfString(cidByte.Ordering, null));
                pdfDictionary2.Put(PdfName.SUPPLEMENT, new PdfNumber(cidByte.Supplement));
            }

            pdfDictionary.Put(PdfName.CIDSYSTEMINFO, pdfDictionary2);
            return pdfDictionary;
        }

        private PdfDictionary GetFontBaseType(PdfIndirectReference CIDFont)
        {
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONT);
            pdfDictionary.Put(PdfName.SUBTYPE, PdfName.TYPE0);
            string text = fontName;
            if (style.Length > 0)
            {
                text = text + "-" + style.Substring(1);
            }

            pdfDictionary.Put(value: new PdfName(text + "-" + CMap), key: PdfName.BASEFONT);
            pdfDictionary.Put(PdfName.ENCODING, new PdfName(CMap));
            pdfDictionary.Put(PdfName.DESCENDANTFONTS, new PdfArray(CIDFont));
            return pdfDictionary;
        }

        internal override void WriteFont(PdfWriter writer, PdfIndirectReference piref, object[] parms)
        {
            IntHashtable cjkTag = (IntHashtable)parms[0];
            PdfIndirectReference pdfIndirectReference = null;
            PdfObject pdfObject = null;
            pdfObject = GetFontDescriptor();
            if (pdfObject != null)
            {
                pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
            }

            pdfObject = GetCIDFont(pdfIndirectReference, cjkTag);
            if (pdfObject != null)
            {
                pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
            }

            pdfObject = GetFontBaseType(pdfIndirectReference);
            writer.AddToBody(pdfObject, piref);
        }

        public override PdfStream GetFullFontStream()
        {
            return null;
        }

        private float GetDescNumber(string name)
        {
            return int.Parse((string)fontDesc[name]);
        }

        private float GetBBox(int idx)
        {
            StringTokenizer stringTokenizer = new StringTokenizer((string)fontDesc["FontBBox"], " []\r\n\t\f");
            string s = stringTokenizer.NextToken();
            for (int i = 0; i < idx; i++)
            {
                s = stringTokenizer.NextToken();
            }

            return int.Parse(s);
        }

        public override float GetFontDescriptor(int key, float fontSize)
        {
            switch (key)
            {
                case 1:
                case 9:
                    return GetDescNumber("Ascent") * fontSize / 1000f;
                case 2:
                    return GetDescNumber("CapHeight") * fontSize / 1000f;
                case 3:
                case 10:
                    return GetDescNumber("Descent") * fontSize / 1000f;
                case 4:
                    return GetDescNumber("ItalicAngle");
                case 5:
                    return fontSize * GetBBox(0) / 1000f;
                case 6:
                    return fontSize * GetBBox(1) / 1000f;
                case 7:
                    return fontSize * GetBBox(2) / 1000f;
                case 8:
                    return fontSize * GetBBox(3) / 1000f;
                case 11:
                    return 0f;
                case 12:
                    return fontSize * (GetBBox(2) - GetBBox(0)) / 1000f;
                default:
                    return 0f;
            }
        }

        internal static IntHashtable CreateMetric(string s)
        {
            IntHashtable intHashtable = new IntHashtable();
            StringTokenizer stringTokenizer = new StringTokenizer(s);
            while (stringTokenizer.HasMoreTokens())
            {
                int key = int.Parse(stringTokenizer.NextToken());
                intHashtable[key] = int.Parse(stringTokenizer.NextToken());
            }

            return intHashtable;
        }

        internal static string ConvertToHCIDMetrics(int[] keys, IntHashtable h)
        {
            if (keys.Length == 0)
            {
                return null;
            }

            int num = 0;
            int num2 = 0;
            int i;
            for (i = 0; i < keys.Length; i++)
            {
                num = keys[i];
                num2 = h[num];
                if (num2 != 0)
                {
                    i++;
                    break;
                }
            }

            if (num2 == 0)
            {
                return null;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('[');
            stringBuilder.Append(num);
            int num3 = 0;
            for (int j = i; j < keys.Length; j++)
            {
                int num4 = keys[j];
                int num5 = h[num4];
                if (num5 == 0)
                {
                    continue;
                }

                switch (num3)
                {
                    case 0:
                        if (num4 == num + 1 && num5 == num2)
                        {
                            num3 = 2;
                        }
                        else if (num4 == num + 1)
                        {
                            num3 = 1;
                            stringBuilder.Append('[').Append(num2);
                        }
                        else
                        {
                            stringBuilder.Append('[').Append(num2).Append(']')
                                .Append(num4);
                        }

                        break;
                    case 1:
                        if (num4 == num + 1 && num5 == num2)
                        {
                            num3 = 2;
                            stringBuilder.Append(']').Append(num);
                        }
                        else if (num4 == num + 1)
                        {
                            stringBuilder.Append(' ').Append(num2);
                        }
                        else
                        {
                            num3 = 0;
                            stringBuilder.Append(' ').Append(num2).Append(']')
                                .Append(num4);
                        }

                        break;
                    case 2:
                        if (num4 != num + 1 || num5 != num2)
                        {
                            stringBuilder.Append(' ').Append(num).Append(' ')
                                .Append(num2)
                                .Append(' ')
                                .Append(num4);
                            num3 = 0;
                        }

                        break;
                }

                num2 = num5;
                num = num4;
            }

            switch (num3)
            {
                case 0:
                    stringBuilder.Append('[').Append(num2).Append("]]");
                    break;
                case 1:
                    stringBuilder.Append(' ').Append(num2).Append("]]");
                    break;
                case 2:
                    stringBuilder.Append(' ').Append(num).Append(' ')
                        .Append(num2)
                        .Append(']');
                    break;
            }

            return stringBuilder.ToString();
        }

        internal static string ConvertToVCIDMetrics(int[] keys, IntHashtable v, IntHashtable h)
        {
            if (keys.Length == 0)
            {
                return null;
            }

            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int i;
            for (i = 0; i < keys.Length; i++)
            {
                num = keys[i];
                num2 = v[num];
                if (num2 != 0)
                {
                    i++;
                    break;
                }

                num3 = h[num];
            }

            if (num2 == 0)
            {
                return null;
            }

            if (num3 == 0)
            {
                num3 = 1000;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('[');
            stringBuilder.Append(num);
            int num4 = 0;
            for (int j = i; j < keys.Length; j++)
            {
                int num5 = keys[j];
                int num6 = v[num5];
                if (num6 == 0)
                {
                    continue;
                }

                int num7 = h[num];
                if (num7 == 0)
                {
                    num7 = 1000;
                }

                switch (num4)
                {
                    case 0:
                        if (num5 == num + 1 && num6 == num2 && num7 == num3)
                        {
                            num4 = 2;
                        }
                        else
                        {
                            stringBuilder.Append(' ').Append(num).Append(' ')
                                .Append(-num2)
                                .Append(' ')
                                .Append(num3 / 2)
                                .Append(' ')
                                .Append(880)
                                .Append(' ')
                                .Append(num5);
                        }

                        break;
                    case 2:
                        if (num5 != num + 1 || num6 != num2 || num7 != num3)
                        {
                            stringBuilder.Append(' ').Append(num).Append(' ')
                                .Append(-num2)
                                .Append(' ')
                                .Append(num3 / 2)
                                .Append(' ')
                                .Append(880)
                                .Append(' ')
                                .Append(num5);
                            num4 = 0;
                        }

                        break;
                }

                num2 = num6;
                num = num5;
                num3 = num7;
            }

            stringBuilder.Append(' ').Append(num).Append(' ')
                .Append(-num2)
                .Append(' ')
                .Append(num3 / 2)
                .Append(' ')
                .Append(880)
                .Append(" ]");
            return stringBuilder.ToString();
        }

        internal static Dictionary<string, object> ReadFontProperties(string name)
        {
            name += ".properties";
            Stream resourceStream = StreamUtil.GetResourceStream("text.pdf.fonts.cmaps." + name);
            Sign.SystemItext.util.Properties properties = new Sign.SystemItext.util.Properties();
            properties.Load(resourceStream);
            resourceStream.Close();
            IntHashtable value = CreateMetric(properties["W"]);
            properties.Remove("W");
            IntHashtable value2 = CreateMetric(properties["W2"]);
            properties.Remove("W2");
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (string key in properties.Keys)
            {
                dictionary[key] = properties[key];
            }

            dictionary["W"] = value;
            dictionary["W2"] = value2;
            return dictionary;
        }

        public override int GetUnicodeEquivalent(int c)
        {
            if (cidDirect)
            {
                if (c == 32767)
                {
                    return 10;
                }

                return cidUni.Lookup(c);
            }

            return c;
        }

        public override int GetCidCode(int c)
        {
            if (cidDirect)
            {
                return c;
            }

            return uniCid.Lookup(c);
        }

        public override bool HasKernPairs()
        {
            return false;
        }

        public override bool CharExists(int c)
        {
            if (cidDirect)
            {
                return true;
            }

            return cidByte.Lookup(uniCid.Lookup(c)).Length != 0;
        }

        public override bool SetCharAdvance(int c, int advance)
        {
            return false;
        }

        public override bool SetKerning(int char1, int char2, int kern)
        {
            return false;
        }

        public override int[] GetCharBBox(int c)
        {
            return null;
        }

        protected override int[] GetRawCharBBox(int c, string name)
        {
            return null;
        }

        public override byte[] ConvertToBytes(string text)
        {
            if (cidDirect)
            {
                return base.ConvertToBytes(text);
            }

            if (text.Length == 1)
            {
                return ConvertToBytes(text[0]);
            }

            MemoryStream memoryStream = new MemoryStream();
            for (int i = 0; i < text.Length; i++)
            {
                int @char;
                if (Utilities.IsSurrogatePair(text, i))
                {
                    @char = Utilities.ConvertToUtf32(text, i);
                    i++;
                }
                else
                {
                    @char = text[i];
                }

                byte[] array = ConvertToBytes(@char);
                memoryStream.Write(array, 0, array.Length);
            }

            return memoryStream.ToArray();
        }

        internal override byte[] ConvertToBytes(int char1)
        {
            if (cidDirect)
            {
                return base.ConvertToBytes(char1);
            }

            return cidByte.Lookup(uniCid.Lookup(char1));
        }

        public virtual bool IsIdentity()
        {
            return cidDirect;
        }
    }
}
