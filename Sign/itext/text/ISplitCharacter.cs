using Sign.itext.text.pdf;

namespace Sign.itext.text
{
    public interface ISplitCharacter
    {
        bool IsSplitCharacter(int start, int current, int end, char[] cc, PdfChunk[] ck);
    }
}
