namespace Sign.itext.text
{
    public interface ILargeElement : IElement
    {
        bool ElementComplete { get; set; }

        void FlushContent();
    }
}
