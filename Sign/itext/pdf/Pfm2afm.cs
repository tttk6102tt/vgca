using Sign.itext.error_messages;
using System.Globalization;
using System.Text;

namespace Sign.itext.pdf
{
    public sealed class Pfm2afm
    {
        private RandomAccessFileOrArray inp;

        private StreamWriter outp;

        private Encoding encoding;

        private short vers;

        private int h_len;

        private string copyright;

        private short type;

        private short points;

        private short verres;

        private short horres;

        private short ascent;

        private short intleading;

        private short extleading;

        private byte italic;

        private byte uline;

        private byte overs;

        private short weight;

        private byte charset;

        private short pixwidth;

        private short pixheight;

        private byte kind;

        private short avgwidth;

        private short maxwidth;

        private int firstchar;

        private int lastchar;

        private byte defchar;

        private byte brkchar;

        private short widthby;

        private int device;

        private int face;

        private int bits;

        private int bitoff;

        private short extlen;

        private int psext;

        private int chartab;

        private int res1;

        private int kernpairs;

        private int res2;

        private int fontname;

        private short capheight;

        private short xheight;

        private short ascender;

        private short descender;

        private bool isMono;

        private int[] Win2PSStd = new int[256]
        {
            0, 0, 0, 0, 197, 198, 199, 0, 202, 0,
            205, 206, 207, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 32, 33, 34, 35, 36, 37, 38, 169,
            40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
            50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
            60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
            70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
            80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
            90, 91, 92, 93, 94, 95, 193, 97, 98, 99,
            100, 101, 102, 103, 104, 105, 106, 107, 108, 109,
            110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
            120, 121, 122, 123, 124, 125, 126, 127, 128, 0,
            184, 166, 185, 188, 178, 179, 195, 189, 0, 172,
            234, 0, 0, 0, 0, 96, 0, 170, 186, 183,
            177, 208, 196, 0, 0, 173, 250, 0, 0, 0,
            0, 161, 162, 163, 168, 165, 0, 167, 200, 0,
            227, 171, 0, 0, 0, 197, 0, 0, 0, 0,
            194, 0, 182, 180, 203, 0, 235, 187, 0, 0,
            0, 191, 0, 0, 0, 0, 0, 0, 225, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 233, 0, 0, 0,
            0, 0, 0, 251, 0, 0, 0, 0, 0, 0,
            241, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 249, 0,
            0, 0, 0, 0, 0, 0
        };

        private int[] WinClass = new int[256]
        {
            0, 0, 0, 0, 2, 2, 2, 0, 2, 0,
            2, 2, 2, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 2, 0, 0,
            2, 0, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 0, 0, 0, 3, 3, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 0, 0, 2,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1
        };

        private string[] WinChars = new string[256]
        {
            "W00", "W01", "W02", "W03", "macron", "breve", "dotaccent", "W07", "ring", "W09",
            "W0a", "W0b", "W0c", "W0d", "W0e", "W0f", "hungarumlaut", "ogonek", "caron", "W13",
            "W14", "W15", "W16", "W17", "W18", "W19", "W1a", "W1b", "W1c", "W1d",
            "W1e", "W1f", "space", "exclam", "quotedbl", "numbersign", "dollar", "percent", "ampersand", "quotesingle",
            "parenleft", "parenright", "asterisk", "plus", "comma", "hyphen", "period", "slash", "zero", "one",
            "two", "three", "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon",
            "less", "equal", "greater", "question", "at", "A", "B", "C", "D", "E",
            "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
            "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y",
            "Z", "bracketleft", "backslash", "bracketright", "asciicircum", "underscore", "grave", "a", "b", "c",
            "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w",
            "x", "y", "z", "braceleft", "bar", "braceright", "asciitilde", "W7f", "euro", "W81",
            "quotesinglbase", "florin", "quotedblbase", "ellipsis", "dagger", "daggerdbl", "circumflex", "perthousand", "Scaron", "guilsinglleft",
            "OE", "W8d", "Zcaron", "W8f", "W90", "quoteleft", "quoteright", "quotedblleft", "quotedblright", "bullet",
            "endash", "emdash", "tilde", "trademark", "scaron", "guilsinglright", "oe", "W9d", "zcaron", "Ydieresis",
            "reqspace", "exclamdown", "cent", "sterling", "currency", "yen", "brokenbar", "section", "dieresis", "copyright",
            "ordfeminine", "guillemotleft", "logicalnot", "syllable", "registered", "macron", "degree", "plusminus", "twosuperior", "threesuperior",
            "acute", "mu", "paragraph", "periodcentered", "cedilla", "onesuperior", "ordmasculine", "guillemotright", "onequarter", "onehalf",
            "threequarters", "questiondown", "Agrave", "Aacute", "Acircumflex", "Atilde", "Adieresis", "Aring", "AE", "Ccedilla",
            "Egrave", "Eacute", "Ecircumflex", "Edieresis", "Igrave", "Iacute", "Icircumflex", "Idieresis", "Eth", "Ntilde",
            "Ograve", "Oacute", "Ocircumflex", "Otilde", "Odieresis", "multiply", "Oslash", "Ugrave", "Uacute", "Ucircumflex",
            "Udieresis", "Yacute", "Thorn", "germandbls", "agrave", "aacute", "acircumflex", "atilde", "adieresis", "aring",
            "ae", "ccedilla", "egrave", "eacute", "ecircumflex", "edieresis", "igrave", "iacute", "icircumflex", "idieresis",
            "eth", "ntilde", "ograve", "oacute", "ocircumflex", "otilde", "odieresis", "divide", "oslash", "ugrave",
            "uacute", "ucircumflex", "udieresis", "yacute", "thorn", "ydieresis"
        };

