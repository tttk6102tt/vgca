namespace Sign.itext
{
    public interface IFontProvider
    {
        bool IsRegistered(string fontname);

        Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color);
    }
}
