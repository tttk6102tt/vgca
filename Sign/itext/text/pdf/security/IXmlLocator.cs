using System.Xml;

namespace Sign.itext.text.pdf.security
{
    public interface IXmlLocator
    {
        XmlDocument GetDocument();

        void SetDocument(XmlDocument document);

        string GetEncoding();
    }
}
