using Sign.itext.text.pdf;

namespace Sign.itext.text
{
    public class TabSplitCharacter : ISplitCharacter
    {
        public static readonly ISplitCharacter TAB = new TabSplitCharacter();

        public virtual bool IsSplitCharacter(int start, int current, int end, char[] cc, PdfChunk[] ck)
        {
            return true;
        }
    }
}
