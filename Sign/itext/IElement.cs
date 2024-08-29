namespace Sign.itext
{
    public interface IElement
    {
        int Type { get; }

        IList<Chunk> Chunks { get; }

        bool Process(IElementListener listener);

        bool IsContent();

        bool IsNestable();

        new string ToString();
    }
}
