using Sign.itext.xml.xmp.options;

namespace Sign.itext.xml.xmp.properties
{
    public interface IXmpAliasInfo
    {
        string Namespace { get; }

        string Prefix { get; }

        string PropName { get; }

        AliasOptions AliasForm { get; }
    }
}
