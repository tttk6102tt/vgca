using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.factories;

namespace Sign.itext.text.pdf
{
    public class PdfPageLabels
    {
        public class PdfPageLabelFormat
        {
            public int physicalPage;

            public int numberStyle;

            public string prefix;

            public int logicalPage;

            public PdfPageLabelFormat(int physicalPage, int numberStyle, string prefix, int logicalPage)
            {
                this.physicalPage = physicalPage;
                this.numberStyle = numberStyle;
                this.prefix = prefix;
                this.logicalPage = logicalPage;
            }
        }

        public const int DECIMAL_ARABIC_NUMERALS = 0;

        public const int UPPERCASE_ROMAN_NUMERALS = 1;

        public const int LOWERCASE_ROMAN_NUMERALS = 2;

        public const int UPPERCASE_LETTERS = 3;

        public const int LOWERCASE_LETTERS = 4;

        public const int EMPTY = 5;

        internal static PdfName[] numberingStyle = new PdfName[5]
        {
            PdfName.D,
            PdfName.R,
            new PdfName("r"),
            PdfName.A,
            new PdfName("a")
        };

        internal Dictionary<int, PdfDictionary> map;

        public PdfPageLabels()
        {
            map = new Dictionary<int, PdfDictionary>();
            AddPageLabel(1, 0, null, 1);
        }

        public virtual void AddPageLabel(int page, int numberStyle, string text, int firstPage)
        {
            if (page < 1 || firstPage < 1)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("in.a.page.label.the.page.numbers.must.be.greater.or.equal.to.1"));
            }

            PdfDictionary pdfDictionary = new PdfDictionary();
            if (numberStyle >= 0 && numberStyle < numberingStyle.Length)
            {
                pdfDictionary.Put(PdfName.S, numberingStyle[numberStyle]);
            }

            if (text != null)
            {
                pdfDictionary.Put(PdfName.P, new PdfString(text, "UnicodeBig"));
            }

            if (firstPage != 1)
            {
                pdfDictionary.Put(PdfName.ST, new PdfNumber(firstPage));
            }

            map[page - 1] = pdfDictionary;
        }

        public virtual void AddPageLabel(int page, int numberStyle, string text)
        {
            AddPageLabel(page, numberStyle, text, 1);
        }

        public virtual void AddPageLabel(int page, int numberStyle)
        {
            AddPageLabel(page, numberStyle, null, 1);
        }

        public virtual void AddPageLabel(PdfPageLabelFormat format)
        {
            AddPageLabel(format.physicalPage, format.numberStyle, format.prefix, format.logicalPage);
        }

        public virtual void RemovePageLabel(int page)
        {
            if (page > 1)
            {
                map.Remove(page - 1);
            }
        }

        public virtual PdfDictionary GetDictionary(PdfWriter writer)
        {
            return PdfNumberTree.WriteTree(map, writer);
        }

        public static string[] GetPageLabels(PdfReader reader)
        {
            int numberOfPages = reader.NumberOfPages;
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.PAGELABELS));
            if (pdfDictionary == null)
            {
                return null;
            }

            string[] array = new string[numberOfPages];
            Dictionary<int, PdfObject> dictionary = PdfNumberTree.ReadTree(pdfDictionary);
            int num = 1;
            string text = "";
            char c = 'D';
            for (int i = 0; i < numberOfPages; i++)
            {
                if (dictionary.ContainsKey(i))
                {
                    PdfDictionary pdfDictionary2 = (PdfDictionary)PdfReader.GetPdfObjectRelease(dictionary[i]);
                    num = ((!pdfDictionary2.Contains(PdfName.ST)) ? 1 : ((PdfNumber)pdfDictionary2.Get(PdfName.ST)).IntValue);
                    if (pdfDictionary2.Contains(PdfName.P))
                    {
                        text = ((PdfString)pdfDictionary2.Get(PdfName.P)).ToUnicodeString();
                    }

                    c = ((!pdfDictionary2.Contains(PdfName.S)) ? 'e' : ((PdfName)pdfDictionary2.Get(PdfName.S)).ToString()[1]);
                }

                switch (c)
                {
                    default:
                        array[i] = text + num;
                        break;
                    case 'R':
                        array[i] = text + RomanNumberFactory.GetUpperCaseString(num);
                        break;
                    case 'r':
                        array[i] = text + RomanNumberFactory.GetLowerCaseString(num);
                        break;
                    case 'A':
                        array[i] = text + RomanAlphabetFactory.GetUpperCaseString(num);
                        break;
                    case 'a':
                        array[i] = text + RomanAlphabetFactory.GetLowerCaseString(num);
                        break;
                    case 'e':
                        array[i] = text;
                        break;
                }

                num++;
            }

            return array;
        }

        public static PdfPageLabelFormat[] GetPageLabelFormats(PdfReader reader)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.PAGELABELS));
            if (pdfDictionary == null)
            {
                return null;
            }

            Dictionary<int, PdfObject> dictionary = PdfNumberTree.ReadTree(pdfDictionary);
            int[] array = new int[dictionary.Count];
            dictionary.Keys.CopyTo(array, 0);
            Array.Sort(array);
            PdfPageLabelFormat[] array2 = new PdfPageLabelFormat[dictionary.Count];
            for (int i = 0; i < array.Length; i++)
            {
                int num = array[i];
                PdfDictionary pdfDictionary2 = (PdfDictionary)PdfReader.GetPdfObjectRelease(dictionary[num]);
                int logicalPage = ((!pdfDictionary2.Contains(PdfName.ST)) ? 1 : ((PdfNumber)pdfDictionary2.Get(PdfName.ST)).IntValue);
                string prefix = ((!pdfDictionary2.Contains(PdfName.P)) ? "" : ((PdfString)pdfDictionary2.Get(PdfName.P)).ToUnicodeString());
                int numberStyle = ((!pdfDictionary2.Contains(PdfName.S)) ? 5 : (((PdfName)pdfDictionary2.Get(PdfName.S)).ToString()[1] switch
                {
                    'R' => 1,
                    'r' => 2,
                    'A' => 3,
                    'a' => 4,
                    _ => 0,
                }));
                array2[i] = new PdfPageLabelFormat(num + 1, numberStyle, prefix, logicalPage);
            }

            return array2;
        }
    }
}
