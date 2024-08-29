namespace Sign.itext.xml.simpleparser
{
    public interface ISimpleXMLDocHandler
    {
        void StartElement(string tag, IDictionary<string, string> h);

        void EndElement(string tag);

        void StartDocument();

        void EndDocument();

        void Text(string str);
    }
}
