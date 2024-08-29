using Sign.itext.text.pdf;

namespace Sign.itext.pdf.fonts.cmaps
{
    public abstract class AbstractCMap
    {
        private string cmapName;

        private string registry;

        private string ordering;

        private int supplement;

        public virtual int Supplement
        {
            get
            {
                return supplement;
            }
            set
            {
                supplement = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return cmapName;
            }
            set
            {
                cmapName = value;
            }
        }

        public virtual string Ordering
        {
            get
            {
                return ordering;
            }
            set
            {
                ordering = value;
            }
        }

        public virtual string Registry
        {
            get
            {
                return registry;
            }
            set
            {
                registry = value;
            }
        }

        internal abstract void AddChar(PdfString mark, PdfObject code);

        internal void AddRange(PdfString from, PdfString to, PdfObject code)
        {
            byte[] array = DecodeStringToByte(from);
            byte[] array2 = DecodeStringToByte(to);
            if (array.Length != array2.Length || array.Length == 0)
            {
                throw new ArgumentException("Invalid map.");
            }

            byte[] array3 = null;
            if (code is PdfString)
            {
                array3 = DecodeStringToByte((PdfString)code);
            }

            int num = array[^1] & 0xFF;
            int num2 = array2[^1] & 0xFF;
            for (int i = num; i <= num2; i++)
            {
                array[^1] = (byte)i;
                PdfString pdfString = new PdfString(array);
                pdfString.SetHexWriting(hexWriting: true);
                if (code is PdfArray)
                {
                    AddChar(pdfString, ((PdfArray)code)[i - num]);
                }
                else if (code is PdfNumber)
                {
                    int value = ((PdfNumber)code).IntValue + i - num;
                    AddChar(pdfString, new PdfNumber(value));
                }
                else if (code is PdfString)
                {
                    PdfString pdfString2 = new PdfString(array3);
                    pdfString2.SetHexWriting(hexWriting: true);
                    array3[^1]++;
                    AddChar(pdfString, pdfString2);
                }
            }
        }

        public static byte[] DecodeStringToByte(PdfString s)
        {
            byte[] bytes = s.GetBytes();
            byte[] array = new byte[bytes.Length];
            Array.Copy(bytes, 0, array, 0, bytes.Length);
            return array;
        }

        public virtual string DecodeStringToUnicode(PdfString ps)
        {
            if (ps.IsHexWriting())
            {
                return PdfEncodings.ConvertToString(ps.GetBytes(), "UnicodeBigUnmarked");
            }

            return ps.ToUnicodeString();
        }
    }
}
