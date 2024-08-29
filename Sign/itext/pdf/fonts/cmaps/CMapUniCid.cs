using Sign.itext.text;
using Sign.itext.text.pdf;

namespace Sign.itext.pdf.fonts.cmaps
{
    public class CMapUniCid : AbstractCMap
    {
        private IntHashtable map = new IntHashtable(65537);

        internal override void AddChar(PdfString mark, PdfObject code)
        {
            if (code is PdfNumber)
            {
                string text = DecodeStringToUnicode(mark);
                int key = ((!Utilities.IsSurrogatePair(text, 0)) ? text[0] : Utilities.ConvertToUtf32(text, 0));
                map[key] = ((PdfNumber)code).IntValue;
            }
        }

        public virtual int Lookup(int character)
        {
            return map[character];
        }

        public virtual CMapToUnicode ExportToUnicode()
        {
            CMapToUnicode cMapToUnicode = new CMapToUnicode();
            int[] keys = map.GetKeys();
            foreach (int num in keys)
            {
                cMapToUnicode.AddChar(map[num], Utilities.ConvertFromUtf32(num));
            }

            return cMapToUnicode;
        }
    }
}