        private Pfm2afm(RandomAccessFileOrArray inp, Stream outp)
        {
            this.inp = inp;
            encoding = Encoding.GetEncoding(1252);
            this.outp = new StreamWriter(outp, encoding);
        }

        public static void Convert(RandomAccessFileOrArray inp, Stream outp)
        {
            Pfm2afm pfm2afm = new Pfm2afm(inp, outp);
            pfm2afm.Openpfm();
            pfm2afm.Putheader();
            pfm2afm.Putchartab();
            pfm2afm.Putkerntab();
            pfm2afm.Puttrailer();
            pfm2afm.outp.Flush();
        }

        private string ReadString(int n)
        {
            byte[] array = new byte[n];
            inp.ReadFully(array);
            int i;
            for (i = 0; i < array.Length && array[i] != 0; i++)
            {
            }

            return encoding.GetString(array, 0, i);
        }

        private string ReadString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                int num = inp.Read();
                if (num <= 0)
                {
                    break;
                }

                stringBuilder.Append((char)num);
            }

            return stringBuilder.ToString();
        }

        private void Outval(int n)
        {
            outp.Write(' ');
            outp.Write(n);
        }

        private void Outchar(int code, int width, string name)
        {
            outp.Write("C ");
            Outval(code);
            outp.Write(" ; WX ");
            Outval(width);
            if (name != null)
            {
                outp.Write(" ; N ");
                outp.Write(name);
            }

            outp.Write(" ;\n");
        }

        private void Openpfm()
        {
            inp.Seek(0);
            vers = inp.ReadShortLE();
            h_len = inp.ReadIntLE();
            copyright = ReadString(60);
            type = inp.ReadShortLE();
            points = inp.ReadShortLE();
            verres = inp.ReadShortLE();
            horres = inp.ReadShortLE();
            ascent = inp.ReadShortLE();
            intleading = inp.ReadShortLE();
            extleading = inp.ReadShortLE();
            italic = (byte)inp.Read();
            uline = (byte)inp.Read();
            overs = (byte)inp.Read();
            weight = inp.ReadShortLE();
            charset = (byte)inp.Read();
            pixwidth = inp.ReadShortLE();
            pixheight = inp.ReadShortLE();
            kind = (byte)inp.Read();
            avgwidth = inp.ReadShortLE();
            maxwidth = inp.ReadShortLE();
            firstchar = inp.Read();
            lastchar = inp.Read();
            defchar = (byte)inp.Read();
            brkchar = (byte)inp.Read();
            widthby = inp.ReadShortLE();
            device = inp.ReadIntLE();
            face = inp.ReadIntLE();
            bits = inp.ReadIntLE();
            bitoff = inp.ReadIntLE();
            extlen = inp.ReadShortLE();
            psext = inp.ReadIntLE();
            chartab = inp.ReadIntLE();
            res1 = inp.ReadIntLE();
            kernpairs = inp.ReadIntLE();
            res2 = inp.ReadIntLE();
            fontname = inp.ReadIntLE();
            if (h_len != inp.Length || extlen != 30 || fontname < 75 || fontname > 512)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("not.a.valid.pfm.file"));
            }

            inp.Seek(psext + 14);
            capheight = inp.ReadShortLE();
            xheight = inp.ReadShortLE();
            ascender = inp.ReadShortLE();
            descender = inp.ReadShortLE();
        }

        private void Putheader()
        {
            outp.Write("StartFontMetrics 2.0\n");
            if (copyright.Length > 0)
            {
                outp.Write("Comment " + copyright + "\n");
            }

            outp.Write("FontName ");
            inp.Seek(fontname);
            string text = ReadString();
            outp.Write(text);
            outp.Write("\nEncodingScheme ");
            if (charset != 0)
            {
                outp.Write("FontSpecific\n");
            }
            else
            {
                outp.Write("AdobeStandardEncoding\n");
            }

            outp.Write("FullName " + text.Replace('-', ' '));
            if (face != 0)
            {
                inp.Seek(face);
                outp.Write("\nFamilyName " + ReadString());
            }

            outp.Write("\nWeight ");
            if (weight > 475 || text.ToLower(CultureInfo.InvariantCulture).IndexOf("bold") >= 0)
            {
                outp.Write("Bold");
            }
            else if ((weight < 325 && weight != 0) || text.ToLower(CultureInfo.InvariantCulture).IndexOf("light") >= 0)
            {
                outp.Write("Light");
            }
            else if (text.ToLower(CultureInfo.InvariantCulture).IndexOf("black") >= 0)
            {
                outp.Write("Black");
            }
            else
            {
                outp.Write("Medium");
            }

            outp.Write("\nItalicAngle ");
            if (italic != 0 || text.ToLower(CultureInfo.InvariantCulture).IndexOf("italic") >= 0)
            {
                outp.Write("-12.00");
            }
            else
            {
                outp.Write("0");
            }

            outp.Write("\nIsFixedPitch ");
            if ((kind & 1) == 0 || avgwidth == maxwidth)
            {
                outp.Write("true");
                isMono = true;
            }
            else
            {
                outp.Write("false");
                isMono = false;
            }

            outp.Write("\nFontBBox");
            if (isMono)
            {
                Outval(-20);
            }
            else
            {
                Outval(-100);
            }

            Outval(-(descender + 5));
            Outval(maxwidth + 10);
            Outval(ascent + 5);
            outp.Write("\nCapHeight");
            Outval(capheight);
            outp.Write("\nXHeight");
            Outval(xheight);
            outp.Write("\nDescender");
            Outval(-descender);
            outp.Write("\nAscender");
            Outval(ascender);
            outp.Write('\n');
        }

        private void Putchartab()
        {
            int num = lastchar - firstchar + 1;
            int[] array = new int[num];
            inp.Seek(chartab);
            for (int i = 0; i < num; i++)
            {
                array[i] = inp.ReadUnsignedShortLE();
            }

            int[] array2 = new int[256];
            if (charset == 0)
            {
                for (int j = firstchar; j <= lastchar; j++)
                {
                    if (Win2PSStd[j] != 0)
                    {
                        array2[Win2PSStd[j]] = j;
                    }
                }
            }

            outp.Write("StartCharMetrics");
            Outval(num);
            outp.Write('\n');
            if (charset != 0)
            {
                for (int k = firstchar; k <= lastchar; k++)
                {
                    if (array[k - firstchar] != 0)
                    {
                        Outchar(k, array[k - firstchar], null);
                    }
                }
            }
            else
            {
                for (int l = 0; l < 256; l++)
                {
                    int num2 = array2[l];
                    if (num2 != 0)
                    {
                        Outchar(l, array[num2 - firstchar], WinChars[num2]);
                        array[num2 - firstchar] = 0;
                    }
                }

                for (int m = firstchar; m <= lastchar; m++)
                {
                    if (array[m - firstchar] != 0)
                    {
                        Outchar(-1, array[m - firstchar], WinChars[m]);
                    }
                }
            }

            outp.Write("EndCharMetrics\n");
        }

        private void Putkerntab()
        {
            if (kernpairs == 0)
            {
                return;
            }

            inp.Seek(kernpairs);
            int num = inp.ReadUnsignedShortLE();
            int num2 = 0;
            int[] array = new int[num * 3];
            int num3 = 0;
            while (num3 < array.Length)
            {
                array[num3++] = inp.Read();
                array[num3++] = inp.Read();
                if ((array[num3++] = inp.ReadShortLE()) != 0)
                {
                    num2++;
                }
            }

            if (num2 == 0)
            {
                return;
            }

            outp.Write("StartKernData\nStartKernPairs");
            Outval(num2);
            outp.Write('\n');
            for (int i = 0; i < array.Length; i += 3)
            {
                if (array[i + 2] != 0)
                {
                    outp.Write("KPX ");
                    outp.Write(WinChars[array[i]]);
                    outp.Write(' ');
                    outp.Write(WinChars[array[i + 1]]);
                    Outval(array[i + 2]);
                    outp.Write('\n');
                }
            }

            outp.Write("EndKernPairs\nEndKernData\n");
        }

        private void Puttrailer()
        {
            outp.Write("EndFontMetrics\n");
        }
    }
}
