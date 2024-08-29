using System.Xml;

namespace Sign.itext.text.pdf.security
{
    public interface IXpathConstructor
    {
        string GetXpathExpression();

        XmlNamespaceManager GetNamespaceManager();
    }
}
