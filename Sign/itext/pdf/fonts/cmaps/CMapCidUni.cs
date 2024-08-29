using Sign.itext.text;
using Sign.itext.text.pdf;

namespace Sign.itext.pdf.fonts.cmaps
{
    public class CMapCidUni : AbstractCMap
    {
        private IntHashtable map = new IntHashtable(65537);

        internal override void AddChar(PdfString mark, PdfObject code)
        {
            if (code is PdfNumber)
            {
                string text = DecodeStringToUnicode(mark);
                int value = ((!Utilities.IsSurrogatePair(text, 0)) ? text[0] : Utilities.ConvertToUtf32(text, 0));
                map[((PdfNumber)code).IntValue] = value;
            }
        }

        public virtual int Lookup(int character)
        {
            return map[character];
        }
    }
}
