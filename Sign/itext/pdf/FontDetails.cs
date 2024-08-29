using Sign.itext.text;
using Sign.itext.text.pdf;
using System.Text;

namespace Sign.itext.pdf
{
    public class FontDetails
    {
        private PdfIndirectReference indirectReference;

        private PdfName fontName;

        private BaseFont baseFont;

        private TrueTypeFontUnicode ttu;

        private CJKFont cjkFont;

        private byte[] shortTag;

        private Dictionary<int, int[]> longTag;

        private IntHashtable cjkTag;

        private int fontType;

        private bool symbolic;

        protected bool subset = true;

        internal PdfIndirectReference IndirectReference => indirectReference;

        internal PdfName FontName => fontName;

        internal BaseFont BaseFont => baseFont;

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

        internal FontDetails(PdfName fontName, PdfIndirectReference indirectReference, BaseFont baseFont)
        {
            this.fontName = fontName;
            this.indirectReference = indirectReference;
            this.baseFont = baseFont;
            fontType = baseFont.FontType;
            switch (fontType)
            {
                case 0:
                case 1:
                    shortTag = new byte[256];
                    break;
                case 2:
                    cjkTag = new IntHashtable();
                    cjkFont = (CJKFont)baseFont;
                    break;
                case 3:
                    longTag = new Dictionary<int, int[]>();
                    ttu = (TrueTypeFontUnicode)baseFont;
                    symbolic = baseFont.IsFontSpecific();
                    break;
            }
        }

        internal virtual object[] ConvertToBytesGid(string gids)
        {
            if (fontType != 3)
            {
                throw new ArgumentException("GID require TT Unicode");
            }

            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                int num = 0;
                char[] array = gids.ToCharArray();
                foreach (char c in array)
                {
                    int glyphWidth = ttu.GetGlyphWidth(c);
                    num += glyphWidth;
                    int charFromGlyphId = ttu.GetCharFromGlyphId(c);
                    if (charFromGlyphId != 0)
                    {
                        stringBuilder.Append(Utilities.ConvertFromUtf32(charFromGlyphId));
                    }

                    int key = c;
                    if (!longTag.ContainsKey(key))
                    {
                        longTag[key] = new int[3] { c, glyphWidth, charFromGlyphId };
                    }
                }

                return new object[3]
                {
                    Encoding.GetEncoding("UNICODEBIGUNMARKED").GetBytes(gids),
                    stringBuilder.ToString(),
                    num
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal byte[] ConvertToBytes(string text)
        {
            byte[] array = null;
            switch (fontType)
            {
                case 5:
                    return baseFont.ConvertToBytes(text);
                case 0:
                case 1:
                    {
                        array = baseFont.ConvertToBytes(text);
                        int num3 = array.Length;
                        for (int m = 0; m < num3; m++)
                        {
                            shortTag[array[m] & 0xFF] = 1;
                        }

                        break;
                    }
                case 2:
                    {
                        int length3 = text.Length;
                        if (cjkFont.IsIdentity())
                        {
                            foreach (char key2 in text)
                            {
                                cjkTag[key2] = 0;
                            }
                        }
                        else
                        {
                            for (int l = 0; l < length3; l++)
                            {
                                int c;
                                if (Utilities.IsSurrogatePair(text, l))
                                {
                                    c = Utilities.ConvertToUtf32(text, l);
                                    l++;
                                }
                                else
                                {
                                    c = text[l];
                                }

                                cjkTag[cjkFont.GetCidCode(c)] = 0;
                            }
                        }

                        array = cjkFont.ConvertToBytes(text);
                        break;
                    }
                case 4:
                    array = baseFont.ConvertToBytes(text);
                    break;
                case 3:
                    {
                        int length = text.Length;
                        int[] array2 = null;
                        char[] array3 = new char[length];
                        int length2 = 0;
                        if (symbolic)
                        {
                            array = PdfEncodings.ConvertToBytes(text, "symboltt");
                            length = array.Length;
                            for (int i = 0; i < length; i++)
                            {
                                array2 = ttu.GetMetricsTT(array[i] & 0xFF);
                                if (array2 != null)
                                {
                                    longTag[array2[0]] = new int[3]
                                    {
                                array2[0],
                                array2[1],
                                ttu.GetUnicodeDifferences(array[i] & 0xFF)
                                    };
                                    array3[length2++] = (char)array2[0];
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < length; j++)
                            {
                                int num;
                                if (Utilities.IsSurrogatePair(text, j))
                                {
                                    num = Utilities.ConvertToUtf32(text, j);
                                    j++;
                                }
                                else
                                {
                                    num = text[j];
                                }

                                array2 = ttu.GetMetricsTT(num);
                                if (array2 != null)
                                {
                                    int num2 = array2[0];
                                    int key = num2;
                                    if (!longTag.ContainsKey(key))
                                    {
                                        longTag[key] = new int[3]
                                        {
                                    num2,
                                    array2[1],
                                    num
                                        };
                                    }

                                    array3[length2++] = (char)num2;
                                }
                            }
                        }

                        array = PdfEncodings.ConvertToBytes(new string(array3, 0, length2), "UNICODEBIGUNMARKED");
                        break;
                    }
            }

            return array;
        }

        public virtual void WriteFont(PdfWriter writer)
        {
            switch (fontType)
            {
                case 5:
                    baseFont.WriteFont(writer, indirectReference, null);
                    break;
                case 0:
                case 1:
                    {
                        int i;
                        for (i = 0; i < 256 && shortTag[i] == 0; i++)
                        {
                        }

                        int num = 255;
                        while (num >= i && shortTag[num] == 0)
                        {
                            num--;
                        }

                        if (i > 255)
                        {
                            i = 255;
                            num = 255;
                        }

                        baseFont.WriteFont(writer, indirectReference, new object[4] { i, num, shortTag, subset });
                        break;
                    }
                case 2:
                    baseFont.WriteFont(writer, indirectReference, new object[1] { cjkTag });
                    break;
                case 3:
                    baseFont.WriteFont(writer, indirectReference, new object[2] { longTag, subset });
                    break;
                case 4:
                    break;
            }
        }
    }
}
