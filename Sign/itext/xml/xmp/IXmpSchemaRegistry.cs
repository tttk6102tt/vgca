using Sign.itext.xml.xmp.properties;
using System.Collections;

namespace Sign.itext.xml.xmp
{
    public interface IXmpSchemaRegistry
    {
        string RegisterNamespace(string namespaceUri, string suggestedPrefix);

        string GetNamespacePrefix(string namespaceUri);

        string GetNamespaceUri(string namespacePrefix);

        IDictionary GetNamespaces();

        IDictionary GetPrefixes();

        void DeleteNamespace(string namespaceUri);

        IXmpAliasInfo ResolveAlias(string aliasNs, string aliasProp);

        IXmpAliasInfo[] FindAliases(string aliasNs);

        IXmpAliasInfo FindAlias(string qname);

        IDictionary GetAliases();
    }
}
