using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfNameTree
    {
        internal class CompareSrt : IComparer<string>
        {
            public virtual int Compare(string x, string y)
            {
                char[] array = x.ToCharArray();
                char[] array2 = y.ToCharArray();
                int num = Math.Min(array.Length, array2.Length);
                for (int i = 0; i < num; i++)
                {
                    if (array[i] < array2[i])
                    {
                        return -1;
                    }

                    if (array[i] > array2[i])
                    {
                        return 1;
                    }
                }

                if (array.Length < array2.Length)
                {
                    return -1;
                }

                if (array.Length > array2.Length)
                {
                    return 1;
                }

                return 0;
            }
        }

        private const int leafSize = 64;

        public static PdfDictionary WriteTree<T>(Dictionary<string, T> items, PdfWriter writer) where T : PdfObject
        {
            if (items.Count == 0)
            {
                return null;
            }

            string[] array = new string[items.Count];
            items.Keys.CopyTo(array, 0);
            Array.Sort(array, new CompareSrt());
            if (array.Length <= 64)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                PdfArray pdfArray = new PdfArray();
                for (int i = 0; i < array.Length; i++)
                {
                    pdfArray.Add(new PdfString(array[i], null));
                    pdfArray.Add(items[array[i]]);
                }

                pdfDictionary.Put(PdfName.NAMES, pdfArray);
                return pdfDictionary;
            }

            int num = 64;
            PdfIndirectReference[] array2 = new PdfIndirectReference[(array.Length + 64 - 1) / 64];
            for (int j = 0; j < array2.Length; j++)
            {
                int k = j * 64;
                int num2 = Math.Min(k + 64, array.Length);
                PdfDictionary pdfDictionary2 = new PdfDictionary();
                PdfArray pdfArray2 = new PdfArray();
                pdfArray2.Add(new PdfString(array[k], null));
                pdfArray2.Add(new PdfString(array[num2 - 1], null));
                pdfDictionary2.Put(PdfName.LIMITS, pdfArray2);
                pdfArray2 = new PdfArray();
                for (; k < num2; k++)
                {
                    pdfArray2.Add(new PdfString(array[k], null));
                    pdfArray2.Add(items[array[k]]);
                }

                pdfDictionary2.Put(PdfName.NAMES, pdfArray2);
                array2[j] = writer.AddToBody(pdfDictionary2).IndirectReference;
            }

            int num3 = array2.Length;
            while (num3 > 64)
            {
                num *= 64;
                int num4 = (array.Length + num - 1) / num;
                for (int l = 0; l < num4; l++)
                {
                    int m = l * 64;
                    int num5 = Math.Min(m + 64, num3);
                    PdfDictionary pdfDictionary3 = new PdfDictionary();
                    PdfArray pdfArray3 = new PdfArray();
                    pdfArray3.Add(new PdfString(array[l * num], null));
                    pdfArray3.Add(new PdfString(array[Math.Min((l + 1) * num, array.Length) - 1], null));
                    pdfDictionary3.Put(PdfName.LIMITS, pdfArray3);
                    pdfArray3 = new PdfArray();
                    for (; m < num5; m++)
                    {
                        pdfArray3.Add(array2[m]);
                    }

                    pdfDictionary3.Put(PdfName.KIDS, pdfArray3);
                    array2[l] = writer.AddToBody(pdfDictionary3).IndirectReference;
                }

                num3 = num4;
            }

            PdfArray pdfArray4 = new PdfArray();
            for (int n = 0; n < num3; n++)
            {
                pdfArray4.Add(array2[n]);
            }

            PdfDictionary pdfDictionary4 = new PdfDictionary();
            pdfDictionary4.Put(PdfName.KIDS, pdfArray4);
            return pdfDictionary4;
        }

        private static PdfString IterateItems(PdfDictionary dic, Dictionary<string, PdfObject> items, PdfString leftOverString)
        {
            PdfArray pdfArray = (PdfArray)PdfReader.GetPdfObjectRelease(dic.Get(PdfName.NAMES));
            if (pdfArray != null)
            {
                for (int i = 0; i < pdfArray.Size; i++)
                {
                    PdfString pdfString;
                    if (leftOverString == null)
                    {
                        pdfString = (PdfString)PdfReader.GetPdfObjectRelease(pdfArray.GetPdfObject(i++));
                    }
                    else
                    {
                        pdfString = leftOverString;
                        leftOverString = null;
                    }

                    if (i < pdfArray.Size)
                    {
                        items[PdfEncodings.ConvertToString(pdfString.GetBytes(), null)] = pdfArray.GetPdfObject(i);
                        continue;
                    }

                    return pdfString;
                }
            }
            else if ((pdfArray = (PdfArray)PdfReader.GetPdfObjectRelease(dic.Get(PdfName.KIDS))) != null)
            {
                for (int j = 0; j < pdfArray.Size; j++)
                {
                    leftOverString = IterateItems((PdfDictionary)PdfReader.GetPdfObjectRelease(pdfArray.GetPdfObject(j)), items, leftOverString);
                }
            }

            return null;
        }

        public static Dictionary<string, PdfObject> ReadTree(PdfDictionary dic)
        {
            Dictionary<string, PdfObject> dictionary = new Dictionary<string, PdfObject>();
            if (dic != null)
            {
                IterateItems(dic, dictionary, null);
            }

            return dictionary;
        }
    }
}
