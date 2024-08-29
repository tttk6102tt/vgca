namespace Sign.itext.xml.xmp.properties
{
    public interface IXmpPropertyInfo : IXmpProperty
    {
        string Namespace { get; }

        string Path { get; }
    }
}
