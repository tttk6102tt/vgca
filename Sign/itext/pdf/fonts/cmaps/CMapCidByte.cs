using Sign.itext.text.pdf;

namespace Sign.itext.pdf.fonts.cmaps
{
    public class CMapCidByte : AbstractCMap
    {
        private Dictionary<int, byte[]> map = new Dictionary<int, byte[]>();

        private static byte[] EMPTY = new byte[0];

        internal override void AddChar(PdfString mark, PdfObject code)
        {
            if (code is PdfNumber)
            {
                byte[] value = AbstractCMap.DecodeStringToByte(mark);
                map[((PdfNumber)code).IntValue] = value;
            }
        }

        public virtual byte[] Lookup(int cid)
        {
            map.TryGetValue(cid, out var value);
            if (value == null)
            {
                return EMPTY;
            }

            return value;
        }
    }
}
