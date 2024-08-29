using Sign.itext.xml.xmp.options;

namespace Sign.itext.xml.xmp.properties
{
    public interface IXmpProperty
    {
        string Value { get; }

        PropertyOptions Options { get; }

        string Language { get; }
    }
}
