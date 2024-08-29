using Sign.itext.pdf;

namespace Sign.itext.text
{
    public interface IHyphenationEvent
    {
        string HyphenSymbol { get; }

        string HyphenatedWordPost { get; }

        string GetHyphenatedWordPre(string word, BaseFont font, float fontSize, float remainingWidth);
    }
}
