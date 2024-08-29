using Sign.itext.error_messages;
using Sign.itext.pdf;
using System.Globalization;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class TrueTypeFontUnicode : TrueTypeFont, IComparer<int[]>
    {
        private static readonly byte[] rotbits = new byte[8] { 128, 64, 32, 16, 8, 4, 2, 1 };

        internal TrueTypeFontUnicode(string ttFile, string enc, bool emb, byte[] ttfAfm, bool forceRead)
        {
            string baseName = Sign.itext.pdf.BaseFont.GetBaseName(ttFile);
            string tTCName = TrueTypeFont.GetTTCName(baseName);
            if (baseName.Length < ttFile.Length)
            {
                style = ttFile.Substring(baseName.Length);
            }

            encoding = enc;
            embedded = emb;
            fileName = tTCName;
            ttcIndex = "";
            if (tTCName.Length < baseName.Length)
            {
                ttcIndex = baseName.Substring(tTCName.Length + 1);
            }

            FontType = 3;
            if ((fileName.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") || fileName.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") || fileName.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttc")) && (enc.Equals("Identity-H") || enc.Equals("Identity-V")) && emb)
            {
                Process(ttfAfm, forceRead);
                if (os_2.fsType == 2)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("1.cannot.be.embedded.due.to.licensing.restrictions", fileName + style));
                }

                if ((cmap31 == null && !fontSpecific) || (cmap10 == null && fontSpecific))
                {
                    directTextToByte = true;
                }

                if (fontSpecific)
                {
                    fontSpecific = false;
                    string text = encoding;
                    encoding = "";
                    CreateEncoding();
                    encoding = text;
                    fontSpecific = true;
                }

                vertical = enc.EndsWith("V");
                return;
            }

            throw new DocumentException(MessageLocalization.GetComposedMessage("1.2.is.not.a.ttf.font.file", fileName, style));
        }

        public override int GetWidth(int char1)
        {
            if (vertical)
            {
                return 1000;
            }

            if (fontSpecific)
            {
                if ((char1 & 0xFF00) == 0 || (char1 & 0xFF00) == 61440)
                {
                    return GetRawWidth(char1 & 0xFF, null);
                }

                return 0;
            }

            return GetRawWidth(char1, encoding);
        }

        public override int GetWidth(string text)
        {
            if (vertical)
            {
                return text.Length * 1000;
            }

            int num = 0;
            if (fontSpecific)
            {
                char[] array = text.ToCharArray();
                int num2 = array.Length;
                for (int i = 0; i < num2; i++)
                {
                    char c = array[i];
                    if ((c & 0xFF00) == 0 || (c & 0xFF00) == 61440)
                    {
                        num += GetRawWidth(c & 0xFF, null);
                    }
                }
            }
            else
            {
                int length = text.Length;
                for (int j = 0; j < length; j++)
                {
                    if (Utilities.IsSurrogatePair(text, j))
                    {
                        num += GetRawWidth(Utilities.ConvertToUtf32(text, j), encoding);
                        j++;
                    }
                    else
                    {
                        num += GetRawWidth(text[j], encoding);
                    }
                }
            }

            return num;
        }

        public virtual PdfStream GetToUnicode(object[] metrics)
        {
            if (metrics.Length == 0)
            {
                return null;
            }

            StringBuilder stringBuilder = new StringBuilder("/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\n/CIDSystemInfo\n<< /Registry (TTX+0)\n/Ordering (T42UV)\n/Supplement 0\n>> def\n/CMapName /TTX+0 def\n/CMapType 2 def\n1 begincodespacerange\n<0000><FFFF>\nendcodespacerange\n");
            int num = 0;
            for (int i = 0; i < metrics.Length; i++)
            {
                if (num == 0)
                {
                    if (i != 0)
                    {
                        stringBuilder.Append("endbfrange\n");
                    }

                    num = Math.Min(100, metrics.Length - i);
                    stringBuilder.Append(num).Append(" beginbfrange\n");
                }

                num--;
                int[] array = (int[])metrics[i];
                string value = ToHex(array[0]);
                stringBuilder.Append(value).Append(value).Append(ToHex(array[2]))
                    .Append('\n');
            }

            stringBuilder.Append("endbfrange\nendcmap\nCMapName currentdict /CMap defineresource pop\nend end\n");
            PdfStream pdfStream = new PdfStream(PdfEncodings.ConvertToBytes(stringBuilder.ToString(), null));
            pdfStream.FlateCompress(compressionLevel);
            return pdfStream;
        }

        internal static string ToHex(int n)
        {
            if (n < 65536)
            {
                return "<" + Convert.ToString(n, 16).PadLeft(4, '0') + ">";
            }

            n -= 65536;
            int num = n / 1024 + 55296;
            int num2 = n % 1024 + 56320;
            return "[<" + Convert.ToString(num, 16).PadLeft(4, '0') + Convert.ToString(num2, 16).PadLeft(4, '0') + ">]";
        }

        public virtual PdfDictionary GetCIDFontType2(PdfIndirectReference fontDescriptor, string subsetPrefix, object[] metrics)
        {
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONT);
            if (cff)
            {
                pdfDictionary.Put(PdfName.SUBTYPE, PdfName.CIDFONTTYPE0);
                pdfDictionary.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName + "-" + encoding));
            }
            else
            {
                pdfDictionary.Put(PdfName.SUBTYPE, PdfName.CIDFONTTYPE2);
                pdfDictionary.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName));
            }

            pdfDictionary.Put(PdfName.FONTDESCRIPTOR, fontDescriptor);
            if (!cff)
            {
                pdfDictionary.Put(PdfName.CIDTOGIDMAP, PdfName.IDENTITY);
            }

            PdfDictionary pdfDictionary2 = new PdfDictionary();
            pdfDictionary2.Put(PdfName.REGISTRY, new PdfString("Adobe"));
            pdfDictionary2.Put(PdfName.ORDERING, new PdfString("Identity"));
            pdfDictionary2.Put(PdfName.SUPPLEMENT, new PdfNumber(0));
            pdfDictionary.Put(PdfName.CIDSYSTEMINFO, pdfDictionary2);
            if (!vertical)
            {
                pdfDictionary.Put(PdfName.DW, new PdfNumber(1000));
                StringBuilder stringBuilder = new StringBuilder("[");
                int num = -10;
                bool flag = true;
                for (int i = 0; i < metrics.Length; i++)
                {
                    int[] array = (int[])metrics[i];
                    if (array[1] == 1000)
                    {
                        continue;
                    }

                    int num2 = array[0];
                    if (num2 == num + 1)
                    {
                        stringBuilder.Append(' ').Append(array[1]);
                    }
                    else
                    {
                        if (!flag)
                        {
                            stringBuilder.Append(']');
                        }

                        flag = false;
                        stringBuilder.Append(num2).Append('[').Append(array[1]);
                    }

                    num = num2;
                }

                if (stringBuilder.Length > 1)
                {
                    stringBuilder.Append("]]");
                    pdfDictionary.Put(PdfName.W, new PdfLiteral(stringBuilder.ToString()));
                }
            }

            return pdfDictionary;
        }

        public virtual PdfDictionary GetFontBaseType(PdfIndirectReference descendant, string subsetPrefix, PdfIndirectReference toUnicode)
        {
            PdfDictionary pdfDictionary = new PdfDictionary(PdfName.FONT);
            pdfDictionary.Put(PdfName.SUBTYPE, PdfName.TYPE0);
            if (cff)
            {
                pdfDictionary.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName + "-" + encoding));
            }
            else
            {
                pdfDictionary.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName));
            }

            pdfDictionary.Put(PdfName.ENCODING, new PdfName(encoding));
            pdfDictionary.Put(PdfName.DESCENDANTFONTS, new PdfArray(descendant));
            if (toUnicode != null)
            {
                pdfDictionary.Put(PdfName.TOUNICODE, toUnicode);
            }

            return pdfDictionary;
        }

        public virtual int GetCharFromGlyphId(int gid)
        {
            if (glyphIdToChar == null)
            {
                int[] array = new int[maxGlyphId];
                Dictionary<int, int[]> dictionary = null;
                if (cmapExt != null)
                {
                    dictionary = cmapExt;
                }
                else if (cmap31 != null)
                {
                    dictionary = cmap31;
                }

                if (dictionary != null)
                {
                    foreach (KeyValuePair<int, int[]> item in dictionary)
                    {
                        array[item.Value[0]] = item.Key;
                    }
                }

                glyphIdToChar = array;
            }

            return glyphIdToChar[gid];
        }

        public virtual int Compare(int[] o1, int[] o2)
        {
            int num = o1[0];
            int num2 = o2[0];
            if (num < num2)
            {
                return -1;
            }

            if (num == num2)
            {
                return 0;
            }

            return 1;
        }

        internal override void WriteFont(PdfWriter writer, PdfIndirectReference piref, object[] parms)
        {
            writer.GetTtfUnicodeWriter().WriteFont(this, piref, parms, rotbits);
        }

        public override PdfStream GetFullFontStream()
        {
            if (cff)
            {
                return new StreamFont(ReadCffFont(), "CIDFontType0C", compressionLevel);
            }

            return base.GetFullFontStream();
        }

        public override byte[] ConvertToBytes(string text)
        {
            return null;
        }

        internal override byte[] ConvertToBytes(int char1)
        {
            return null;
        }

        public override int[] GetMetricsTT(int c)
        {
            int[] value;
            if (cmapExt != null)
            {
                cmapExt.TryGetValue(c, out value);
                return value;
            }

            Dictionary<int, int[]> dictionary = null;
            dictionary = ((!fontSpecific) ? cmap31 : cmap10);
            if (dictionary == null)
            {
                return null;
            }

            if (fontSpecific)
            {
                if ((c & 0xFFFFFF00u) == 0L || (c & 0xFFFFFF00u) == 61440)
                {
                    dictionary.TryGetValue(c & 0xFF, out value);
                    return value;
                }

                return null;
            }

            dictionary.TryGetValue(c, out value);
            return value;
        }

        public override bool CharExists(int c)
        {
            return GetMetricsTT(c) != null;
        }

        public override bool SetCharAdvance(int c, int advance)
        {
            int[] metricsTT = GetMetricsTT(c);
            if (metricsTT == null)
            {
                return false;
            }

            metricsTT[1] = advance;
            return true;
        }

        public override int[] GetCharBBox(int c)
        {
            if (bboxes == null)
            {
                return null;
            }

            int[] metricsTT = GetMetricsTT(c);
            if (metricsTT == null)
            {
                return null;
            }

            return bboxes[metricsTT[0]];
        }
    }
}
