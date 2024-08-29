using Sign.itext.error_messages;
using Sign.itext.text.pdf;
using System.Text;

namespace Sign.itext.pdf.fonts.cmaps
{
    public class CMapByteCid : AbstractCMap
    {
        private List<char[]> planes = new List<char[]>();

        public CMapByteCid()
        {
            planes.Add(new char[256]);
        }

        internal override void AddChar(PdfString mark, PdfObject code)
        {
            if (code is PdfNumber)
            {
                EncodeSequence(AbstractCMap.DecodeStringToByte(mark), (char)((PdfNumber)code).IntValue);
            }
        }

        private void EncodeSequence(byte[] seqs, char cid)
        {
            int num = seqs.Length - 1;
            int index = 0;
            char[] array;
            int num2;
            for (int i = 0; i < num; i++)
            {
                array = planes[index];
                num2 = seqs[i] & 0xFF;
                char c = array[num2];
                if (c != 0 && (c & 0x8000) == 0)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("inconsistent.mapping"));
                }

                if (c == '\0')
                {
                    planes.Add(new char[256]);
                    c = (array[num2] = (char)((uint)(planes.Count - 1) | 0x8000u));
                }

                index = c & 0x7FFF;
            }

            array = planes[index];
            num2 = seqs[num] & 0xFF;
            if ((array[num2] & 0x8000u) != 0)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("inconsistent.mapping"));
            }

            array[num2] = cid;
        }

        public virtual int DecodeSingle(CMapSequence seq)
        {
            int num = seq.off + seq.len;
            int index = 0;
            while (seq.off < num)
            {
                int num2 = seq.seq[seq.off++] & 0xFF;
                seq.len--;
                int num3 = planes[index][num2];
                if ((num3 & 0x8000) == 0)
                {
                    return num3;
                }

                index = num3 & 0x7FFF;
            }

            return -1;
        }

        public virtual string DecodeSequence(CMapSequence seq)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            while ((num = DecodeSingle(seq)) >= 0)
            {
                stringBuilder.Append((char)num);
            }

            return stringBuilder.ToString();
        }
    }
}
