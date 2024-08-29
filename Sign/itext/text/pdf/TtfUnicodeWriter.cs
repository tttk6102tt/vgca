using Sign.itext.pdf;
using Sign.SystemItext.util.collections;

namespace Sign.itext.text.pdf
{
    public class TtfUnicodeWriter
    {
        protected PdfWriter writer;

        public TtfUnicodeWriter(PdfWriter writer)
        {
            this.writer = writer;
        }

        protected internal virtual void WriteFont(TrueTypeFontUnicode font, PdfIndirectReference refer, object[] parms, byte[] rotbits)
        {
            Dictionary<int, int[]> dictionary = (Dictionary<int, int[]>)parms[0];
            font.AddRangeUni(dictionary, includeMetrics: true, font.Subset);
            int[][] array = new int[dictionary.Count][];
            dictionary.Values.CopyTo(array, 0);
            Array.Sort(array, font);
            PdfIndirectReference pdfIndirectReference = null;
            PdfObject pdfObject = null;
            if (font.Cff)
            {
                byte[] array2 = font.ReadCffFont();
                if (font.Subset || font.SubsetRanges != null)
                {
                    CFFFontSubset cFFFontSubset = new CFFFontSubset(new RandomAccessFileOrArray(array2), dictionary);
                    array2 = cFFFontSubset.Process(cFFFontSubset.GetNames()[0]);
                }

                pdfObject = new BaseFont.StreamFont(array2, "CIDFontType0C", font.CompressionLevel);
                pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
            }
            else
            {
                byte[] array3 = ((!font.Subset && font.DirectoryOffset == 0) ? font.GetFullFont() : font.GetSubSet(new HashSet2<int>(dictionary.Keys), subsetp: true));
                int[] lengths = new int[1] { array3.Length };
                pdfObject = new BaseFont.StreamFont(array3, lengths, font.CompressionLevel);
                pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
            }

            string subsetPrefix = "";
            if (font.Subset)
            {
                subsetPrefix = BaseFont.CreateSubsetPrefix();
            }

            PdfDictionary fontDescriptor = font.GetFontDescriptor(pdfIndirectReference, subsetPrefix, null);
            pdfIndirectReference = writer.AddToBody(fontDescriptor).IndirectReference;
            pdfObject = font.GetCIDFontType2(pdfIndirectReference, subsetPrefix, array);
            pdfIndirectReference = writer.AddToBody(pdfObject).IndirectReference;
            pdfObject = font.GetToUnicode(array);
            PdfIndirectReference toUnicode = null;
            if (pdfObject != null)
            {
                toUnicode = writer.AddToBody(pdfObject).IndirectReference;
            }

            pdfObject = font.GetFontBaseType(pdfIndirectReference, subsetPrefix, toUnicode);
            writer.AddToBody(pdfObject, refer);
        }
    }
}
